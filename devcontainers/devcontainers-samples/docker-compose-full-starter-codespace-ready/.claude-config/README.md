# Configurazione Claude Code (Codespaces-ready)

Questo esempio NON usa bind-mount da `${localWorkspaceFolder}`.
Lo script di init fa solo l’onboarding, mentre la configurazione viene gestita manualmente dentro al container.

## Setup iniziale

### 1) Onboarding automatico

All’avvio/creazione del devcontainer, `.devcontainer/init-claude.cjs` imposta `hasCompletedOnboarding: true` in `~/.claude.json`.

### 2) Configurazione manuale (consigliata)

Creare e modificare `~/.claude/settings.json` direttamente nel terminale del devcontainer.

Esempio “copy & paste” (template minimale):

```bash
mkdir -p ~/.claude
cat > ~/.claude/settings.json <<'JSON'
{
   "env": {
      "ANTHROPIC_BASE_URL": "url-del-proprio-provider",
      "ANTHROPIC_API_KEY": "INSERISCI_LA_PROPRIA_CHIAVE"
   },
   "enabledPlugins": {},
   "autoUpdatesChannel": "latest"
}
JSON
```

Aprire il file e sostituire la chiave:

```bash
code ~/.claude/settings.json
```

## Codespaces Secrets (alternativa ai file locali)

Si può salvare la propria API key in Codespaces Secrets così non la si scrive “a mano” ogni volta:

- `ANTHROPIC_API_KEY` (obbligatorio)
- `ANTHROPIC_BASE_URL` (opzionale)

In questa variante però **non** c’è automazione che trasformi i Secrets in `~/.claude/settings.json`.
Se si usano i Secrets, bisogna comunque incollarli/usarli manualmente quando si crea `settings.json`.

## Sicurezza

⚠️ **IMPORTANTE**: `.claude-config/settings.json` deve rimanere fuori da Git.
Il template `settings.json.example` può essere committato.
