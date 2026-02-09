# Configurazione OpenCode

Il file `auth.json` verrà copiato in `~/.local/share/opencode/auth.json` nel container durante la creazione.

## Setup iniziale

1. **Copia il template**:
   ```bash
   cp auth.json.example auth.json
   ```

2. **Modifica `auth.json`**:
   - Inserisci il tuo token OpenCode in `token`

3. **Ricostruisci il dev container**:
   - Riapri la cartella nel container
   - Il file verrà copiato automaticamente durante la creazione

## Sicurezza

⚠️ **IMPORTANTE**: Il file `auth.json` è ignorato da git per proteggere i tuoi token.

Il file `auth.json.example` può essere committato come template.
