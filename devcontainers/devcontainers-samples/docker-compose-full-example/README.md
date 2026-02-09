# MyMinimalApi (Dev Container + MariaDB + rete/volumi in Docker Compose)

Esempio di **ASP.NET Core Minimal API** sviluppata in **Dev Container**.

Questo esempio crea tutto il necessario **da zero** tramite `docker compose` (lanciato da Dev Containers):

- container applicativo (devcontainer)
- container MariaDB
- rete Docker bridge `my-net`
- volume persistente per i dati del DB

Le credenziali e i parametri di connessione sono definiti in un file `.env` e vengono:

- caricati nel container applicativo tramite `docker compose` (`env_file`)
- letti dall’app ASP.NET tramite configuration provider delle variabili d’ambiente
- usati per costruire la connection string a runtime in `Program.cs`

## Indice

- [MyMinimalApi (Dev Container + MariaDB + rete/volumi in Docker Compose)](#myminimalapi-dev-container--mariadb--retevolumi-in-docker-compose)
  - [Indice](#indice)
  - [🚀 Quick Start](#-quick-start)
    - [Prerequisiti](#prerequisiti)
    - [Avvio (passi consigliati)](#avvio-passi-consigliati)
  - [🧠 Come funziona (in breve)](#-come-funziona-in-breve)
  - [🗄️ Database in Compose: rete `my-net`, volume e naming](#️-database-in-compose-rete-my-net-volume-e-naming)
    - [Perché usare ancora `my-net`?](#perché-usare-ancora-my-net)
    - [Hostname del DB (`MARIADB_HOST`)](#hostname-del-db-mariadb_host)
    - [Provisioning "production-like" (schema/seed/grants)](#provisioning-production-like-schemaseedgrants)
      - [Casi d’uso (DEV/PROD) con esempi pronti](#casi-duso-devprod-con-esempi-pronti)
      - [Riferimento: variabili di permesso (cosa fanno davvero)](#riferimento-variabili-di-permesso-cosa-fanno-davvero)
      - [Come verificare i grants (debug veloce)](#come-verificare-i-grants-debug-veloce)
      - [Spiegazione Bash (per capire gli script)](#spiegazione-bash-per-capire-gli-script)
  - [🔐 Variabili d’ambiente: `.env` e `.env.example`](#-variabili-dambiente-env-e-envexample)
    - [`.env`](#env)
    - [`.env.example`](#envexample)
    - [Nota su Windows (CRLF)](#nota-su-windows-crlf)
  - [🧩 Come la Minimal API usa le variabili](#-come-la-minimal-api-usa-le-variabili)
  - [📚 File di contorno: `.gitignore` e `.dockerignore`](#-file-di-contorno-gitignore-e-dockerignore)
    - [`.gitignore`](#gitignore)
    - [`.dockerignore`](#dockerignore)
    - [`.gitattributes`](#gitattributes)
  - [🧪 Verifiche rapide](#-verifiche-rapide)
    - [Verificare che le variabili siano visibili nel container](#verificare-che-le-variabili-siano-visibili-nel-container)
    - [Verificare che l’app parta](#verificare-che-lapp-parta)
  - [🔌 Collegarsi al DB dall'host](#-collegarsi-al-db-dallhost)
  - [🛠️ Troubleshooting](#️-troubleshooting)
    - [La rete `my-net` non esiste](#la-rete-my-net-non-esiste)
    - [Il DB non è raggiungibile dal devcontainer](#il-db-non-è-raggiungibile-dal-devcontainer)
    - [Credenziali errate / Access denied](#credenziali-errate--access-denied)
    - [E' stato modificato `.env` ma l’app non vede i cambiamenti](#e-stato-modificato-env-ma-lapp-non-vede-i-cambiamenti)
  - [🧯 Trovare PID e fermare processi (Linux / Dev Container)](#-trovare-pid-e-fermare-processi-linux--dev-container)
  - [🤖 AI Assistants: Claude Code e OpenCode](#-ai-assistants-claude-code-e-opencode)
    - [Panoramica](#panoramica)
    - [Mount della configurazione (host → container)](#mount-della-configurazione-host--container)
    - [`postCreateCommand`: restore + init automatico](#postcreatecommand-restore--init-automatico)
    - [Cosa fanno gli script di init](#cosa-fanno-gli-script-di-init)
    - [Aggiornare la configurazione (senza rebuild completo)](#aggiornare-la-configurazione-senza-rebuild-completo)
    - [Sicurezza (token e API key)](#sicurezza-token-e-api-key)
  - [🌐 Dev Containers a scuola (proxy)](#-dev-containers-a-scuola-proxy)
    - [Scenario](#scenario)
    - [Avvio corretto di VS Code su Windows (PowerShell)](#avvio-corretto-di-vs-code-su-windows-powershell)
    - [Perché `http.proxySupport` è impostato su `off`](#perché-httpproxysupport-è-impostato-su-off)
    - [Problemi tipici e fix rapidi](#problemi-tipici-e-fix-rapidi)
  - [🐛 Debug Node: perché `debug.javascript.autoAttachFilter` è `disabled`](#-debug-node-perché-debugjavascriptautoattachfilter-è-disabled)

## 🚀 Quick Start

### Prerequisiti

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/) con estensione [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Avvio (passi consigliati)

1) **Aprire la cartella in VS Code**

   - File → Open Folder… → seleziona `docker-compose-full-example`

2) **Configurare le variabili locali (`.env` (locale) partendo da `.env.example`)**

   Questo repo include un template committabile `.env.example` e un file `.env` ignorato da Git.
   Se si vuole ripartire “puliti”, si può usare questo workflow:

   ```bash
   cp .env.example .env
   ```

   Poi modificare le variabili d’ambiente e salvare il file `.env`.

   Best practice: l’app non deve usare `root` (usa `MARIADB_USER` / `MARIADB_PASSWORD`).

3) **Aprire in Dev Container**

   - Se appare il popup “Reopen in Container”, cliccalo.
   - Oppure: `F1` → “Dev Containers: Reopen in Container”.

   La prima volta VS Code costruisce l’immagine, avvia il container ed esegue `dotnet restore` nella cartella del progetto.

4) **Avviare l’app**

   Nel terminale del devcontainer:

   ```bash
   dotnet run --project src/MyApi/MyApi.csproj
   ```

   In Development, l’endpoint `/` reindirizza a Swagger UI.
   Con port forwarding attivo, apri:

   - `http://localhost:5000/swagger`
   - `https://localhost:5001/swagger`

## 🧠 Come funziona (in breve)

Questo progetto usa Dev Containers con `docker compose` per avviare il servizio applicativo.

- Il devcontainer è definito in `.devcontainer/devcontainer.json`.
- L’orchestrazione avviene con `.devcontainer/docker-compose.yml`.
- `docker compose` avvia `app` e `mariadb`, crea la rete `my-net` e il volume del DB.
- Le variabili in `.env` vengono iniettate nei container tramite `env_file`.

Risultato: l’app parla con MariaDB sulla rete Docker senza dover esporre porte del DB verso l’host.

## 🗄️ Database in Compose: rete `my-net`, volume e naming

### Perché usare ancora `my-net`?

Si può continuare a usare `my-net` anche se ora la rete viene creata da Compose:

- mantiene coerente l’esempio e i nomi
- rende esplicita la topologia app↔db

Nel file `.devcontainer/docker-compose.yml` la rete è dichiarata con nome fisso:

```yaml
networks:
   my-net:
      driver: bridge
      # (opzionale) nome globale fisso, sconsigliato se si hanno piu' esempi/stack in parallelo
      # name: my-net
```

Questo significa che Docker Compose crea/gestisce la rete. Se non si specifica `name:`, Compose userà un nome isolato per-progetto (evita collisioni tra stack diversi).

### Hostname del DB (`MARIADB_HOST`)

Dal container applicativo, il DB si raggiunge tramite:

- **nome servizio Compose** (`mariadb`)

   Nel template `.env.example` il valore di default è:

   ```dotenv
   MARIADB_HOST=mariadb
   ```

Se il proprio container DB ha un nome diverso, cambiare `MARIADB_HOST`.

Nota: `localhost` dentro il devcontainer significa “il devcontainer stesso”, non MariaDB.

### Provisioning "production-like" (schema/seed/grants)

Il container MariaDB monta degli script in `/docker-entrypoint-initdb.d` da:

- `.devcontainer/db-init/`

Questi script vengono eseguiti automaticamente dall'immagine ufficiale MariaDB **solo al primo avvio**, cioe' quando il volume dati (`/var/lib/mysql`) e' vuoto.

In piu', per scopi didattici, esiste un servizio one-shot che permette di rieseguirli anche dopo (su DB gia' avviato): `db-provision`.

Contenuto (idempotente):

- creazione tabella `Pizzas`
- seed iniziale (3 record) tramite script
- grants per l'utente applicativo (non-root) tramite script `.sh` che usa `MARIADB_DATABASE` / `MARIADB_USER`

Nota sul seed (EF `HasData` vs script):

- In questo esempio i dati iniziali vengono inseriti da [.devcontainer/db-init/02-seed.sh](.devcontainer/db-init/02-seed.sh) con un approccio idempotente (`ON DUPLICATE KEY UPDATE`).
- Lo script fa anche `USE <db>` esplicitamente, così funziona uguale sia nel bootstrap (initdb) sia quando lo rilanci via `db-provision` (TCP).
- Se non si vuole inserire dati demo, si può disabilitare il seed impostando `MARIADB_ENABLE_SEED=0` nel proprio `.env` (poi si riesegue `db-provision` oppure si riparte con `down -v`).
- Per evitare duplicati o migrazioni che falliscono quando i record esistono gia' (tipico quando si riesegue `db-provision`), il seed via EF Core `HasData(...)` è stato rimosso da [src/MyApi/Data/PizzaDb.cs](src/MyApi/Data/PizzaDb.cs).
- Se si vuole gestire seed/dati iniziali tramite migrations EF, occorre fare l’opposto: ripristinare `HasData` e disattivare/rimuovere il seed nello script.

Permessi e migrazioni:

- default: permessi "least-privilege" (CRUD + DDL base) sul solo database applicativo
- se in sviluppo si vuole eseguire EF migrations, impostare `MARIADB_GRANT_ALL_ON_DB=1` per ottenere `ALL PRIVILEGES` (in base allo scope)

Dev vs produzione (scelta pratica):

- **DEV ("si può fare tutto")**: `MARIADB_GRANT_SCOPE=server` (grants su `*.*`) e, se si vuole anche poter fare grant ad altri utenti, `MARIADB_GRANT_WITH_GRANT_OPTION=1`.
- **PROD (restrittivo)**: `MARIADB_GRANT_SCOPE=database` e `MARIADB_GRANT_ALL_ON_DB=0` (least-privilege su `<db>.*`).

#### Casi d’uso (DEV/PROD) con esempi pronti

Qui sotto sono riportati alcuni profili tipici. L’idea è:

- in **DEV** massima libertà per imparare/sperimentare
- in **PROD** permessi minimi, confinati al singolo DB applicativo

Nota importante: cambiare `.env` non modifica automaticamente i grants già applicati. Dopo ogni cambio di permessi, eseguire:

- re-provision (consigliato): `docker compose -f .devcontainer/docker-compose.yml --profile tools run --rm db-provision`
- oppure reset completo: `docker compose -f .devcontainer/docker-compose.yml down -v`

Caso A — DEV superuser (tutti i database, "come root ma senza usare root")

Quando usarlo:

- demo didattiche
- si vuole poter creare/eliminare DB liberamente
- si vuole evitare qualsiasi blocco di permessi durante le migrazioni

`.env`:

```dotenv
MARIADB_GRANT_SCOPE=server
MARIADB_GRANT_ALL_ON_DB=1

# facoltativo (molto permissivo): consente anche di fare GRANT ad altri utenti
MARIADB_GRANT_WITH_GRANT_OPTION=1
```

Effetto:

- `GRANT ALL PRIVILEGES ON *.*` all’utente applicativo (più eventuale `WITH GRANT OPTION`)

Caso B — DEV “migrations friendly” ma confinato al DB applicativo

Quando usarlo:

- si fanno migrazioni EF e si vogliono pochi problemi
- ma non si vogliono concedere `*.*`

`.env`:

```dotenv
MARIADB_GRANT_SCOPE=database
MARIADB_GRANT_ALL_ON_DB=1
MARIADB_GRANT_WITH_GRANT_OPTION=0
```

Effetto:

- `GRANT ALL PRIVILEGES ON <db>.*` all’utente applicativo

Caso C — PROD (least-privilege, confinato al DB applicativo)

Quando usarlo:

- produzione
- staging/QA dove si vogliono intercettare richieste di permessi “in più”

`.env`:

```dotenv
MARIADB_GRANT_SCOPE=database
MARIADB_GRANT_ALL_ON_DB=0
MARIADB_GRANT_WITH_GRANT_OPTION=0
```

Effetto:

- permessi CRUD + DDL base su `<db>.*` (nessun `*.*`)

Caso D — Workflow pratico con `.env.dev` e `.env.prod`

Si possono tenere due file locali (da non committare) e copiare quello attivo in `.env`.

Esempio:

1) creare due file locali:

   - `.env.dev`
   - `.env.prod`

2) per passare a DEV:

   ```bash
   cp .env.dev .env
   docker compose -f .devcontainer/docker-compose.yml --profile tools run --rm db-provision
   ```

3) per passare a PROD-like:

   ```bash
   cp .env.prod .env
   docker compose -f .devcontainer/docker-compose.yml --profile tools run --rm db-provision
   ```

#### Riferimento: variabili di permesso (cosa fanno davvero)

Gli script applicano i grants leggendo queste variabili:

- `MARIADB_GRANT_SCOPE`
   - `database` (default “prod-like”): i grants vengono applicati su `<db>.*`
   - `server` (dev superuser): i grants vengono applicati su `*.*`
- `MARIADB_GRANT_ALL_ON_DB`
   - `0`: modalità least-privilege (CRUD + DDL base) sul target scelto da `MARIADB_GRANT_SCOPE`
   - `1`: `ALL PRIVILEGES` sul target scelto da `MARIADB_GRANT_SCOPE`
- `MARIADB_GRANT_WITH_GRANT_OPTION`
   - `0`: non aggiunge `WITH GRANT OPTION`
   - `1`: aggiunge `WITH GRANT OPTION` (molto permissivo: consente all’utente applicativo di concedere permessi ad altri utenti)

Nota di sicurezza:

- `MARIADB_GRANT_SCOPE=server` e/o `MARIADB_GRANT_WITH_GRANT_OPTION=1` sono comodi in sviluppo, ma in produzione aumentano molto l’impatto di un bug o di una compromissione dell’app.

#### Come verificare i grants (debug veloce)

Da host (o dentro il devcontainer), puoi ispezionare i permessi con:

```bash
docker compose -f .devcontainer/docker-compose.yml exec mariadb \
   mariadb -uroot -p$MARIADB_ROOT_PASSWORD -e "SHOW GRANTS FOR '$MARIADB_USER'@'%';"
```

Se si sono cambiati i valori in `.env` ma non si vedono differenze qui, significa che bisogna rieseguire `db-provision` (oppure fare `down -v`).

#### Spiegazione Bash (per capire gli script)

Gli script in `.devcontainer/db-init/*.sh` usano alcuni costrutti Bash ricorrenti.

`mariadb_root()`

- È una funzione helper che “sceglie” come connettersi al server MariaDB **come root**.
- Se si sta usando nella fase di bootstrap iniziale (init scripts), la connessione avviene via **socket locale** (senza host/porta).
- Se si sta rieseguendo gli script tramite `db-provision`, la connessione avviene via **TCP** verso `mariadb:3306`.

Esempio semplificato (concetto):

```bash
mariadb_root() {
   if [[ -n "${PROVISION_HOST}" ]]; then
      mariadb -h"${PROVISION_HOST}" -P"${PROVISION_PORT}" -uroot -p"${ROOT_PASSWORD}"
   else
      mariadb -uroot -p"${ROOT_PASSWORD}"
   fi
}
```

`cat <<EOSQL | mariadb_root`

- `<<EOSQL` è un *heredoc*: permette di scrivere un blocco di testo multilinea (qui: SQL) direttamente nello script.
- `cat <<EOSQL` stampa quel blocco su `stdout`.
- `| mariadb_root` (pipe) inoltra quello `stdout` come `stdin` al client `mariadb`.

In pratica è equivalente a: “esegui questo SQL come se fosse un file `.sql`”.

`if [[ -n "${PROVISION_HOST}" ]]; then`

- `[[ ... ]]` è il test “moderno” di Bash.
- `-n` significa: la stringa è **non vuota**.
- Qui viene usato per capire se siamo in modalità “provision su DB già avviato” (TCP) oppure in init (socket).

`if [[ -z "${APP_PASSWORD}" ]]; then`

- `-z` significa: la stringa è **vuota**.
- Qui è una guardia di sicurezza: senza `MARIADB_PASSWORD` non avrebbe senso creare/aggiornare l’utente applicativo.

Scenario d’uso: `MARIADB_GRANT_ALL_ON_DB`

- Default `0`: permessi più “contenuti” (CRUD + DDL base) sul solo database applicativo.
- In sviluppo, se si fanno migrazioni EF (`dotnet ef migrations add ...` + `dotnet ef database update`), può servire più libertà.

Esempio (DEV "poter fare tutto"):

1) Mettere in `.env`:

   ```dotenv
   MARIADB_GRANT_ALL_ON_DB=1
   MARIADB_GRANT_SCOPE=server
   MARIADB_GRANT_WITH_GRANT_OPTION=1
   ```

1) Applicare i grants:

   - reset volume (soluzione “pulita”): `docker compose -f .devcontainer/docker-compose.yml down -v`
   - oppure re-provision: `docker compose -f .devcontainer/docker-compose.yml --profile tools run --rm db-provision`

   Nota:

   - se `MARIADB_GRANT_SCOPE=database`, anche con `MARIADB_GRANT_ALL_ON_DB=1` i permessi restano confinati a `<db>.*`
   - se `MARIADB_GRANT_SCOPE=server`, i permessi diventano su `*.*` (molto comodo in sviluppo, ma da evitare in produzione)

Se si vuole rieseguire il provisioning si hanno 2 opzioni:

1) Reset completo (ricrea volume e rilancia automaticamente init scripts)

- `docker compose -f .devcontainer/docker-compose.yml down -v`

Poi riapertura/ricostruzione del devcontainer.

1) Re-provision senza eliminare il volume (utile dopo `DROP DATABASE`)

- (opzionale) elimina il database:
   - `docker compose -f .devcontainer/docker-compose.yml exec mariadb mariadb -uroot -p$MARIADB_ROOT_PASSWORD -e "DROP DATABASE IF EXISTS \`$MARIADB_DATABASE\`;"`
- rieseguire il provisioning (idempotente):
   - `docker compose -f .devcontainer/docker-compose.yml --profile tools run --rm db-provision`

## 🔐 Variabili d’ambiente: `.env` e `.env.example`

### `.env`

File **locale** con variabili d’ambiente (può contenere password), quindi:

- è ignorato da Git (vedi `.gitignore`)
- va gestito per-macchina/per-utente

In questo repo viene caricato dal servizio `app` tramite `env_file` in Docker Compose.

Variabili attese:

- `MARIADB_HOST`
- `MARIADB_PORT`
- `MARIADB_DATABASE`
- `MARIADB_USER`
- `MARIADB_PASSWORD`

Variabili opzionali (permessi):

- `MARIADB_GRANT_ALL_ON_DB`
- `MARIADB_GRANT_SCOPE`
- `MARIADB_GRANT_WITH_GRANT_OPTION`

Variabili opzionali (seed):

- `MARIADB_ENABLE_SEED`

Variabile usata dal container MariaDB per l’inizializzazione:

- `MARIADB_ROOT_PASSWORD`

### `.env.example`

Template committabile con le stesse chiavi ma valori “di esempio”.
Workflow tipico:

1. `cp .env.example .env`
2. aggiorna i valori
3. non committare `.env`

### Nota su Windows (CRLF)

Se `.env` viene salvato con fine riga `CRLF`, alcuni tool/shell possono introdurre un carattere `\r` nei valori.
Buona pratica: salvare `.env` con fine riga `LF`.

## 🧩 Come la Minimal API usa le variabili

In `src/MyApi/Program.cs` la connection string viene costruita a runtime.
Lettura variabili:

- tramite `builder.Configuration["MARIADB_..."]` (che include le variabili d’ambiente)
- con fallback a valori di default se alcune variabili mancano

Esempio logico:

- host/port/db/user/password vengono letti dall’environment
- si compone una stringa tipo:
   - `Server=<host>;Port=<port>;Database=<db>;User Id=<user>;Password=<pwd>;`

Entity Framework Core (provider MySQL/MariaDB) usa poi questa connection string in `UseMySql(...)`.

Nota: in `appsettings.json` trovi un valore placeholder per `ConnectionStrings:Pizzas`. In questo esempio la stringa effettiva viene presa dalle variabili d’ambiente.

## 📚 File di contorno: `.gitignore` e `.dockerignore`

### `.gitignore`

Serve a evitare di committare file locali (build output, file IDE, segreti).
In particolare `.env` è ignorato apposta.

### `.dockerignore`

Serve a non inviare al Docker build context file inutili o sensibili, così:

- build più veloce (meno file trasferiti/coperti)
- cache migliore (meno layer invalidati da file che cambiano spesso)
- minor rischio di includere segreti nell’immagine (es. `.env`)

### `.gitattributes`

Serve a garantire che i file di testo siano salvati con fine riga `LF` (comportamento standard su Linux e consigliato per i progetti multi-OS).

## 🧪 Verifiche rapide

### Verificare che le variabili siano visibili nel container

Nel terminale del devcontainer:

```bash
printenv | grep '^MARIADB_'
```

Se non si vedono le variabili, la causa più comune è che il container era già avviato prima di creare/modificare `.env`.
In tal caso:

- `F1` → “Dev Containers: Restart Container” (spesso basta)
- oppure “Dev Containers: Rebuild Container” (più drastico)

### Verificare che l’app parta

```bash
dotnet run --project src/MyApi/MyApi.csproj
```

Poi aprire Swagger:

- `http://localhost:5000/swagger`

## 🔌 Collegarsi al DB dall'host

Per scopi didattici MariaDB viene pubblicato sull'host (porta `3306` per default), così si possono usare tool esterni (DBeaver / MySQL Workbench).

Se sull'host si hanno già un DB sulla `3306`, cambiare `MARIADB_HOST_PORT` nel proprio `.env` (es. `3307`). Poi ricostruire il devcontainer.

Dal tool sull'host:

- host: `localhost`
- port: `MARIADB_HOST_PORT` (default `3306`)
- database/user/password: quelli del tuo `.env`

Nota: in scenari reali è spesso preferibile NON esporre la porta del DB e far parlare l'app al DB solo sulla rete Docker.

## 🛠️ Troubleshooting

### La rete `my-net` non esiste

Con questa versione dell’esempio la rete viene creata da Compose. Se ci sono errori:

- `F1` → “Dev Containers: Rebuild Container”
- verificare che `.devcontainer/docker-compose.yml` non abbia `external: true`

### Il DB non è raggiungibile dal devcontainer

Sintomi:

- timeout di connessione
- “Name or service not known”

Checklist:

- il servizio `mariadb` è avviato
- `MARIADB_HOST` corrisponde al nome servizio (`mariadb`)
- stai usando la porta corretta (`MARIADB_PORT`, default 3306)

### Credenziali errate / Access denied

Sintomi:

- “Access denied for user ...”

Soluzione:

- verificare che le credenziali `MARIADB_USER` / `MARIADB_PASSWORD` siano corrette
- verificare che l’utente abbia permessi sul database `MARIADB_DATABASE`

Nota: usare `root` è comodo per demo, ma non è una best practice per ambienti reali.

### E' stato modificato `.env` ma l’app non vede i cambiamenti

Perché succede: le variabili di `env_file` vengono lette quando Compose crea/avvia il container.

Soluzione:

- `F1` → “Dev Containers: Restart Container”

Se non basta:

- `F1` → “Dev Containers: Rebuild Container”

## 🧯 Trovare PID e fermare processi (Linux / Dev Container)

Quando si lancia un comando nel terminale (es. `dotnet run`) il processo può restare attivo finché non lo si interrompe.

**Stop “pulito” (foreground):**

- Se il processo sta girando nel terminale in primo piano, premere `Ctrl+C` nel *medesimo* terminale.

**Trovare il PID (Process ID):**

- Lista processi con PID e comando (output più leggibile di `ps aux`):

   ```bash
   ps -eo pid,cmd
   ```

- Filtrare i processi per parola chiave (trucco `[d]otnet` evita di fare il match con la riga del `grep` stesso):

   ```bash
   ps -eo pid,cmd | grep -E '[d]otnet'
   ```

**Caso particolare: `dotnet` (trovare i PID giusti)**

- PID dei processi avviati con `dotnet run`:

   ```bash
   pgrep -f "dotnet run"
   ```

- PID dei processi avviati con `dotnet watch`:

   ```bash
   pgrep -f "dotnet watch"
   ```

**Fermare un processo usando il PID:**

- Tentativo “gentile” (SIGTERM):

   ```bash
   kill <PID>
   ```

- Se non si ferma (forzato, SIGKILL):

   ```bash
   kill -9 <PID>
   ```

**Fermare tutti i `dotnet run` (attenzione: li chiude tutti)**

   ```bash
   pkill -f "dotnet run"
   ```

## 🤖 AI Assistants: Claude Code e OpenCode

### Panoramica

Questo repository include (opzionali) due assistenti AI configurabili in Dev Container:

- **Claude Code** (estensione + feature)
- **OpenCode** (estensione + feature)

L’idea è separare in modo chiaro:

- **segreti/config** (API key, token) → restano in file locali *git-ignored*
- **bootstrap nel container** → copia dei file nella *home* dell’utente container, al momento della creazione

### Mount della configurazione (host → container)

In [.devcontainer/devcontainer.json](.devcontainer/devcontainer.json) sono definiti due `mounts`:

- `.claude-config` → `/mnt/claude-config`
- `.opencode-config` → `/mnt/opencode-config`

Questi mount fanno sì che il container possa leggere i file locali (sull’host) senza che finiscano nell’immagine.

File attesi (locali, sul repo):

- `.claude-config/settings.json` (da creare a partire da `.claude-config/settings.json.example`)
- `.opencode-config/auth.json` (da creare a partire da `.opencode-config/auth.json.example`)

### `postCreateCommand`: restore + init automatico

Sempre in [.devcontainer/devcontainer.json](.devcontainer/devcontainer.json) viene eseguito un `postCreateCommand` che:

1) se trova una `*.sln` nella workspace:
    - esegue `dotnet restore`
    - esegue `dotnet dev-certs https --trust`
2) esegue due script Node.js nella workspace:
    - `.devcontainer/init-claude.cjs`
    - `.devcontainer/init-opencode.cjs`

Nota: questa logica gira **dentro** il Dev Container (utente `vscode`), quindi la “home” tipica è `/home/vscode`.

### Cosa fanno gli script di init

Claude Code — [.devcontainer/init-claude.cjs](.devcontainer/init-claude.cjs):

- imposta/aggiorna `~/.claude.json` aggiungendo `hasCompletedOnboarding: true`
- copia `/mnt/claude-config/settings.json` in `~/.claude/settings.json` (se il file esiste)

OpenCode — [.devcontainer/init-opencode.cjs](.devcontainer/init-opencode.cjs):

- crea `~/.local/share/opencode/` (se manca)
- copia `/mnt/opencode-config/auth.json` in `~/.local/share/opencode/auth.json` (se il file esiste)

Se i file non sono presenti nei mount, gli script non falliscono subito per “file mancante”: emettono un warning. Questo è utile per tenere gli assistenti **facoltativi**.

### Aggiornare la configurazione (senza rebuild completo)

Se si modificano sull’host:

- `.claude-config/settings.json` oppure
- `.opencode-config/auth.json`

il mount rende immediatamente visibile il file aggiornato in `/mnt/...`, ma la copia nella home (`~/.claude/...` e `~/.local/share/opencode/...`) va aggiornata.

Opzione rapida (dentro il devcontainer):

```bash
node .devcontainer/init-claude.cjs
node .devcontainer/init-opencode.cjs
```

Opzione “pulita” (se si vuole rieseguire tutta la pipeline):

- `F1` → “Dev Containers: Rebuild Container”

### Sicurezza (token e API key)

I file con segreti sono volutamente esclusi dal versionamento (tramite `.gitignore`) e non vengono copiati nell’immagine Docker, ma solo montati al momento dell’esecuzione del container:

- `.claude-config/settings.json`
- `.opencode-config/auth.json`

Per i dettagli su come prepararli in modo sicuro, vedere:

- [.claude-config/README.md](.claude-config/README.md)
- [.opencode-config/README.md](.opencode-config/README.md)

## 🌐 Dev Containers a scuola (proxy)

### Scenario

In alcune reti scolastiche l’accesso a Internet è mediato da un proxy (es. `http://proxy:3128`).

In questo setup:

- **Docker** spesso gestisce il proxy in modo “trasparente” per i container: dal container si riesce a navigare/scaricare senza dover configurare proxy applicativi.
- Il problema più comune riguarda **VS Code e le sue estensioni**, che possono fare richieste di rete e (a seconda di dove girano) non “vedere” automaticamente la stessa configurazione.

Obiettivo: far funzionare insieme

- app e tool nel container
- estensioni/strumenti legati a VS Code (specialmente quelle basate su Node)

### Avvio corretto di VS Code su Windows (PowerShell)

1) Aprire **Windows PowerShell**.

2) Impostare le variabili d’ambiente nella *stessa* sessione:

   ```powershell
   $env:HTTP_PROXY="http://proxy:3128"
   $env:HTTPS_PROXY="http://proxy:3128"
   $env:NO_PROXY="localhost,127.0.0.1,host.docker.internal"
   ```

3) Dalla stessa shell, aprire VS Code sulla cartella del progetto:

   ```powershell
   code .
   ```

4) In VS Code: `F1` → “Dev Containers: Reopen in Container”.

Così VS Code (lato host) eredita le variabili e, quando serve, anche i processi collegati possono usarle.

### Perché `http.proxySupport` è impostato su `off`

In [.devcontainer/devcontainer.json](.devcontainer/devcontainer.json) è presente:

- `"http.proxySupport": "off"`

Questo forza VS Code (lato Dev Container / VS Code Server) a **non** gestire un proxy applicativo “proprio”, e a comportarsi come se l’accesso fosse diretto.

Nel contesto “proxy trasparente” di Docker, questo evita la situazione in cui:

- alcune estensioni tentano di usare un proxy configurato in VS Code (o auto-detect)
- mentre la rete del container è già instradata correttamente

Risultato pratico: riduce i casi di estensioni che non riescono a scaricare risorse o che rimangono in timeout per una configurazione proxy incoerente.

### Problemi tipici e fix rapidi

- **Le estensioni non installano / non aggiornano**
   - assicurati di lanciare VS Code da PowerShell con `HTTP_PROXY/HTTPS_PROXY/NO_PROXY` già impostate
   - prova `F1` → “Developer: Reload Window”, poi “Dev Containers: Rebuild Container”

- **Autenticazioni via proxy**
   - se il proxy richiede credenziali, la stringa proxy potrebbe dover includere user/password (dipende dalle policy della scuola)

- **Servizi locali non raggiungibili**
   - verifica che `NO_PROXY` includa `localhost,127.0.0.1,host.docker.internal`

## 🐛 Debug Node: perché `debug.javascript.autoAttachFilter` è `disabled`

In [.devcontainer/devcontainer.json](.devcontainer/devcontainer.json) è presente:

- `"debug.javascript.autoAttachFilter": "disabled"`

Motivo: alcune estensioni (es. assistenti AI e tool che usano Node) avviano processi Node in background. Se l’auto-attach del debugger JS è attivo, VS Code può tentare di “agganciarsi” a quei processi e causare:

- rallentamenti
- comportamenti strani
- errori intermittenti

Scelta consigliata:

- lasciare `disabled` come default nel Dev Container
- abilitarlo solo quando serve davvero fare debug di un’app Node/JS (e poi rimetterlo `disabled`)