# Basic Dev Container for ASP.NET Core Development

- [Basic Dev Container for ASP.NET Core Development](#basic-dev-container-for-aspnet-core-development)
  - [🚀 Quick Start](#-quick-start)
    - [Prerequisiti](#prerequisiti)
    - [Avvio](#avvio)
  - [Tutorial completo su come funziona la configurazione di MariaDB e delle variabili d’ambiente nel Dev Container: Database (MariaDB) e Usare le variabili di `.env` nel Dev Container (runArgs)](#tutorial-completo-su-come-funziona-la-configurazione-di-mariadb-e-delle-variabili-dambiente-nel-dev-container-database-mariadb-e-usare-le-variabili-di-env-nel-dev-container-runargs)
    - [🗄️ Database (MariaDB)](#️-database-mariadb)
      - [Come è reso disponibile nel Dev Container](#come-è-reso-disponibile-nel-dev-container)
      - [Perché MariaDB viene avviato “senza systemd”](#perché-mariadb-viene-avviato-senza-systemd)
      - [Nota su `root` e password](#nota-su-root-e-password)
      - [Volumi: persistenza dei dati (opzionale)](#volumi-persistenza-dei-dati-opzionale)
      - [Troubleshooting (MariaDB)](#troubleshooting-mariadb)
    - [🔐 Root del container (Sistema Operativo)](#-root-del-container-sistema-operativo)
      - [Qual è la password di `root` (Linux)?](#qual-è-la-password-di-root-linux)
      - [Nota: perché nel `Dockerfile` usiamo `DEBIAN_FRONTEND=noninteractive` ed eventualmente `--no-install-recommends`](#nota-perché-nel-dockerfile-usiamo-debian_frontendnoninteractive-ed-eventualmente---no-install-recommends)
      - [Nota: perché nel `Dockerfile` configuriamo Git (`init.defaultBranch` e `safe.directory`)](#nota-perché-nel-dockerfile-configuriamo-git-initdefaultbranch-e-safedirectory)
    - [📚 Mini-tutorial: file “di contorno” del repo](#-mini-tutorial-file-di-contorno-del-repo)
  - [📚 File di contorno: `.gitignore`, `.dockerignore` e `.gitattributes`](#-file-di-contorno-gitignore-dockerignore-e-gitattributes)
    - [`.gitignore`](#gitignore)
    - [`.dockerignore`](#dockerignore)
    - [`.gitattributes`](#gitattributes)
    - [`.env`](#env)
    - [`.env.example`](#envexample)
    - [🧯 Trovare PID e fermare processi (Linux / Dev Container)](#-trovare-pid-e-fermare-processi-linux--dev-container)
    - [🧩 Usare le variabili di `.env` nel Dev Container (runArgs)](#-usare-le-variabili-di-env-nel-dev-container-runargs)
    - [🧪 Tutorial completo: rendere `.env` disponibile nella shell del container](#-tutorial-completo-rendere-env-disponibile-nella-shell-del-container)
      - [`runArgs` + `--env-file` (variabili globali nel container)](#runargs----env-file-variabili-globali-nel-container)
      - [Verifica rapida](#verifica-rapida)
    - [Nome immagine e nome container (perché a volte Docker “sceglie a caso”)](#nome-immagine-e-nome-container-perché-a-volte-docker-sceglie-a-caso)

Esempio di Dev Container per sviluppo ASP.NET Core senza database esterno: **MariaDB Server** è installato e avviato direttamente nel container di sviluppo per semplicità.

## 🚀 Quick Start

### Prerequisiti

- [Docker Desktop](https://www.docker.com/products/docker-desktop)

- [VS Code](https://code.visualstudio.com/) con estensione [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Avvio

1. **Clonare il repository**

   ```bash
   git clone <url-repo>
   cd basic-container-demo
   ```

   In alternativa, se non si usa Git, è possibile scaricare un `.zip`, estrarlo e aprire la cartella estratta.

2. **Aprire la cartella in VS Code**

   - File → Open Folder… → selezionare la cartella del progetto

3. **Avviare il Dev Container**

   - Se VS Code mostra il popup “Reopen in Container”, cliccarlo.
   - Oppure: `F1` → “Dev Containers: Reopen in Container”.

   La prima volta VS Code:

   - Effettuare la build dell’immagine Docker (può richiedere qualche minuto)
   - creare il container
   - eseguire le configurazioni (`postCreateCommand`, ecc.)

4. **Configurare le variabili locali (`.env`)**

   Questo repo usa `runArgs --env-file` (metodo semplice) e quindi serve un file `.env` nella root del repository.
   Per evitare errori e semplificare l'uso in contesti didattici, il devcontainer crea automaticamente `.env` copiando `.env.example` (se `.env` non esiste) tramite `initializeCommand`.
   Nota: `.env` è ignorato da Git e non fa parte del repository.

   **Come funziona `initializeCommand` (creazione automatica di `.env`)**

   Lo script [.devcontainer/init-dotenv.cjs](.devcontainer/init-dotenv.cjs) viene eseguito da VS Code tramite `initializeCommand`.
   Fa una cosa semplice: se `.env` non esiste ancora, copia `.env.example` in `.env`.

   Nota importante: `initializeCommand` gira sul computer **host** (prima che il container parta). Serve solo a garantire che `.env` esista, perché Docker fallisce se `--env-file .env` punta a un file mancante.

   Modifica `.env` e imposta `MARIADB_DATABASE`, `MARIADB_USER`, `MARIADB_PASSWORD`.
   In questo progetto didattico `MARIADB_USER` è pensato come **utente applicativo** (es. `devuser`) usato da Minimal API e da `dotnet ef`.
   Evita di impostare `MARIADB_USER=root`: `root` è pensato solo per operazioni amministrative (vedi nota sotto).
   Se si vuole impostare anche una password per l’utente `root` di MariaDB, valorizzare `MARIADB_ROOT_PASSWORD`.

   Nota: dopo aver modificato `.env`, riavviare il devcontainer per far rileggere le variabili (Command Palette → “Dev Containers: Restart Container”).

    Troubleshooting rapido (errore `--env-file` / `.env` mancante):

    - Se il devcontainer fallisce in avvio con messaggi tipo “cannot open env file” o simili, significa che `.env` non è stato creato.
    - Fix: assicurarsi che `.env.example` esista nella root del repo, poi lanciare il devcontainer (sul PC host,    nella cartella del repo):

       - Windows (PowerShell):

          ```powershell
          if (!(Test-Path .env)) { Copy-Item .env.example .env }
          ```

       - macOS/Linux:

          ```bash
          [ -f .env ] || cp .env.example .env
          ```

    Poi: `F1` → “Dev Containers: Rebuild Container”.

5. **Verificare che MariaDB sia raggiungibile**

   Dal terminale nel container:

   ```bash
   mariadb -h 127.0.0.1 -P 3306 -u"${MARIADB_USER:-devuser}" -p"${MARIADB_PASSWORD:-devpass}" -e "SELECT 1;"
   ```

   Dall’host (Windows/macOS) è possibile collegarsi a `localhost:3306` perché VS Code fa il port-forward della porta.

6. **Creare/aggiornare il database tramite migrazioni (EF Core)**

   Questo progetto usa **Entity Framework Core**: prima di avviare l’API conviene creare lo schema DB con le migrazioni.
   Nel devcontainer `dotnet-ef` è già installato come tool.

   Nota (runtime .NET): il progetto ha come target `net9.0`. Anche se nel Dev Container è presente l’SDK .NET 10, per eseguire `dotnet ef` serve anche il runtime **.NET 9** (`Microsoft.NETCore.App`).
   Se compare l’errore:

   - “You must install or update .NET to run this application”
   - “Framework: 'Microsoft.NETCore.App', version '9.0.0' … The following frameworks were found: 10.x …”

   significa che manca il runtime 9: eseguire “Dev Containers: Rebuild Container” (oppure installare il runtime .NET 9 nel proprio ambiente).

   Nota (tool `dotnet-ef`): se il tool globale ha una major diversa dai package EF del progetto, di solito funziona comunque; in caso di problemi di compatibilità, installare `dotnet-ef` allineato alla versione di EF Core usata dal progetto.

    **Pin della versione di `dotnet-ef` (consigliato per coerenza)**

    Il progetto usa EF Core **9.x** (vedi `MyApi.csproj`), quindi un pin tipico è `9.0.12`.

    - Pin **globale** (vale per tutto l’utente `vscode` nel devcontainer):

       ```bash
       dotnet tool uninstall --global dotnet-ef
       dotnet tool install --global dotnet-ef --version 9.0.12
       dotnet ef --version
       ```

    - Pin **locale** (vale solo per questo repository, più riproducibile in team):

       ```bash
       # Crea il manifest se non esiste
       dotnet new tool-manifest

       # Installa/pinna il tool in .config/dotnet-tools.json
       dotnet tool install dotnet-ef --version 9.0.12

       # Usa il tool locale
       dotnet tool run dotnet-ef --version
       dotnet tool run dotnet-ef migrations add InitialCreate
       ```

   Dal terminale nel container:

   ```bash
   cd src/MyApi
   # Solo la prima volta (se non esistono migrazioni nel repo)
   dotnet ef migrations add InitialCreate

   # Applica (o aggiorna) lo schema sul database MariaDB
   dotnet ef database update
   ```

   Nota: se in futuro si modificano i modelli di EF, generare una nuova migration e rieseguire `dotnet ef database update`.

7. **Avviare l’app**

   Esempio (se si ha un progetto .NET nella cartella):

   ```bash
   dotnet run
   ```

## Tutorial completo su come funziona la configurazione di MariaDB e delle variabili d’ambiente nel Dev Container: [Database (MariaDB)](#️-database-mariadb) e [Usare le variabili di `.env` nel Dev Container (runArgs)](#-usare-le-variabili-di-env-nel-dev-container-runargs)

### 🗄️ Database (MariaDB)

Per configurare le variabili in locale, modificare `.env` (oppure usare `.env.example` come riferimento).

#### Come è reso disponibile nel Dev Container

- MariaDB Server è **installato nell’immagine** (package `mariadb-server`).
- All’avvio del devcontainer, uno **script di entrypoint** avvia `mariadbd` (senza systemd) e lo espone su `0.0.0.0:3306`.
- Alla prima esecuzione MariaDB inizializza i dati in `/var/lib/mysql` e crea (una sola volta) database + utente applicativo usando le variabili d’ambiente `MARIADB_DATABASE`, `MARIADB_USER`, `MARIADB_PASSWORD`.
- La porta `3306` viene resa raggiungibile dall’host via **port forwarding** di Dev Containers.

**Cosa fa lo script di MariaDB:**

Lo script [.devcontainer/mariadb-entrypoint.sh](.devcontainer/mariadb-entrypoint.sh) gestisce:

- Avvio di `mariadbd` (senza systemd) e attesa che il server sia pronto.
- Inizializzazione “one-shot” (una sola volta) basata su variabili d’ambiente:
   - `CREATE DATABASE` per `MARIADB_DATABASE` (se non esiste)
   - `CREATE USER` per l’utente applicativo `MARIADB_USER`@`%` (se non esiste)
   - `GRANT` dei privilegi su `MARIADB_DATABASE`.* a quell’utente
   - opzionale: update password di `root` se `MARIADB_ROOT_PASSWORD` è valorizzata
- Creazione del marker `/var/lib/mysql/.devcontainer-initialized` per non ripetere l’init ad ogni riavvio.

Nota: lo script non fa `source` di `.env`: usa solo le variabili già presenti nell’environment (nel devcontainer arrivano da `runArgs --env-file`).

Nota: in `.devcontainer/devcontainer.json` usiamo `forwardPorts` per includere `3306`. Non è necessario aggiungere anche `portsAttributes` per MariaDB: è una configurazione opzionale che serve principalmente per dare “metadati” alle porte (label/protocollo/azioni UI), ed è più rilevante per HTTP/HTTPS.

#### Perché MariaDB viene avviato “senza systemd”

Nei container Docker (e quindi anche nei Dev Containers) in genere **non gira un init system completo** come `systemd`.
Il container di solito esegue un numero ridotto di processi e non è una VM: per questo comandi come `systemctl start mariadb` spesso non funzionano o danno errori (es. “system has not been booted with systemd”).

Per rimanere compatibili e semplici, qui avviamo direttamente il demone `mariadbd` tramite script, aspettando che sia pronto e poi facendo l’inizializzazione necessaria.

Le variabili sono definite nel file `.env` e vengono iniettate nel container tramite `runArgs --env-file`.

In pratica, l'app ASP.NET Core si collega a MariaDB usando le credenziali applicative riportate sotto (non `root`).

#### Nota su `root` e password

In questa configurazione `root` serve solo per l’inizializzazione locale e per l’accesso “amministrativo” all’interno del container.

Nota importante: in MariaDB gli account sono distinti per `User` + `Host`.
Per esempio `root@localhost` e `root@127.0.0.1` possono avere **privilegi diversi** anche se la password è la stessa.
In questo container, in pratica, per `root` vale quanto segue:

- Funziona: `mariadb -h localhost -u root -p`
- Può non funzionare: `mariadb -h 127.0.0.1 -u root -p`

Se vuoi un accesso amministrativo affidabile, usa sempre il socket Unix:

- `sudo mariadb --protocol=socket --socket=/var/run/mysqld/mysqld.sock -u root -p`
- Modalità interattiva (più semplice da ricordare): `sudo mariadb -uroot -p` e poi inserire la password quando richiesta.

Perché succede / cosa cambia tra i metodi di accesso:

- `-h localhost`: il client MariaDB, su Linux, tende a usare il **socket Unix** (non TCP). Quindi l’accesso avviene come account `root@localhost`.
- `-h 127.0.0.1`: forzi il protocollo **TCP/IP** (loopback). In questo caso MariaDB valuta un account diverso, tipicamente `root@127.0.0.1` (o comunque una voce host diversa da `localhost`).
- `--protocol=socket --socket=...`: forzi esplicitamente il **socket Unix** e rendi il comportamento deterministico.

Impatto pratico:

- In MariaDB gli account sono separati per coppia `User`+`Host`, quindi `root@localhost` e `root@127.0.0.1` non sono lo stesso account.
- Plugin di autenticazione e password possono differire tra le due entry (es. una valida e l’altra no), quindi un comando può funzionare e l’altro fallire.
- Per amministrazione locale nel devcontainer, il socket è la scelta più stabile; per l’applicazione è meglio usare l’utente applicativo (`devuser`) via TCP.

Per questo, dentro al container, per comandi amministrativi conviene usare il socket:

- `mariadb --protocol=socket -u root -p`

e usare invece l’utente applicativo (`MARIADB_USER`, es. `devuser`) per l’app e per le migrazioni.

Se si vuole comunque impostare una password a `root`, ci sono due opzioni:

- Impostarla **automaticamente al primo avvio** valorizzando `MARIADB_ROOT_PASSWORD` (es. in `.env`).
- Impostarla **manualmente** entrando nel container e lanciando (da utente root) un comando tipo:
   - `mariadb --protocol=socket -uroot -e "ALTER USER 'root'@'localhost' IDENTIFIED BY 'nuova-password'; FLUSH PRIVILEGES;"`

Oppure, in alternativa, creare un utente admin dedicato e usare quello.

Nota: la procedura di init viene eseguita una sola volta (marker file in `/var/lib/mysql/.devcontainer-initialized`). Se si cambiano le variabili dopo il primo avvio, MariaDB non viene reinizializzato automaticamente.

#### Volumi: persistenza dei dati (opzionale)

Di default, MariaDB salva i dati in `/var/lib/mysql` **dentro al filesystem del container**:

- Sopravvive a **stop/start** del devcontainer.
- Non sopravvive a **“Dev Containers: Rebuild Container”** (che ricrea il container da zero).

Se si vuole che il database sopravviva anche ai rebuild (utile per progetti più lunghi), si può montare un **named volume** su `/var/lib/mysql`.
Nel file `.devcontainer/devcontainer.json` si trova un esempio già pronto (commentato) usando `mounts`.

Reset quando si usa un volume:

- Se si vuole ripartire da zero, la via più “pulita” è rimuovere il volume dal Docker host (fuori dal devcontainer):
   - `docker volume ls`
   - `docker volume rm <nome-volume>`
- In alternativa si possono cancellare i file da dentro al container (come nel reset completo), ma si sta comunque svuotando il volume.

#### Troubleshooting (MariaDB)

Se MariaDB parte ma non si trova il DB/utente atteso, oppure se sono state modificate le variabili in `.env` e si vuole rieseguire l’inizializzazione, queste sono le operazioni tipiche.

**Nota didattica: “CLI riutilizzabile” nello script Bash:**

In [.devcontainer/mariadb-entrypoint.sh](.devcontainer/mariadb-entrypoint.sh) si trova un pattern comune in Bash: costruire un comando in un **array** e riusarlo.
Serve per evitare di ripetere lo stesso comando `mariadb ...` in più punti e, soprattutto, per garantire che tutte le query usino sempre gli stessi parametri (protocollo socket e password se presente).

Esempio semplificato (come nello script):

```bash
mariadb_root_cli=(mariadb --protocol=socket --socket="${SOCKET_PATH}" -uroot)
if [ -n "${MARIADB_ROOT_PASSWORD}" ]; then
   mariadb_root_cli+=("-p${MARIADB_ROOT_PASSWORD}")
fi

"${mariadb_root_cli[@]}" -e "SELECT 1;"
"${mariadb_root_cli[@]}" -e "FLUSH PRIVILEGES;"
```

Nota: `"${array[@]}"` espande l’array in argomenti separati (quello giusto per eseguire comandi). Questo evita di dover “ricostruire” una stringa con quoting complesso.

**A) Rieseguire solo l’inizializzazione (senza cancellare i dati):**

1. Entrare nel devcontainer e aprire un terminale.
2. Rimuovere il marker “one-shot”:

   ```bash
   sudo rm -f /var/lib/mysql/.devcontainer-initialized
   ```

3. Riavviare il container (Command Palette → “Dev Containers: Restart Container”).

   In alternativa, senza riavviare, si può rieseguire lo script manualmente.
   Nota: eseguire lo script dalla cartella del repository (workspace): è la versione “sorgente”. La copia in `/usr/local/bin` viene creata al build dell’immagine e non si aggiorna finché non si rifà il rebuild.

   ```bash
   sudo -E bash .devcontainer/mariadb-entrypoint.sh true
   ```

   Nota: se l'utente `root` di MariaDB ha già una password impostata, assicurarsi che `MARIADB_ROOT_PASSWORD` sia valorizzata (es. in `.env`), altrimenti lo script non riesce ad autenticarsi per eseguire le query di init.

**B) Reset completo del database (cancella TUTTI i dati):**

   Usare solo per ripartire da zero:

   ```bash
   sudo rm -f /var/lib/mysql/.devcontainer-initialized
   sudo rm -rf /var/lib/mysql/*
   ```

   Poi riavviare il devcontainer.

**C) `.env` su Windows (CRLF) e valori “strani”**

Se si crea/modifica `.env` su Windows, potrebbe avere fine-riga `CRLF`. In Bash questo può introdurre un carattere `\r` nei valori (es. `devdb\r`) e causare errori o comportamenti inattesi.
Lo script rimuove automaticamente `\r` dai valori più importanti, ma **è comunque una buona pratica salvare `.env` con fine riga `LF`**.

- Host: `127.0.0.1`
- Porta: `3306`
- Database: `pizzadb`
- User: `devuser`
- Password: `devpass`

Questi sono i valori **di default** usati se non imposti `.env` (o se alcune variabili mancano).

### 🔐 Root del container (Sistema Operativo)

Per evitare confusione (soprattutto all’inizio): nel progetto esistono **due “root” diversi**.

- **`root` del sistema operativo (Linux nel container)**: è l’utente amministratore del container (permessi totali sui file, installazione pacchetti, ecc.).
- **`root` di MariaDB**: è l’utente amministratore del database.

#### Qual è la password di `root` (Linux)?

Nei Dev Containers, in genere **non si usa una password** per diventare `root`.
Di solito si lavora come utente non-root (`vscode`) e, quando serve, si ottengono privilegi admin tramite `sudo`.

- Aprire un terminale nel container e usare: `sudo -s` (oppure `sudo su -`).

Questa cosa è indipendente da `MARIADB_ROOT_PASSWORD`: quella variabile riguarda **solo** l’utente `root` del database, non l’utente `root` del sistema operativo.

#### Nota: perché nel `Dockerfile` usiamo `DEBIAN_FRONTEND=noninteractive` ed eventualmente `--no-install-recommends`

Quando installiamo pacchetti con `apt-get` dentro un Dockerfile, ci sono due opzioni comuni che rendono la build più affidabile:

- `DEBIAN_FRONTEND=noninteractive`: evita prompt interattivi durante l’installazione (es. scelte su timezone o configurazioni). In una build Docker non puoi rispondere “a mano”, quindi senza questa opzione la build può bloccarsi.
- `--no-install-recommends`: installa solo le dipendenze **necessarie** ("Depends") e non anche quelle solo “consigliate” ("Recommends"). Risultato: immagine più piccola e meno pacchetti inutili.

#### Nota: perché nel `Dockerfile` configuriamo Git (`init.defaultBranch` e `safe.directory`)

Nel Dockerfile ci sono due comandi utili per evitare problemi comuni quando si lavora in un Dev Container:

- `git config --system init.defaultBranch main`
   - Imposta `main` come nome di branch predefinito quando si esegue `git init`.
   - Evita differenze tra PC diversi (alcuni usano ancora `master` di default) e rende i tutorial più coerenti.

- `git config --system --add safe.directory "/workspaces/*"`
   - Nei Dev Containers, i file del workspace sono spesso montati da Windows/macOS dentro Linux e possono risultare “di proprietà” di un utente diverso.
   - Git, per sicurezza, può mostrare l’errore/avviso **“dubious ownership”** e rifiutarsi di operare sul repo.
   - Con `safe.directory` si dice a Git che quel percorso è considerato affidabile.

Nota didattica: evitare `safe.directory "*"` è meglio, perché renderebbe affidabile *qualsiasi* percorso nel container. Limitare a `/workspaces/*` risolve il problema tipico dei Dev Containers riducendo il rischio.

### 📚 Mini-tutorial: file “di contorno” del repo

Questi file non sono “codice applicativo”, ma sono fondamentali per lavorare bene in team e in modo riproducibile.

## 📚 File di contorno: `.gitignore`, `.dockerignore` e `.gitattributes`

### `.gitignore`

Serve a evitare di committare file locali (build output, file IDE, segreti).
In particolare `.env` è ignorato apposta.

### `.dockerignore`

Serve a non inviare al Docker build context file inutili o sensibili, così:

- build più veloce (meno file trasferiti/coperti)
- cache migliore (meno layer invalidati da file che cambiano spesso)
- minor rischio di includere segreti nell’immagine (es. `.env`)

### `.gitattributes`

Serve a garantire che i file di testo siano salvati con fine riga `LF` (comportamento standard su Linux e consigliato per i progetti multi-OS).

### `.env`

File di variabili d’ambiente **locale**: vale solo sul proprio PC.
In generale non va committato perché può contenere password.

In questo repo `.env` è **ignorato da Git**.
Per ridurre gli errori in aula, il devcontainer crea `.env` automaticamente copiando `.env.example` (prima di avviare il container).

In questo progetto il file `.env` può contenere, ad esempio:

- `MARIADB_DATABASE`, `MARIADB_USER`, `MARIADB_PASSWORD`
- `MARIADB_ROOT_PASSWORD` (opzionale)

### `.env.example`

È un “template” committabile: contiene **le stesse chiavi** di `.env`, ma con valori di esempio/placeholder.

Workflow tipico:

1. Copiare `.env.example` in `.env`
2. Personalizzare i valori
3. Non committare `.env`

### 🧯 Trovare PID e fermare processi (Linux / Dev Container)

Quando lanci un comando nel terminale (es. `dotnet run`) il processo può restare attivo finché non lo interrompi.

**Stop “pulito” (foreground):**

- Se il processo sta girando nel terminale in primo piano, premi `Ctrl+C` nel *medesimo* terminale.

**Trovare il PID (Process ID):**

- Lista processi con PID e comando (output più leggibile di `ps aux`):

   ```bash
   ps -eo pid,cmd
   ```

- Filtra per parola chiave (trucco `[d]otnet` evita di matchare la riga del `grep` stesso):

   ```bash
   ps -eo pid,cmd | grep -E '[d]otnet'
   ```

**Caso particolare: `dotnet` (trovare i PID giusti)**

- PID dei processi avviati con `dotnet run`:

   ```bash
   pgrep -f "dotnet run"
   ```

- PID dei processi avviati con `dotnet watch`:

   ```bash
   pgrep -f "dotnet watch"
   ```

**Fermare un processo usando il PID:**

- Tentativo “gentile” (SIGTERM):

   ```bash
   kill <PID>
   ```

- Se non si ferma (forzato, SIGKILL):

   ```bash
   kill -9 <PID>
   ```

**Fermare tutti i `dotnet run` (attenzione: li chiude tutti)**

   ```bash
   pkill -f "dotnet run"
   ```

### 🧩 Usare le variabili di `.env` nel Dev Container (runArgs)

In questo repo usiamo l’approccio più semplice per studenti: `runArgs` con `--env-file`.
Così le variabili sono disponibili in terminali e processi del container (es. `dotnet run`).

Esempio in `.devcontainer/devcontainer.json` aggiungere `runArgs`:

```jsonc
{
   "runArgs": ["--env-file", "${localWorkspaceFolder}/.env"]
}
```

Nota importante: se `.env` **non esiste**, Docker tende a fallire l’avvio del container. Quindi questo metodo va bene quando si ha la certezza che ogni utilizzatore del container abbia il file `.env` già creato `.env` partendo da `.env.example`.

Nota: in questo repo `.env` viene creato automaticamente da `.env.example` per evitare l’errore di avvio dovuto al file mancante.

### 🧪 Tutorial completo: rendere `.env` disponibile nella shell del container

Obiettivo: aprire un terminale nel devcontainer e avere comandi come `echo "$MARIADB_DATABASE"` funzionanti.

#### `runArgs` + `--env-file` (variabili globali nel container)

Questa è l’opzione più “globale”: le variabili diventano parte dell’environment del container e quindi sono visibili in ogni terminale e processo.

1) Aggiungere `runArgs` in `.devcontainer/devcontainer.json`

   ```jsonc
   {
      "runArgs": ["--env-file", "${localWorkspaceFolder}/.env"]
   }
   ```

2) Applicare le modifiche

   - `F1` → “Dev Containers: Rebuild Container”

#### Verifica rapida

Dentro al devcontainer:

```bash
printenv | grep '^MARIADB_'
echo "$MARIADB_DATABASE"
```

### Nome immagine e nome container (perché a volte Docker “sceglie a caso”)

Di default, quando VS Code/Dev Containers crea e avvia un container, Docker può usare nomi generati automaticamente.
In questo progetto invece impostiamo:

- un nome container deterministico (`<nome-cartella>-devcontainer`)

⚠️ Nota importante: in Dev Containers esistono **due “nomi” diversi** (e possono coesistere):

- `"name"` in `.devcontainer/devcontainer.json`: è il **nome del Dev Container per VS Code** (etichetta UI). Non impone il nome del container Docker.
- `"runArgs": ["--name", "..."]`: impone il **nome del container Docker** (quello visibile in `docker ps`).

In breve: se è sufficiente una label leggibile in VS Code, basta `name`. Se invece si vuole un nome Docker deterministico per debug e per esercitazioni, allora serve anche `--name`.

Questo rende più facile per gli utenti fare debug (es. vedere subito il container giusto in `docker ps`).

Nota: in configurazione Dev Containers, quando si usa `build` (Dockerfile), il tag dell’immagine è gestito automaticamente da Dev Containers. Se si vuole un tag immagine 100% deterministico, in genere serve usare la CLI `devcontainer build --image-name ...` (workflow più avanzato).

Nota: se Docker segnala che il nome container è “già in uso”, significa che esiste già un container con quel nome. In tal caso:

- rimuovere il vecchio container (da host): `docker rm -f <nome-container>`
- oppure usare “Dev Containers: Rebuild Container” (di solito ricrea il container)
