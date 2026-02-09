# Configurazione Claude Code

Questo file `settings.json` verrà copiato in `~/.claude/settings.json` nel container durante la creazione.

## Setup iniziale

1. **Copia il template**:
   ```bash
   cp settings.json.example settings.json
   ```

2. **Modifica `settings.json`**:
   - Inserisci la tua API key in `ANTHROPIC_API_KEY`
   - Se necessario, modifica anche `ANTHROPIC_BASE_URL` per il tuo provider LLM

3. **Ricostruisci il dev container**:
   - Riapri la cartella nel container
   - Il `postCreateCommand` configurerà automaticamente Claude Code

## Come funziona

Il devcontainer:
1. Monta `.claude-config` in `/mnt/claude-config` nel container
2. Durante il `postCreateCommand`:
   - Esegue il codice Node.js per impostare `hasCompletedOnboarding: true` in `~/.claude.json`
   - Crea la directory `~/.claude`
   - Copia `settings.json` in `~/.claude/settings.json`

## Sicurezza

⚠️ **IMPORTANTE**: Il file `settings.json` è ignorato da git per proteggere le tue API key.

**NON** rimuovere questa riga dal `.gitignore`:
```
.claude-config/settings.json
```

Il file `settings.json.example` può essere committato come template.
