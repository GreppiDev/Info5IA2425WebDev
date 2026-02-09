#!/usr/bin/env bash
set -euo pipefail

# 01-schema.sh
# Idempotente: crea DB e tabella se non esistono.
#
# Esecuzione:
# - automatica al primo bootstrap (via /docker-entrypoint-initdb.d)
# - manuale in qualsiasi momento (via servizio compose db-provision)

DB_NAME="${MARIADB_DATABASE:-pizza_store}"
ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:-}"
PROVISION_HOST="${MARIADB_PROVISION_HOST:-}"
PROVISION_PORT="${MARIADB_PROVISION_PORT:-3306}"

mariadb_root() {
  if [[ -n "${PROVISION_HOST}" ]]; then
    mariadb -h"${PROVISION_HOST}" -P"${PROVISION_PORT}" -uroot -p"${ROOT_PASSWORD}"
  else
    # In initdb phase, il client usa il socket locale.
    mariadb -uroot -p"${ROOT_PASSWORD}"
  fi
}

cat <<EOSQL | mariadb_root
CREATE DATABASE IF NOT EXISTS \`${DB_NAME}\`;

CREATE TABLE IF NOT EXISTS \`${DB_NAME}\`.\`Pizzas\` (
  \`Id\` INT NOT NULL AUTO_INCREMENT,
  \`Name\` VARCHAR(255) NULL,
  \`Description\` TEXT NULL,
  PRIMARY KEY (\`Id\`)
) ENGINE=InnoDB;
EOSQL

echo "[db-init] Schema pronto su DB '${DB_NAME}'."
