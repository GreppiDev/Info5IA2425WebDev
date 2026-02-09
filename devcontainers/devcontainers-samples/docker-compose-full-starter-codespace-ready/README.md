# Starter Project (Codespaces-ready) - Dev Container + MariaDB (Docker Compose)

Questa cartella è una variante di **docker-compose-full-starter** pensata per funzionare bene anche in **GitHub Codespaces** e con **prebuilds**, senza cambiare la versione locale già testata.

## Guida operativa (step-by-step)

Questa guida spiega come:

1. creare un **repository dedicato** solo per questa versione Codespaces-ready
2. abilitare **Codespaces** e (opzionale) **Prebuilds**
3. configurare i **Secrets** (DB)
4. creare un Codespace e verificare che sia tutto ok

> Nota: Codespaces usa la configurazione devcontainer presente nel repository (tipicamente `.devcontainer/` in root). Per questo motivo la strada più semplice è avere un repo dedicato dove questa cartella diventa la root del repo.

### 1) Creare un repo dedicato (GitHub)

1. Andare su GitHub → `New repository`
2. Nome consigliato: `docker-compose-full-starter-codespace-ready` (o simile)
3. Visibilità: `Private` se dentro ci saranno studenti/chiavi, altrimenti `Public`
4. Inizializzazione:

  - si può **non** creare README (si può aggiungere con push), oppure crearlo e poi sovrascriverlo
  - non serve aggiungere `.gitignore` da template (ce l’hai già)

### 2) Popolare il repo con i file della variante

Obiettivo: nel nuovo repo, questi elementi devono essere in root:

- `.devcontainer/`
- `README.md`
- `.env.example`
- eventuali cartelle `src/`, ecc.

Workflow consigliato (locale):

1. Creare una cartella vuota sul PC, es. `codespace-starter-repo/`
2. Copiare dentro TUTTO il contenuto della cartella di questo esempio
3. In quella cartella:

  - `git init`
  - `git add .`
  - `git commit -m "Initial commit (codespaces-ready)"`
  - `git branch -M main`
  - `git remote add origin <URL-del-repo>`
  - `git push -u origin main`

> Importante: NON committare mai `.env` o file con token. Usare solo `.env.example` e Secrets.

### 3) Abilitare Codespaces sul repository

Nel repo su GitHub:

1. Andare su `Settings`
2. Cercare `Codespaces`
3. Assicurarsi che Codespaces sia consentito per quel repository (dipende anche dalle policy dell’organizzazione/account)

Se non vedi la sezione Codespaces:

- verificare che l’account/organizzazione abbia Codespaces abilitato
- verificare di avere permessi di admin sul repo

### 4) (Opzionale) Abilitare Prebuilds

I prebuilds servono a far trovare il Codespace “già pronto” (immagine buildata, estensioni, restore, ecc.).

Nel repo su GitHub:

1. Andare su `Settings` → `Codespaces`
2. Sezione `Prebuilds`
3. Creare una configurazione di prebuild per:

  - branch: `main`
  - region: quella più vicina (EU/US)
  - trigger: tipicamente su `push`/schedule (in base alle opzioni che GitHub mostra)

Consiglio pratico: abilitare prebuild su `main` e rigenerare quando si aggiorna `.devcontainer/`.

### 5) Configurare i Secrets per il DB (consigliato)

Nel repo su GitHub:

1. Andare su `Settings` → `Secrets and variables` → `Codespaces`
2. Aggiungere i secret (nomi uguali alle variabili):

  - `MARIADB_DATABASE`
  - `MARIADB_USER`
  - `MARIADB_PASSWORD`
  - `MARIADB_ROOT_PASSWORD`
  - (opzionale) `MARIADB_HOST_PORT`

Perché così: la compose usa default `${VAR:-...}` ma se il Secret esiste lo userà automaticamente.

### 6) Creare il Codespace

1. Andare sul repo su GitHub
2. Cliccare `Code` → tab `Codespaces`
3. `Create codespace on main`

La prima creazione può richiedere tempo (build immagine + download). Con prebuild attivo, le successive saranno molto più veloci.

### 7) Verifiche rapide (dentro al Codespace)

Nel terminale del devcontainer:

Verificare che le variabili DB siano presenti:

```bash
printenv | egrep '^(MARIADB_DATABASE|MARIADB_USER|MARIADB_PASSWORD|MARIADB_ROOT_PASSWORD|MARIADB_HOST|MARIADB_PORT)='
```

Verificare che MariaDB sia su e risponda:

```bash
mariadb -h"${MARIADB_HOST:-mariadb}" -P"${MARIADB_PORT:-3306}" -u"${MARIADB_USER:-pizza_user}" -p"${MARIADB_PASSWORD:-pizza_password}" -e "SELECT 1;"
```

### 8) Avviare l’app (.NET)

Quando si ha un progetto in `src/`:

```bash
dotnet run --project src/MyApi/MyApi.csproj
```

Poi aprire:

- `http://localhost:5000/swagger`
- `https://localhost:5001/swagger`

## Cosa cambia rispetto allo starter “locale”

- Niente `mounts` basati su `${localWorkspaceFolder}`: in Codespaces non esiste un path locale Windows/Mac da bind-montare.
- Nessuna dipendenza da un file `.env` obbligatorio all’avvio: in Compose qui usiamo variabili con default `${VAR:-default}`.
  - Si può comunque usare un `.env` locale (non committato) oppure **Codespaces Secrets** per sovrascrivere i default.

## AI assistants (Claude / OpenCode)

Questa variante mantiene le cose semplici:

- **Claude Code**: lo script `.devcontainer/init-claude.cjs` fa solo l’onboarding (set `hasCompletedOnboarding: true`).
  - La configurazione (API key, provider, ecc.) viene inserita manualmente dentro al container creando/modificando `~/.claude/settings.json`.

Esempio “copy & paste” (dentro al terminale del devcontainer) per creare un template minimale:

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

Poi aprire e modificare il file direttamente nel Codespace:

```bash
code ~/.claude/settings.json
```

- **OpenCode**: nessuno script di init.
  - Fare login direttamente da OpenCode dentro al Codespace; il tool scrive i token nel proprio `$HOME` (tipicamente in `~/.local/share/opencode/`).

### Codespaces Secrets (AI)

Se si vuole comunque usare Secrets per le AI, si può farlo: i Secrets diventano variabili d’ambiente nel Codespace.
In questo esempio però non c’è automazione che le trasformi in file di configurazione.

⚠️ Non committare mai token/API key.
