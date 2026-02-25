#!/bin/bash
# Script per creare symlink del .env dalla root a .devcontainer/
# Questo permette a Docker Compose di trovare il file .env nella stessa directory

cd "$(dirname "$0")" || exit 1

# Se il .env non esiste nella root, lo crea da .env.example
if [ ! -f ../.env ]; then
    if [ -f ../.env.example ]; then
        echo "File .env non trovato. Creazione da .env.example..."
        cp ../.env.example ../.env
        echo "File .env creato con successo. Puoi modificarlo se necessario."
    else
        echo "Errore: né .env né .env.example trovati nella root del progetto"
        exit 1
    fi
fi

# Crea o aggiorna il symlink in .devcontainer/
rm -f .env
ln -sf ../.env .env
echo "Symlink creato: .devcontainer/.env -> ../.env"
