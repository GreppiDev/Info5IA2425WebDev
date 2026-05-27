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
- non serve aggiungere `.gitignore` da template (è già presente)

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

### Nota: certificato HTTPS di sviluppo su Linux (`dotnet dev-certs`)

Il `postCreateCommand` esegue `dotnet dev-certs https` per generare il certificato self-signed usato da Kestrel sulla porta HTTPS (5001). Su Linux, aggiungere o meno il flag `--trust` ha effetti limitati:

| Flag                             | Cosa fa su Linux                                                                                                                                                                            |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `dotnet dev-certs https`         | Genera il certificato (necessario per Kestrel)                                                                                                                                              |
| `dotnet dev-certs https --trust` | Tenta di aggiungere il certificato al trust store via `libnss3-tools` e `update-ca-certificates`, ma spesso fallisce o funziona solo per alcuni client. Produce warning nel log del rebuild |

Su **Windows** e **macOS** `--trust` è gestito a livello OS ed è pienamente efficace. Su Linux l'effetto è parziale e dipende da cosa è installato nel container.

In Codespaces questo è **irrilevante per l'accesso esterno**: l'URL pubblico `*.app.github.dev` usa TLS gestito dal proxy di Codespaces, che è un certificato valido. Solo `https://localhost:5001` dal terminale interno avrebbe il solito warning di certificato non trusted.

## Port Forwarding in GitHub Codespaces

### Come funziona il port forwarding

Quando un'app ascolta su una porta all'interno del Codespace, GitHub crea automaticamente un **tunnel HTTPS** con un URL pubblico del tipo:

```
https://<codespace-name>-<port>.app.github.dev
```

Questo tunnel passa attraverso il **proxy di Codespaces**, che può operare in due modalità:

| Visibilità            | Comportamento                                              |
| --------------------- | ---------------------------------------------------------- |
| **Private** (default) | Il proxy richiede autenticazione GitHub per ogni richiesta |
| **Public**            | Il proxy lascia passare le richieste senza autenticazione  |

### Il problema: porta privata vs porta pubblica

Con visibilità **Private** (impostazione di default):

- `curl http://localhost:5000/weatherforecast` → **funziona** (richiesta interna al container, non passa per il proxy)
- `curl https://<codespace-name>-5000.app.github.dev/weatherforecast` → **non funziona** (il proxy blocca la richiesta perché manca autenticazione)

Con visibilità **Public**:

- Entrambe le forme funzionano senza autenticazione.

### Come accedere a una porta privata con curl

Per autenticarsi si usa `gh`, la **GitHub CLI** (preinstallata nei Codespaces), che espone il comando `gh auth token` per ottenere il token di sessione GitHub corrente:

```bash
curl https://<codespace-name>-5000.app.github.dev/weatherforecast \
  -H "Authorization: Bearer $(gh auth token)" \
  -H "X-Github-Token: $(gh auth token)"
```

> `$(gh auth token)` viene sostituito dalla shell con il token GitHub attivo. Se non sei autenticato, esegui prima `gh auth login`.

### Come impostare la visibilità della porta manualmente

Nel pannello **Ports** di VS Code (in basso) o nella tab **Ports** dell'interfaccia web di Codespaces:

1. Click destro sulla porta
2. **Port Visibility** → **Public**

⚠️ Questa impostazione è legata alla **sessione** del Codespace: viene persa ogni volta che il Codespace viene ricreato da zero.

### Come rendere la configurazione persistente nel devcontainer.json

Per evitare di dover impostare manualmente la visibilità ad ogni nuova creazione del Codespace, si può configurare `portsAttributes` nel file `.devcontainer/devcontainer.json`:

```json
{
  "portsAttributes": {
    "5000": {
      "protocol": "http",
      "label": "API HTTP",
      "onAutoForward": "openPreview",
      "visibility": "public"
    },
    "5001": {
      "protocol": "https",
      "label": "API HTTPS",
      "onAutoForward": "openPreview",
      "visibility": "public"
    }
  }
}
```

### Problema: porte duplicate nel pannello Ports

Ogni volta che l'applicazione ASP.NET Core viene avviata, Codespaces rileva che Kestrel inizia ad ascoltare sulle porte e le aggiunge di nuovo come "auto-forwarded", anche se sono già presenti in `forwardPorts`. Il risultato è che il pannello **Ports** si riempie di voci duplicate ad ogni avvio.

La soluzione è aggiungere `"onAutoForward": "notify"` per tutte le porte già dichiarate in `forwardPorts`. Così Codespaces le forwarderà comunque (grazie a `forwardPorts`), ma quando l'app le attiva non creerà nuove voci:

```json
"forwardPorts": [5000, 5001],
"portsAttributes": {
  "5000": {
    "protocol": "http",
    "label": "API HTTP",
    "onAutoForward": "notify"
  },
  "5001": {
    "protocol": "https",
    "label": "API HTTPS",
    "onAutoForward": "notify"
  }
}
```

### Opzioni disponibili per `onAutoForward`

| Valore        | Comportamento                                              |
| ------------- | ---------------------------------------------------------- |
| `openBrowser` | Apre il browser automaticamente quando la porta è rilevata |
| `openPreview` | Apre il Simple Browser integrato in VS Code                |
| `notify`      | Mostra una notifica con il link                            |
| `silent`      | Fa il forward senza notifiche                              |
| `ignore`      | Non fa il forward automatico                               |

Opzioni per `visibility`:

| Valore    | Comportamento                              |
| --------- | ------------------------------------------ |
| `public`  | Accessibile senza autenticazione           |
| `private` | Accessibile solo con autenticazione GitHub |

> Nota: impostare una porta come `public` in un repo pubblico significa che **chiunque** può accedere all'URL del Codespace mentre è in esecuzione. Per API di sviluppo con dati sensibili, considerare di mantenere la visibilità `private` e usare l'autenticazione via token.

## Cosa cambia rispetto allo starter "locale"

- Niente `mounts` basati su `${localWorkspaceFolder}`: in Codespaces non esiste un path locale Windows/Mac da bind-montare.
- **Inizializzazione automatica del file `.env`**: lo script `init-env.sh` (eseguito da `initializeCommand`) crea automaticamente il file `.env` dalla root copiandolo da `.env.example` se non è presente, e crea un symlink in `.devcontainer/.env` per permettere a Docker Compose di caricare le variabili.
- **Variabili sensibili senza default**: le variabili del database (`MARIADB_DATABASE`, `MARIADB_USER`, `MARIADB_PASSWORD`) non hanno valori di default nel `docker-compose.yml` e devono essere configurate. Puoi farlo in tre modi:
  1. **Codespaces Secrets** (consigliato per produzione): imposta i secret nel repository GitHub
  2. **Modificare `.env`**: dopo che lo script lo ha creato automaticamente, modifica il file `.env` con i tuoi valori
  3. **`.env.example` personalizzato**: modifica `.env.example` prima del primo avvio del Codespace

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
