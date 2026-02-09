#!/usr/bin/env bash
set -euo pipefail

# 03-grants.sh
# Provisioning "production-like": niente valori hardcoded nel repo.
# Usa le variabili d'ambiente che gia' alimentano il bootstrap di MariaDB:
# - MARIADB_DATABASE
# - MARIADB_USER
# - MARIADB_PASSWORD
# - MARIADB_ROOT_PASSWORD
#
# Nota: gli script in /docker-entrypoint-initdb.d vengono eseguiti SOLO al primo bootstrap
# (volume /var/lib/mysql vuoto).

DB_NAME="${MARIADB_DATABASE:-pizza_store}"
APP_USER="${MARIADB_USER:-pizza_user}"
APP_PASSWORD="${MARIADB_PASSWORD:-}"
ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:-}"
GRANT_ALL_ON_DB="${MARIADB_GRANT_ALL_ON_DB:-0}"
GRANT_SCOPE="${MARIADB_GRANT_SCOPE:-database}"
WITH_GRANT_OPTION="${MARIADB_GRANT_WITH_GRANT_OPTION:-0}"
PROVISION_HOST="${MARIADB_PROVISION_HOST:-}"
PROVISION_PORT="${MARIADB_PROVISION_PORT:-3306}"

mariadb_root() {
  if [[ -n "${PROVISION_HOST}" ]]; then
    mariadb -h"${PROVISION_HOST}" -P"${PROVISION_PORT}" -uroot -p"${ROOT_PASSWORD}"
  else
    mariadb -uroot -p"${ROOT_PASSWORD}"
  fi
}

if [[ -z "${APP_PASSWORD}" ]]; then
  echo "[db-init] MARIADB_PASSWORD non impostata: impossibile creare/gestire l'utente applicativo." >&2
  exit 1
fi

with_grant_sql=""
if [[ "${WITH_GRANT_OPTION}" == "1" ]]; then
  with_grant_sql=" WITH GRANT OPTION"
fi

target_sql="\\\`${DB_NAME}\\\`.*"
if [[ "${GRANT_SCOPE}" == "server" ]]; then
  target_sql="*.*"
fi

# Modalita' permessi:
# - scope=server  : "dev superuser" (ALL PRIVILEGES su *.*), opzionalmente WITH GRANT OPTION
# - scope=database:
#     - GRANT_ALL_ON_DB=1 => ALL PRIVILEGES su <db>.*
#     - GRANT_ALL_ON_DB=0 => least-privilege su <db>.*
if [[ "${GRANT_SCOPE}" == "server" ]]; then
  PRIVS_SQL="GRANT ALL PRIVILEGES ON ${target_sql} TO '${APP_USER}'@'%'${with_grant_sql};"
elif [[ "${GRANT_ALL_ON_DB}" == "1" ]]; then
  PRIVS_SQL="GRANT ALL PRIVILEGES ON ${target_sql} TO '${APP_USER}'@'%';"
else
  PRIVS_SQL="GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES, CREATE VIEW, SHOW VIEW, CREATE TEMPORARY TABLES, LOCK TABLES ON ${target_sql} TO '${APP_USER}'@'%';"
fi

cat <<EOSQL | mariadb_root
-- Se i grants sono DB-scoped, assicuriamoci che il DB esista (idempotente)
CREATE DATABASE IF NOT EXISTS \`${DB_NAME}\`;

-- Crea l'utente applicativo se non esiste (idempotente)
CREATE USER IF NOT EXISTS '${APP_USER}'@'%' IDENTIFIED BY '${APP_PASSWORD}';

-- Permessi sul DB applicativo (confinati al singolo database).
${PRIVS_SQL}

FLUSH PRIVILEGES;
EOSQL

if [[ "${GRANT_SCOPE}" == "server" ]]; then
  echo "[db-init] Grants DEV superuser (ALL PRIVILEGES su *.*) applicati per '${APP_USER}'."
elif [[ "${GRANT_ALL_ON_DB}" == "1" ]]; then
  echo "[db-init] Grants elevati (ALL PRIVILEGES) applicati per '${APP_USER}' su '${DB_NAME}'."
else
  echo "[db-init] Grants least-privilege applicati per '${APP_USER}' su '${DB_NAME}'."
fi
