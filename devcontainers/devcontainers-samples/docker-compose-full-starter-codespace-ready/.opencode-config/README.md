# Configurazione OpenCode (Codespaces-ready)

In questa variante non usiamo script di inizializzazione per OpenCode.
L’idea è: **fai login direttamente dentro il Codespace** usando OpenCode, e lui scrive i token nel tuo `$HOME`.

## Setup

1. Avvia OpenCode nel devcontainer (dalla UI/estensione o dal terminale, in base a come lo usi di solito)
2. Esegui il login seguendo la procedura interattiva

I file di autenticazione vengono salvati nella home del container (tipicamente sotto `~/.local/share/opencode/`).

## Codespaces Secrets

Puoi usare Codespaces Secrets per salvare API key/token come variabili d’ambiente.
In questo esempio però **non** c’è automazione che trasformi i Secrets in un `auth.json`.

## Sicurezza

⚠️ Non committare mai token/API key.
