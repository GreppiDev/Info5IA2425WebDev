#!/usr/bin/env bash
set -euo pipefail

SOCKET_PATH="/var/run/mysqld/mysqld.sock"
DATA_DIR="/var/lib/mysql"
INIT_MARKER="${DATA_DIR}/.devcontainer-initialized"

# Obiettivo dello script:
# - avviare MariaDB nel devcontainer (senza systemd)
# - inizializzare i dati la prima volta
# - creare un DB + utente applicativo da variabili d'ambiente
# - (opzionale) impostare la password dell'utente root di MariaDB

mkdir -p /var/run/mysqld
chown -R mysql:mysql /var/run/mysqld

already_running=false
if [ -S "${SOCKET_PATH}" ] && mariadb-admin --protocol=socket --socket="${SOCKET_PATH}" ping >/dev/null 2>&1; then
  already_running=true
fi

if [ ! -d "${DATA_DIR}/mysql" ]; then
  # Prima inizializzazione del data directory (crea le tabelle di sistema di MariaDB)
  chown -R mysql:mysql "${DATA_DIR}"
  mariadb-install-db --user=mysql --datadir="${DATA_DIR}" >/dev/null
fi

if [ "${already_running}" != "true" ]; then
  # Start MariaDB in background
  mariadbd \
    --user=mysql \
    --datadir="${DATA_DIR}" \
    --bind-address=0.0.0.0 \
    --port=3306 \
    --socket="${SOCKET_PATH}" \
    --skip-name-resolve \
    &

  # Wait for server
  for _ in {1..40}; do
    if mariadb-admin --protocol=socket --socket="${SOCKET_PATH}" ping >/dev/null 2>&1; then
      break
    fi
    sleep 0.5
  done

  if ! mariadb-admin --protocol=socket --socket="${SOCKET_PATH}" ping >/dev/null 2>&1; then
    echo "MariaDB did not start correctly" >&2
    exit 1
  fi
fi

# Questo riportato di seguito è un esempio che crea un database e un utente con privilegi su quel database, usando variabili d'ambiente per i valori.
# Si tratta di una semplificazione pensata per lo sviluppo locale, non è adatta a un ambiente di produzione

# One-time initialization for local dev
if [ ! -f "${INIT_MARKER}" ]; then
  # Questa sezione gira UNA SOLA VOLTA.
  # Il marker `${INIT_MARKER}` evita di ricreare DB/utenti ad ogni riavvio.
  #
  # Importante: questo script NON legge/sourcia automaticamente `.env`.
  # Le variabili devono essere già presenti nell'ambiente del processo che lo esegue.
  # Nel nostro devcontainer succede perché `devcontainer.json` usa `runArgs --env-file .env`
  # e perché avviamo lo script con `sudo -E` (che preserva le variabili d'ambiente).

  # Queste variabili possono essere passate dal devcontainer (es. via `.env`)
  MARIADB_DATABASE="${MARIADB_DATABASE:-devdb}"
  MARIADB_USER="${MARIADB_USER:-devuser}"
  MARIADB_PASSWORD="${MARIADB_PASSWORD:-devpass}"
  MARIADB_ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:-}"

  # Se `.env` è stato creato/modificato su Windows, può avere CRLF.
  # Quando viene `source`-ato in Bash, il carattere `\r` può finire nel valore (es. "devdb\r"),
  # causando errori MariaDB tipo "Incorrect database name". Qui lo rimuoviamo.
  MARIADB_DATABASE="${MARIADB_DATABASE//$'\r'/}"
  MARIADB_USER="${MARIADB_USER//$'\r'/}"
  MARIADB_PASSWORD="${MARIADB_PASSWORD//$'\r'/}"
  MARIADB_ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD//$'\r'/}"

  # Semplificazione (didattica): validiamo nome DB e utente con un set di caratteri "sicuro".
  # Questo evita problemi di quoting e rende lo script più facile da leggere.
  if ! [[ "${MARIADB_DATABASE}" =~ ^[A-Za-z0-9_]+$ ]]; then
    echo "Invalid MARIADB_DATABASE '${MARIADB_DATABASE}'. Use only letters, numbers and underscore." >&2
    exit 1
  fi
  if ! [[ "${MARIADB_USER}" =~ ^[A-Za-z0-9_]+$ ]]; then
    echo "Invalid MARIADB_USER '${MARIADB_USER}'. Use only letters, numbers and underscore." >&2
    exit 1
  fi

  # Escape minimo per valori in apici singoli (password possono contenere apostrofi).
  # Importante: NON stiamo cambiando la password dell'utente.
  # Stiamo solo creando una *rappresentazione escapata* da usare dentro una query SQL tra apici singoli.
  # Esempio: pa'ss -> pa''ss (in SQL un apostrofo si rappresenta raddoppiandolo).
  mariadb_password_sql_escaped="${MARIADB_PASSWORD//\'/\'\'}"

  # --- MariaDB root auth (via socket) ---
  # Per fare CREATE DATABASE / CREATE USER / GRANT abbiamo bisogno di un account con privilegi.
  # Qui usiamo `root` via *Unix socket* (non TCP), perché nel devcontainer il DB gira localmente.
  # Questo significa che qui stiamo autenticando come `root@localhost` (socket), NON come `root@127.0.0.1` (TCP).
  #
  # In MariaDB gli account sono distinti per `User`+`Host` (es. `root@localhost` vs `root@127.0.0.1`).
  # Questo script non crea automaticamente `root@127.0.0.1`: gestisce solo l'init locale via socket.
  #
  # --- "CLI riutilizzabile" (pattern Bash con array) ---
  # In Bash, un modo robusto per "costruire" un comando una volta sola e riusarlo più volte
  # è metterlo in un array. Ogni elemento dell'array è un argomento separato del comando.
  #
  # Esempio: `mariadb_root_cli=(mariadb ... -uroot)` crea il comando base;
  # poi possiamo aggiungere opzioni condizionali (es. la password) con `+=()`.
  #
  # Quando lo eseguiamo con "${mariadb_root_cli[@]}", Bash espande l'array in più argomenti.
  # Questo evita problemi comuni con spazi/quoting e ci garantisce che TUTTE le query usino
  # lo stesso protocollo e lo stesso socket.
  #
  # Nota: "${array[@]}" mantiene gli elementi separati (corretto per eseguire un comando).
  # "${array[*]}" invece tende a concatenare gli elementi in una singola stringa.
  #
  # Per evitare errori ripetuti, costruiamo un comando CLI riutilizzabile in un array:
  # - garantisce che ogni chiamata usi lo stesso protocollo/socket
  # - aggiunge `-p...` solo se serve (e senza prompt interattivo)
  # Nota: con il client MariaDB, `-pPASSWORD` (senza spazio) evita il prompt interattivo.
  mariadb_root_cli=(mariadb --protocol=socket --socket="${SOCKET_PATH}" -uroot)
  if [ -n "${MARIADB_ROOT_PASSWORD}" ]; then
    mariadb_root_cli+=("-p${MARIADB_ROOT_PASSWORD}")
  fi

  # Sanity check: verifichiamo che le credenziali root funzionino prima di eseguire le query di init.
  if ! "${mariadb_root_cli[@]}" -e "SELECT 1;" >/dev/null 2>&1; then
    if [ -z "${MARIADB_ROOT_PASSWORD}" ]; then
      echo "Cannot authenticate as MariaDB root via socket without a password." >&2
      echo "If root has a password set, export MARIADB_ROOT_PASSWORD (e.g. in .env) and rerun." >&2
      exit 1
    fi
    echo "Cannot authenticate as MariaDB root via socket (check MARIADB_ROOT_PASSWORD)." >&2
    exit 1
  fi

  # Nota: usiamo \` per evitare che Bash interpreti i backtick come command substitution.
  # Creiamo:
  # - il database (se non esiste)
  # - l'utente applicativo `${MARIADB_USER}`@'%' (se non esiste)
  # - i privilegi sul solo database `${MARIADB_DATABASE}`
  # In ottica didattica, l'app dovrebbe usare questo utente applicativo (non `root`).
  "${mariadb_root_cli[@]}" -e "CREATE DATABASE IF NOT EXISTS \`${MARIADB_DATABASE}\`; CREATE USER IF NOT EXISTS '${MARIADB_USER}'@'%' IDENTIFIED BY '${mariadb_password_sql_escaped}'; GRANT ALL PRIVILEGES ON \`${MARIADB_DATABASE}\`.* TO '${MARIADB_USER}'@'%'; FLUSH PRIVILEGES;"

  if [ -n "${MARIADB_ROOT_PASSWORD}" ]; then
    # Anche qui: non cambiamo la variabile originale, creiamo solo una versione escapata per SQL.
    mariadb_root_password_sql_escaped="${MARIADB_ROOT_PASSWORD//\'/\'\'}"
    # Se richiesto, imposta la password di root del DB.
    # Nota: in MariaDB possono esistere più entry per root (es. root@localhost, root@127.0.0.1, ...)
    # quindi iteriamo su tutte le righe presenti in mysql.user.
    while IFS=$'\t' read -r root_host; do
      [ -n "${root_host}" ] || continue
      "${mariadb_root_cli[@]}" \
        -e "ALTER USER 'root'@'${root_host}' IDENTIFIED BY '${mariadb_root_password_sql_escaped}';"
    done < <("${mariadb_root_cli[@]}" -N -B -e "SELECT Host FROM mysql.user WHERE User='root';")
    "${mariadb_root_cli[@]}" -e "FLUSH PRIVILEGES;"
  fi

  # Marker: evita di ripetere init e reset password ad ogni riavvio del devcontainer.
  touch "${INIT_MARKER}"
fi

# `exec "$@"` sostituisce il processo corrente (questo script Bash) con il comando
# passato come argomenti. È un pattern tipico negli entrypoint perché:
# - non lascia un processo "wrapper" in più (niente processo figlio da attendere)
# - il comando finale riceve direttamente i segnali (SIGTERM, ecc.)
# - l'exit code del comando finale diventa l'exit code dello script
#
# In questo repository lo script viene lanciato dal devcontainer con:
#   sudo -E bash .devcontainer/mariadb-entrypoint.sh true
# quindi qui `"$@"` è `true` e il risultato è `exec true`:
# - `true` non fa nulla e termina con exit code 0
# - lo scopo è chiudere lo script "pulito" dopo aver avviato/inizializzato MariaDB
# (in questo caso sarebbe equivalente anche fare `exit 0`).
exec "$@"
