#!/usr/bin/env bash
set -euo pipefail

# 02-seed.sh
# Seed idempotente: se le righe esistono gia' (stesso Id) viene aggiornato il contenuto.

ENABLE_SEED="${MARIADB_ENABLE_SEED:-1}"

DB_NAME="${MARIADB_DATABASE:-pizza_store}"
ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:-}"
PROVISION_HOST="${MARIADB_PROVISION_HOST:-}"
PROVISION_PORT="${MARIADB_PROVISION_PORT:-3306}"

mariadb_root() {
  if [[ -n "${PROVISION_HOST}" ]]; then
    mariadb -h"${PROVISION_HOST}" -P"${PROVISION_PORT}" -uroot -p"${ROOT_PASSWORD}"
  else
    mariadb -uroot -p"${ROOT_PASSWORD}"
  fi
}

if [[ "${ENABLE_SEED}" == "0" ]]; then
  echo "[db-init] Seed disabilitato (MARIADB_ENABLE_SEED=0)."
  exit 0
fi

# Nota: quando lo script viene eseguito via provisioner (TCP) non c'e' un database selezionato di default.
# Per funzionare in entrambi i contesti (initdb e db-provision), selezioniamo esplicitamente il DB via USE.
cat <<EOSQL | mariadb_root
USE \`${DB_NAME}\`;

INSERT INTO \`Pizzas\` (\`Id\`, \`Name\`, \`Description\`) VALUES
  (1, 'Montemagno', 'Pizza shaped like a great mountain'),
  (2, 'The Galloway', 'Pizza shaped like a submarine, silent but deadly'),
  (3, 'The Noring', 'Pizza shaped like a Viking helmet, where''s the mead')
ON DUPLICATE KEY UPDATE
  \`Name\` = VALUES(\`Name\`),
  \`Description\` = VALUES(\`Description\`);
EOSQL

echo "[db-init] Seed applicato su DB '${DB_NAME}'."
