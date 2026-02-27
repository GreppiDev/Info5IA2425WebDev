#!/bin/bash
# Script per creare il .env in .devcontainer/ (Docker Compose lo legge qui)
# Crea un symlink se possibile, altrimenti copia (per Windows senza permessi symlink)

cd "$(dirname "$0")" || exit 1

# Se il .env non esiste nella root, lo crea da .env.example
if [ ! -f ../.env ]; then
    if [ -f ../.env.example ]; then
        echo "File .env non trovato nella root. Creazione da .env.example..."
        cp ../.env.example ../.env
        echo "File .env creato con successo nella root."
    else
        echo "Errore: né .env né .env.example trovati nella root del progetto"
        exit 1
    fi
fi

# Rimuovi eventuale file/symlink precedente
rm -f .env

# Prova a creare un symlink (preferito per sincronizzazione live)
if ln -sf ../.env .env 2>/dev/null; then
    # Verifica che il symlink sia effettivamente leggibile
    # Su Windows il symlink può essere creato ma non accessibile nel container
    if test -r .env && test -s .env; then
        echo "Symlink creato: .devcontainer/.env -> ../.env"
        echo "Le modifiche al .env sono sincronizzate automaticamente."
    else
        # Il symlink esiste ma non è leggibile - rimuovilo e usa copia
        echo "INFO: Symlink creato ma non leggibile (problema permessi Windows)."
        rm -f .env
        cp ../.env .env
        echo "File .env copiato in .devcontainer/.env"
        echo "AVVISO: Se modifichi .env nella root, ricostruisci il container per applicare i cambiamenti."
        echo "Per abilitare i symlink su Windows: https://github.com/git-for-windows/git/wiki/Symbolic-Links"
    fi
else
    # Fallback: copia il file (Windows senza permessi symlink)
    echo "INFO: Impossibile creare symlink (permessi Windows?). Uso copia..."
    cp ../.env .env
    echo "File .env copiato in .devcontainer/.env"
    echo "AVVISO: Se modifichi .env nella root, ricostruisci il container per applicare i cambiamenti."
    echo "Per abilitare i symlink su Windows: https://github.com/git-for-windows/git/wiki/Symbolic-Links"
fi
