# Introduzione ai Dev Containers

- [Introduzione ai Dev Containers](#introduzione-ai-dev-containers)
  - [Perché utilizzare i Dev Containers](#perché-utilizzare-i-dev-containers)
  - [Concetti fondamentali](#concetti-fondamentali)
  - [Containerizzazione come fondamento](#containerizzazione-come-fondamento)
    - [Cosa fa Docker?](#cosa-fa-docker)
    - [Container vs Macchine Virtuali](#container-vs-macchine-virtuali)
  - [Immagini base](#immagini-base)
  - [Struttura di un Dev Container](#struttura-di-un-dev-container)
  - [Prerequisiti e installazione](#prerequisiti-e-installazione)
    - [Software necessario](#software-necessario)
    - [Installazione su Windows](#installazione-su-windows)
    - [Installazione su macOS](#installazione-su-macos)
    - [Installazione su Linux](#installazione-su-linux)
  - [Configurazione di base di Dev Container](#configurazione-di-base-di-dev-container)
    - [Il file `devcontainer.json`](#il-file-devcontainerjson)
      - [Proprietà principali](#proprietà-principali)
      - [Features](#features)
        - [Features ufficiali](#features-ufficiali)
        - [Sintassi di utilizzo](#sintassi-di-utilizzo)
        - [Features della community](#features-della-community)
      - [forwardPorts](#forwardports)
      - [Customizations (VS Code)](#customizations-vs-code)
      - [mounts](#mounts)
      - [postCreateCommand](#postcreatecommand)
      - [postStartCommand](#poststartcommand)
      - [remoteUser](#remoteuser)
      - [containerEnv](#containerenv)
    - [Dockerfile personalizzati](#dockerfile-personalizzati)
      - [Referenziare un Dockerfile nel devcontainer.json](#referenziare-un-dockerfile-nel-devcontainerjson)
      - [Esempio di Dockerfile personalizzato](#esempio-di-dockerfile-personalizzato)
      - [Esempio completo di progetto con Dev Container e Dockerfile personalizzato](#esempio-completo-di-progetto-con-dev-container-e-dockerfile-personalizzato)
  - [Docker Compose nei Dev Containers](#docker-compose-nei-dev-containers)
    - [Caso 1: Dev Container solo per l'app (DB già esistente su rete Docker esterna)](#caso-1-dev-container-solo-per-lapp-db-già-esistente-su-rete-docker-esterna)
      - [Esempio completo di progetto con Dev Container con rete esterna e DB separato](#esempio-completo-di-progetto-con-dev-container-con-rete-esterna-e-db-separato)
    - [Caso 2: Stack completo in Compose (app + MariaDB nello stesso docker-compose.yml)](#caso-2-stack-completo-in-compose-app--mariadb-nello-stesso-docker-composeyml)
      - [Esempio completo di progetto con Dev Container, Database e rete gestiti da Docker Compose](#esempio-completo-di-progetto-con-dev-container-database-e-rete-gestiti-da-docker-compose)
    - [Template Dev Container per Docker Compose (multi-container) - Starter project](#template-dev-container-per-docker-compose-multi-container---starter-project)
  - [Prebuilds](#prebuilds)
    - [Vantaggi dei prebuilds](#vantaggi-dei-prebuilds)
    - [Configurazione GitHub Codespaces](#configurazione-github-codespaces)
      - [Nota su secrets e file `.env`](#nota-su-secrets-e-file-env)
      - [Progetto docker-compose-full-starter-codespace-ready](#progetto-docker-compose-full-starter-codespace-ready)
      - [Opzionale: build/push immagine via GitHub Actions](#opzionale-buildpush-immagine-via-github-actions)
        - [Prerequisiti](#prerequisiti)
        - [Procedura operativa](#procedura-operativa)
        - [Workflow esempio (build + push su GHCR)](#workflow-esempio-build--push-su-ghcr)
        - [Come usarla (opzionale)](#come-usarla-opzionale)
        - [Troubleshooting rapido](#troubleshooting-rapido)
      - [Uso in classe: template, fork e lavoro a gruppi](#uso-in-classe-template-fork-e-lavoro-a-gruppi)
        - [Opzione 1: Template repository (consigliata)](#opzione-1-template-repository-consigliata)
          - [Cosa fai il docente una sola volta](#cosa-fai-il-docente-una-sola-volta)
          - [Cosa fa ogni studente (Template)](#cosa-fa-ogni-studente-template)
          - [Note importanti](#note-importanti)
        - [Opzione 2: Fork (se consentito)](#opzione-2-fork-se-consentito)
          - [Cosa fai il docente](#cosa-fai-il-docente)
          - [Cosa fa ogni studente (Fork)](#cosa-fa-ogni-studente-fork)
          - [Costi/crediti (Fork, in genere)](#costicrediti-fork-in-genere)
        - [Opzione 3: Lavoro a gruppi (collaborazione)](#opzione-3-lavoro-a-gruppi-collaborazione)
          - [Setup consigliato (repo per gruppo nell’organizzazione GreppiDev)](#setup-consigliato-repo-per-gruppo-nellorganizzazione-greppidev)
          - [Come collaborano (workflow minimo)](#come-collaborano-workflow-minimo)
          - [Costi/crediti (Gruppi, in genere)](#costicrediti-gruppi-in-genere)
  - [Best Practices](#best-practices)
    - [Organizzazione del progetto](#organizzazione-del-progetto)
    - [Sicurezza](#sicurezza)
    - [Performance](#performance)
    - [Esempio di struttura progetto](#esempio-di-struttura-progetto)

Un **development container** è, essenzialmente, un container [Docker](https://www.docker.com/) in esecuzione che include uno stack di strumenti (tool/runtime) ben definiti e i relativi prerequisiti.

A differenza di un ambiente locale tradizionale, dove SDK e dipendenze vengono installate direttamente sul sistema operativo ospitante (creando potenziali conflitti tra versioni diverse del framework .NET), un Devcontainer isola l'intero ambiente. Questo approccio permette di:

- **Separare** strumenti, librerie e runtime necessari per lavorare con una specifica codebase (es. isolare un progetto .NET 9 da uno legacy in .NET Core 3.1).

- **Utilizzare** il container per eseguire l'applicazione, come ad esempio una Minimal API.

- **Facilitare** l'integrazione continua (CI) e il testing, garantendo che l'ambiente di sviluppo sia identico a quello di build e, idealmente, di produzione.

## Perché utilizzare i Dev Containers

I vantaggi principali derivanti dall'uso dei Dev Containers sono:

- **Replicabilità**: ogni sviluppatore lavora con lo stesso ambiente identico

- **Isolamento**: le dipendenze del progetto non interferiscono con il sistema host

- **Onboarding semplificato**: nuovi sviluppatori operativi in minuti, non in giorni

- **Consistenza tra ambienti**: sviluppo, testing e produzione sono allineati

- **Versioning**: l'ambiente è versionato insieme al codice sorgente

## Concetti fondamentali

Per comprendere appieno il funzionamento dei Dev Containers, è necessario familiarizzare con alcuni concetti chiave che costituiscono la base di questa tecnologia.

Le specifiche dei dev containers sono in continua evoluzione e sono gestite dal progetto open source [Dev Containers](https://containers.dev/), che fornisce linee guida, strumenti e risorse per l'automazione della creazione e gestione di ambienti di sviluppo containerizzati. In particolare, i dettagli tecnici delle specifiche dei dev containers sono disponibili all'indirizzo [Development Container Specification](https://github.com/devcontainers/spec).

La [Development Container Specification](https://containers.dev/implementors/spec/) fornisce informazioni dettagliate per gli sviluppatori che desiderano implementare o estendere le funzionalità dei dev containers. In particolare, la sezione [Dev Container metadata reference](https://containers.dev/implementors/json_reference/) descrive in dettaglio il formato e le opzioni disponibili nel file di configurazione `devcontainer.json`.

## Containerizzazione come fondamento

I Dev Containers per poter funzionare necessitano di:

- un motore di containerizzazione (es. Docker Desktop, OrbStack o Podman).
- Un editor compatibile (es. Visual Studio Code con l'estensione Dev Containers o JetBrains IntelliJ IDEA, etc.).

Tra le piattaforme di containerizzazione, **Docker** è la più diffusa e utilizzata in ambito enterprise. In questa guida, ci concentreremo principalmente su Docker, ma i concetti e le best practices presentati sono applicabili anche ad altre piattaforme compatibili con le specifiche dei dev containers.

### Cosa fa Docker?

Docker permette di impacchettare un'applicazione con tutte le sue dipendenze (librerie, runtime, configurazioni) in un'unità standardizzata chiamata **container**. Questo garantisce che l'applicazione funzioni in modo identico su qualsiasi ambiente: sviluppo locale, testing o produzione.

### Container vs Macchine Virtuali

| Caratteristica | Container | Macchine Virtuali |
| --- | --- | --- |
| **Isolamento** | Process-level | Hardware-level |
| **Kernel** | Condivide il kernel dell'host | Kernel dedicato per VM |
| **Avvio** | Secondi | Minuti |
| **Overhead** | Basso (~MB) | Alto (~GB) |

A differenza delle macchine virtuali, i container **condividono il kernel del sistema operativo host** anziché virtualizzare l'intero stack hardware. Questo li rende:

- **Più leggeri**: occupano pochi MB invece di GB
- **Più veloci**: avvio in millisecondi/secondi invece di minuti
- **Più efficienti**: consumano meno risorse CPU e memoria

## Immagini base

Il punto di partenza per ogni Dev Container è un'immagine Docker.

Il progetto Dev Containers mette a disposizione un repository di **immagini ufficiali** ottimizzate per lo sviluppo, disponibile all'indirizzo [Dev Containers images](https://github.com/devcontainers/images).

Queste immagini coprono i principali linguaggi e framework:

- **.NET** (C#, F#, VB.NET)
- **Node.js** / JavaScript / TypeScript
- **Python**
- **Go**
- **Java**
- **Ruby**
- E molti altri

L'elenco completo delle immagini ufficiali è disponibile nella [sezione `src` delle immagini](https://github.com/devcontainers/images/tree/main/src), e include dettagli su quali versioni degli SDK e runtime sono incluse in ciascuna immagine.

## Struttura di un Dev Container

Un Dev Container si compone tipicamente di tre elementi principali:

1. **devcontainer.json**: File di configurazione principale che definisce l'ambiente
2. **Dockerfile (opzionale)**: Per personalizzare l'immagine base con strumenti aggiuntivi
3. **docker-compose.yml (opzionale)**: Per configurazioni multi-container

## Prerequisiti e installazione

Per iniziare a utilizzare i Dev Containers, è necessario configurare correttamente l'ambiente di lavoro. I requisiti variano leggermente a seconda del sistema operativo utilizzato.

### Software necessario

La configurazione di base che verrà usata in questa guida è composta da:

- Visual Studio Code

- Estensione Dev Containers per VS Code

- Docker Desktop (Windows/macOS) o Docker Engine (Linux)

- Git per il versionamento del codice

### Installazione su Windows

Su Windows, si consiglia l'installazione di Docker Desktop con il WSL2 backend. Questa configurazione offre le migliori prestazioni e compatibilità. Dopo l'installazione di Docker Desktop, è necessario abilitare l'integrazione con WSL2 dalle impostazioni dell'applicazione.

```bash
# Verifica dell'installazione Docker
docker --version
docker-compose --version
```

### Installazione su macOS

Su macOS, Docker Desktop è disponibile sia per architetture Intel che Apple Silicon. L'installazione avviene tramite il pacchetto DMG scaricabile dal sito ufficiale di Docker. Anche in questo caso, si consiglia di verificare il corretto funzionamento tramite riga di comando.

### Installazione su Linux

Su Linux, Docker Engine può essere installato direttamente tramite il package manager della distribuzione. Non è necessario Docker Desktop, anche se disponibile come opzione. L'estensione Dev Containers di VS Code funziona nativamente con Docker Engine.

## Configurazione di base di Dev Container

La configurazione di un Dev Container avviene principalmente attraverso il file `devcontainer.json`. Questo file JSON contiene tutte le impostazioni necessarie per definire l'ambiente di sviluppo.

### Il file `devcontainer.json`

Il file `devcontainer.json` rappresenta il cuore della configurazione di un Dev Container. Può essere posizionato nella cartella `.devcontainer` alla radice del progetto o in un percorso personalizzato. La sua struttura è flessibile e permette di specificare numerosi parametri.

```json
{
    "name": "Il mio Dev Container",
    "image": "mcr.microsoft.com/devcontainers/dotnet:9.0-noble",
    "workspaceFolder": "/workspaces/${localWorkspaceFolderBasename}",
    "features": {
        "ghcr.io/devcontainers/features/github-cli:1": {}
    },
    "customizations": {
        "vscode": {
            "extensions": [
                "ms-dotnettools.csharp",
                "ms-dotnettools.vscode-dotnet-runtime"
            ]
        }
    },
    "forwardPorts": [5000, 5001],
    "postCreateCommand": "dotnet restore"
}
```

Nota: `workspaceFolder` è opzionale nei setup “singolo container” (VS Code usa un default), ma è utile per rendere esplicito il path di lavoro e diventa importante quando usi Docker Compose, mount personalizzati o una struttura non standard.

#### Proprietà principali

| **Proprietà** | **Descrizione** |
| --- | --- |
| `name` | Nome descrittivo del Dev Container |
| `image` | Immagine Docker di base da utilizzare |
| `dockerComposeFile` | Percorso del file docker-compose.yml per configurazioni multi-container |
| `build.context` | Contesto di build per Dockerfile personalizzati |
| `build.dockerfile` | Percorso del Dockerfile personalizzato |
| `features` | Features da installare nell'ambiente |
| `forwardPorts` | Porte da esporre dall'host al container |
| `portsAttributes` | Configurazione avanzata delle porte esposte |
| `customizations` | Personalizzazioni specifiche per l'editor |
| `customizations.vscode.extensions` | Estensioni VS Code da installare automaticamente |
| `customizations.vscode.settings` | Impostazioni VS Code personalizzate |
| `mounts` | Configurazione dei volumi e dei mount point |
| `postCreateCommand` | Comando eseguito dopo la creazione |
| `postStartCommand` | Comando eseguito dopo l'avvio del container |
| `remoteUser` | Utente da utilizzare all'interno del container |
| `containerEnv` | Variabili d'ambiente da impostare nel container |

L'elenco completo delle proprietà disponibili è documentato nella [Dev Container metadata reference](https://containers.dev/implementors/json_reference/).

#### Features

Le Features rappresentano uno dei meccanismi più potenti per personalizzare i Dev Containers. Si tratta di set predefiniti di strumenti e runtime che possono essere aggiunti alla configurazione senza dover creare un Dockerfile personalizzato. Sul sito ufficiale di Dev Containers è disponibile un [elenco completo di Features](https://containers.dev/features), sia ufficiali che della community, che coprono una vasta gamma di strumenti e tecnologie.

##### Features ufficiali

Il repository [github.com/devcontainers/features](https://github.com/devcontainers/features) contiene le Features ufficiali mantenute dal team di Dev Containers. Queste Features coprono gli strumenti più comunemente utilizzati dagli sviluppatori:

| **Feature** | **Descrizione** |
| --- | --- |
| dotnet | Installa .NET SDK e runtime |
| node | Installa Node.js e npm |
| docker-in-docker | Permette di eseguire Docker dentro al container |
| github-cli | Installa GitHub CLI (gh) |
| azure-cli | Installa Azure CLI |
| terraform | Installa Terraform |
| kubectl-helm-minikube | Installa kubectl, Helm e Minikube |

##### Sintassi di utilizzo

Le Features vengono dichiarate nel file `devcontainer.json` utilizzando la proprietà `features`. Ogni Feature è identificata da un URI univoco e può accettare opzioni di configurazione:

```json
{    
  "features": {
      "ghcr.io/devcontainers/features/node:1": {
        "version": "lts"
      },
          "ghcr.io/devcontainers/features/docker-in-docker:2": {},
          "ghcr.io/devcontainers/features/github-cli:1": {}
      }
}
```

##### Features della community

Oltre alle Features ufficiali, la community ha sviluppato numerose estensioni che coprono esigenze specifiche. I principali repository di Features non ufficiali includono:

- [Devcontainer community features](https://github.com/devcontainer-community/devcontainer-features)
- [Devcontainers extra features](https://github.com/devcontainers-extra/features)
- [Containers.dev features](https://containers.dev/features)

Per utilizzare una Feature della community, è sufficiente specificare il repository di origine:

```json
{
    "features": {
        "ghcr.io/devcontainers-extra/features/angular-cli:2": {}
    }
}
```

#### forwardPorts

La proprietà `forwardPorts` espone porte dal container verso l'host, permettendo l'accesso da browser, client HTTP (Postman, httpie), ecc. È un array di numeri interi.

```json
"forwardPorts": [5000, 5001, 8080, 3306]
```

Nota: in ambienti **Docker Compose**, la comunicazione **container → container** usa DNS interno e non richiede `forwardPorts`. Serve solo se si vuole accedere da fuori il network Docker.

#### Customizations (VS Code)

`customizations.vscode` personalizza l'esperienza in VS Code con estensioni e impostazioni dell'editor.

```json
"customizations": {
  "vscode": {
    "extensions": [
      "ms-dotnettools.csharp",
      "ms-dotnettools.csdevkit",
      "ms-azuretools.vscode-docker"
    ],
    "settings": {
      "editor.formatOnSave": true,
      "omnisharp.enableRoslynAnalyzers": true
    }
  }
}
```

Le estensioni elencate verranno installate automaticamente al primo avvio del Dev Container; le impostazioni sovrascrivono le proprie preferenze globali solo dentro il container.

#### mounts

`mounts` configura i volumi e i mount point, permettendo di condividere file/directory tra host e container (es. configurazioni Git, auth token, ecc.).

```json
"mounts": [
  "source=${localEnv:USERPROFILE}/.gitconfig,target=/home/vscode/.gitconfig,type=bind,readonly",
  "source=${localEnv:USERPROFILE}/.ssh,target=/home/vscode/.ssh,type=bind,readonly"
]
```

Sintassi: `source=<path-host>,target=<path-container>,type=bind[,readonly]` o equivalenti Docker Compose.

#### postCreateCommand

`postCreateCommand` è un comando shell eseguito **una sola volta** dopo la creazione del container. Utile per restore, setup iniziale, e trust certificati HTTPS.

```json
"postCreateCommand": "dotnet restore && dotnet dev-certs https --trust"
```

Se il comando fallisce, il container continua comunque ad avviarsi. Usare `|| true` o `if ... then ... fi` per gestire errori graceful.

#### postStartCommand

`postStartCommand` è un comando shell eseguito **ad ogni avvio** del container (a differenza di `postCreateCommand` che corre solo una volta). Utile per operazioni ricorrenti come sincronizzazione file, aggiornamento dipendenze, avvio servizi, ecc.

```json
"postStartCommand": "dotnet watch run"
```

Tipicamente è meno "pesante" di `postCreateCommand`: mentre il primo fa setup iniziale (restore, restore di EF, ecc.), il secondo può eseguire il watch del progetto o avviare servizi secondari.

#### remoteUser

`remoteUser` specifica l'utente con cui VS Code apre il terminale, esegue task, debug, ecc. all'interno del container.

```json
"remoteUser": "vscode"
```

Nota: dev containers ufficiali (es. dotnet:9.0-noble) includono già l'utente `vscode` e lo configurano per lavorare senza privilegi. Se usi `root`, tutti i file creati avranno permessi root, causando problemi su mount/volumi.

#### containerEnv

`containerEnv` imposta variabili d'ambiente visibili inside il container. Utile per configuration ASP.NET, connection string, ecc.

```json
"containerEnv": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "ASPNETCORE_URLS": "https://+:5001;http://+:5000",
  "ConnectionStrings__Default": "Server=mariadb;Port=3306;Database=mydb;User=devuser;Password=devpass;"
}
```

Nota: non mescolare con secrets sensibili (usa `.env` file o secret manager); `remoteUser` e `containerEnv` sono tutti visibili in chiaro nel JSON.

### Dockerfile personalizzati

Quando le immagini predefinite non soddisfano le esigenze del progetto, è possibile creare un Dockerfile personalizzato. Questo approccio offre il massimo controllo sull'ambiente di sviluppo.

#### Referenziare un Dockerfile nel devcontainer.json

Per utilizzare un Dockerfile personalizzato, è necessario specificare le proprietà `build.dockerfile` e `build.context` nel file `devcontainer.json`:

```json
{
  "name": "Custom .NET Environment",
  "build": {
    "dockerfile": "Dockerfile",
    "context": ".."
  },
  "remoteUser": "vscode"
}
```

**Proprietà `build`:**

- **`dockerfile`**: percorso del Dockerfile **relativo al `context`**. Se il Dockerfile è in `.devcontainer/Dockerfile` e il context è `..` (parent directory), allora si usa `"dockerfile": ".devcontainer/Dockerfile"` oppure, se il `devcontainer.json` è già in `.devcontainer/`, basta `"dockerfile": "Dockerfile"`.

- **`context`**: directory di build (build context) per Docker. Definisce la "radice" da cui Docker può copiare file durante la build (es. con `COPY` o `ADD` nel Dockerfile).
  - `.` (default se omesso) → il context è la stessa directory del `devcontainer.json`
  - `..` → il context è la directory parent (tipico se `.devcontainer/` è una sottocartella e si vuole accedere ai file del progetto)
  - path assoluto o relativo personalizzato

**Perché il context è importante:**

Quando Docker esegue `docker build`, riceve **solo i file presenti nel context** (e sottodirectory). Se nel Dockerfile usi:

```Dockerfile
COPY src/ /app/src/
```

Docker può copiare `src/` **solo se** `src/` esiste nel context (es. se context è `..`, allora `../src` deve esistere dalla prospettiva di `.devcontainer/`).

Esempio pratico di struttura:

```text
my-project/
├── .devcontainer/
│   ├── devcontainer.json  ← context: ".."
│   └── Dockerfile          ← dockerfile: "Dockerfile"
├── src/
│   └── Program.cs
└── MyProject.sln
```

In questo caso, Docker vede l'intera directory `my-project/` (grazie a `context: ".."`) e può fare `COPY src/ ...` o `COPY MyProject.sln ...` nel Dockerfile.

#### Esempio di Dockerfile personalizzato

```Dockerfile
FROM mcr.microsoft.com/devcontainers/dotnet:10.0-noble

USER root

# Installa strumenti utili per lo sviluppo
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    git \
    curl \
    vim \
    nano \
    htop \
    tree \
    jq \
    ca-certificates \
    gnupg \
    iproute2 \
    iputils-ping \
    dnsutils \
    netcat-openbsd \
    openssl \
    procps \
    lsof \
    mariadb-client \
    mariadb-server \
    httpie \
    yq \
    && rm -rf /var/lib/apt/lists/*

# Se il progetto ha come target net9.0 anche con SDK .NET 10 serve il runtime .NET 9 per eseguire app/tool net9.
# Installiamo quindi i runtime 9 affiancati a quelli 10 nel DOTNET_ROOT del container.
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --runtime dotnet --channel 9.0 --no-path --skip-non-versioned-files \
    && /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --runtime aspnetcore --channel 9.0 --no-path --skip-non-versioned-files \
    && rm /tmp/dotnet-install.sh

# Configura Git (opzionale)
RUN git config --system init.defaultBranch main
RUN git config --system --add safe.directory "/workspaces/*"

# Installa global tools .NET per l'utente con cui lavorerai nel Dev Container
USER vscode
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

# Avvio MariaDB (senza systemd) quando parte il container
USER root
COPY .devcontainer/mariadb-entrypoint.sh /usr/local/bin/mariadb-entrypoint.sh
RUN chmod +x /usr/local/bin/mariadb-entrypoint.sh

# Torna all'utente di default del devcontainer (l'avvio di MariaDB è gestito via devcontainer.json)
USER vscode


```

Nota:

- I pacchetti installati con `apt-get install` (es. `git`, `curl`, `jq`) scrivono in directory di sistema come `/usr/bin` e `/usr/lib`, quindi richiedono privilegi di `root`.
- `dotnet tool install --global` installa invece nella *home dell'utente corrente*:
  - se eseguito come `root` finisce in `/root/.dotnet/tools`
  - se eseguito come `vscode` finisce in `/home/vscode/.dotnet/tools`
  Per questo, se nel Dev Container si lavora come `vscode` (tipicamente con `"remoteUser": "vscode"` in devcontainer.json), conviene installare i tool globali .NET come `vscode`.
- `USER` nel Dockerfile cambia l'utente di default dell'immagine/container; `remoteUser` in devcontainer.json cambia l'utente con cui VS Code apre la sessione e lancia terminale/task/debug.

Oltre agli strumenti di base, è possibile aggiungere al Dockerfile ulteriori pacchetti utili per lo sviluppo, in particolare quando si lavora con applicazioni web (REST) e database come MariaDB o Redis.

Strumenti utili (REST + MariaDB + Redis):

- **Networking / troubleshooting**: `iproute2`, `iputils-ping`, `dnsutils` (es. `dig`), `netcat-openbsd` (nc), `openssl`
- **Diagnostica processi/porte**: `procps` (ps/top), `lsof` ("List Open Files")
- **Client DB/Cache**: `mariadb-client` (o `default-mysql-client`), `redis-tools` (redis-cli)

Esempio di installazione (opzionale) da aggiungere allo stesso blocco `apt-get install`:

```Dockerfile
# Strumenti utili per lo sviluppo nel Dockerfile personalizzato
RUN apt-get update && apt-get install -y \
  iproute2 iputils-ping dnsutils netcat-openbsd openssl \
  procps lsof \
  mariadb-client redis-tools \
  && rm -rf /var/lib/apt/lists/*
```

- **Testing HTTP (REST)**

  Pacchetti utili: `httpie` (client HTTP più comodo di curl), `yq` (se si lavora con YAML).

  ```Dockerfile
    # Strumenti utili per lo sviluppo nel Dockerfile personalizzato
    RUN apt-get update && apt-get install -y \
    httpie yq \
    && rm -rf /var/lib/apt/lists/*
  ```

- **Opzionali (solo quando serve “deep troubleshooting”)**

  ```Dockerfile
    # Strumenti utili per lo sviluppo nel Dockerfile personalizzato
    RUN apt-get update && apt-get install -y \
    traceroute mtr-tiny tcpdump nmap strace \
    && rm -rf /var/lib/apt/lists/*
  ```

- **.NET diagnostica (global tools)**

  ```Dockerfile
  # Strumenti utili per lo sviluppo nel Dockerfile personalizzato
  USER vscode
  RUN dotnet tool install --global dotnet-counters \
   && dotnet tool install --global dotnet-trace \
   && dotnet tool install --global dotnet-dump
  ENV PATH="${PATH}:/home/vscode/.dotnet/tools"
  ```

Il Dockerfile deve essere referenziato nel file devcontainer.json utilizzando la proprietà build.dockerfile:

```json
{    "name": "Custom .NET Environment",
    "build": {
        "dockerfile": "Dockerfile"
    }
}
```

#### Esempio completo di progetto con Dev Container e Dockerfile personalizzato

Un esempio completo di progetto con Dev Container e Dockerfile personalizzato è disponibile nel progetto di esempio [basic-container-demo](../../devcontainers-samples/basic-container-demo/README.md).

## Docker Compose nei Dev Containers

Quando un progetto ha **più servizi** (ad esempio una Minimal API + un database MariaDB + una cache Redis, etc.), Docker Compose è spesso l'approccio più comodo nei Dev Containers. In breve:

- `docker-compose.yml` descrive lo stack (servizi, reti, volumi).
- `devcontainer.json` istruisce VS Code su *quale servizio* collegarsi (es. `app`) e dove montare il workspace.
- La comunicazione **container → container** avviene tramite rete Docker e DNS interno (nome del servizio/container), quindi **non richiede** `forwardPorts`.
- `forwardPorts` serve per l'accesso **host → container** (browser, Postman, ecc.).

Di seguito due casi tipici per una Minimal API che usa **MariaDB in un container separato**.

### Caso 1: Dev Container solo per l'app (DB già esistente su rete Docker esterna)

Scenario: si vuole il Dev Container solo per la Minimal API e ci si aggancia ad una **rete Docker esterna** già esistente (nell'esempio `my-net`), sulla quale è già presente un database MariaDB chiamato `mariadb-server-1`.

Vantaggi:

- Setup rapido se il DB è già avviato altrove.
- Si può riutilizzare lo stesso DB tra più progetti.

Svantaggi:

- Il DB non è versionato nello stesso compose/progetto.
- Si deve assicurare che rete e container DB esistano prima di aprire il Dev Container.

Esempio di `.devcontainer/docker-compose.yml`:

```yaml
name: my-minimal-api-external-db-network
services:
  app:
    build:
      context: ..
      dockerfile: .devcontainer/Dockerfile
    volumes:
      - ../..:/workspaces:cached
    command: sleep infinity
    env_file:
      - ../.env # Carica le variabili d'ambiente dal file .env
    networks:
      - my-net # Collega alla rete esistente
      # Non serve forwardPorts qui perché la rete bridge gestisce la comunicazione interna

networks:
  my-net:
    external: true # Dice a Docker di usare la rete esistente invece di crearne una nuova
```

Esempio di `Dockerfile` (simile a quello visto prima, con strumenti utili):

```Dockerfile
FROM mcr.microsoft.com/devcontainers/dotnet:10.0-noble

USER root

# Installa strumenti utili per lo sviluppo
RUN apt-get update && apt-get install -y \
    git \
    curl \
    vim \
    nano \
    htop \
    tree \
    jq \
    ca-certificates \
    gnupg \
    iproute2 \
    iputils-ping \
    dnsutils \
    netcat-openbsd \
    openssl \
    procps \
    lsof \
    mariadb-client \
    redis-tools \
    httpie \
    yq \
    && rm -rf /var/lib/apt/lists/*

# Se il progetto ha come target net9.0 anche con SDK .NET 10 serve il runtime .NET 9 per eseguire app/tool net9.
# Installiamo quindi i runtime 9 affiancati a quelli 10 nel DOTNET_ROOT del container.
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --runtime dotnet --channel 9.0 --no-path --skip-non-versioned-files \
    && /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --runtime aspnetcore --channel 9.0 --no-path --skip-non-versioned-files \
    && rm /tmp/dotnet-install.sh

# Configura Git (opzionale)
RUN git config --system init.defaultBranch main
RUN git config --system --add safe.directory "/workspaces/*"

# Installa global tools .NET per l'utente con cui lavorerai nel Dev Container
USER vscode
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

```

Esempio di `.devcontainer/devcontainer.json` (con `remoteUser` e `workspaceFolder`):

```json
{
  "name": "ASP.NET Core - docker compose with existing db and network",
  "dockerComposeFile": "docker-compose.yml",
  "service": "app",
  "workspaceFolder": "/workspaces/${localWorkspaceFolderBasename}",
  //alternativa se non usiamo il docker-compose.yml
  //   "build": {
  //     "dockerfile": "Dockerfile",
  //     "context": ".."
  //   },
  // Features pre-costruite (database, cli tools, etc.)
  "features": {
    "ghcr.io/devcontainers/features/github-cli:1": {},
    "ghcr.io/devcontainers/features/docker-in-docker:2": {},
    "ghcr.io/devcontainers/features/node:1": {
      "version": "lts"
    },
    "ghcr.io/stu-bell/devcontainer-features/open-code:0": {},
    "ghcr.io/stu-bell/devcontainer-features/claude-code:0": {}
  },
  // Porte da forwardare (API standard + Hot Reload)
  "forwardPorts": [
    5000,
    5001,
    8080,
    8081
  ],
  "portsAttributes": {
    "5001": {
      "protocol": "https",
      "label": "API HTTPS"
    },
    "5000": {
      "protocol": "http",
      "label": "API HTTP"
    }
  },
  // Estensioni VS Code essenziali
  "customizations": {
    "vscode": {
      "extensions": [
        // .NET / C#
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "formulahendry.dotnet-test-explorer",
        "josefpihrt-vscode.roslynator", // Analizzatori codice C#
        // Docker
        "ms-azuretools.vscode-docker",
        // GitHub
        "GitHub.vscode-pull-request-github",
        "GitHub.copilot-chat",
        // HTML / CSS
        "formulahendry.auto-close-tag",
        "formulahendry.auto-rename-tag",
        "pranaygp.vscode-css-peek",
        // JavaScript / TypeScript
        "dbaeumer.vscode-eslint",
        "esbenp.prettier-vscode",
        "christian-kohler.path-intellisense",
        "usernamehw.errorlens",
        // Utilità varie
        "redhat.vscode-yaml",
        "pkief.material-icon-theme",
        "aaron-bond.better-comments",
        "humao.rest-client",
        // AI Assistants
        "sst-dev.opencode",
        "Anthropic.claude-code",
        "RooVeterinaryInc.roo-cline",
        // Database
        "cweijan.vscode-mysql-client2"
      ],
      "settings": {
        "terminal.integrated.defaultProfile.linux": "bash",
        "editor.formatOnSave": true,
        "omnisharp.enableRoslynAnalyzers": true,
        "editor.inlineSuggest.enabled": true,
        "debug.javascript.autoAttachFilter": "disabled",
        "http.proxySupport": "off",
        "files.watcherExclude": {
          "**/node_modules/**": true,
          "**/.git/objects/**": true
        },
        "files.useExperimentalFileWatcher": true // Usa un watcher alternativo
      }
    }
  },
  // Mounts della configurazione
  "mounts": [
    // Mount della configurazione di Claude Code
    "source=${localWorkspaceFolder}/.claude-config,target=/mnt/claude-config,type=bind,consistency=cached",
    // Mount della configurazione di OpenCode
    "source=${localWorkspaceFolder}/.opencode-config,target=/mnt/opencode-config,type=bind,consistency=cached"
  ],
  // Post-create commands: 
  "postCreateCommand": "bash -c 'if ls *.sln >/dev/null 2>&1; then dotnet restore && dotnet dev-certs https --trust; else echo \"Nessun progetto trovato. Salto restore...\"; fi || true && node /workspaces/${localWorkspaceFolderBasename}/.devcontainer/init-claude.cjs && node /workspaces/${localWorkspaceFolderBasename}/.devcontainer/init-opencode.cjs'",
  // Utente non-root per sicurezza
  "remoteUser": "vscode",
  // Variabili d'ambiente
  "containerEnv": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS": "https://+:5001;http://+:5000"
  }
}

```

:memo: Nota: in questo caso l'host MariaDB nella connection string è `mariadb-server-1` perché è il nome (DNS) visibile sulla rete Docker esterna. Se l'applicazione è in un container collegato alla stessa rete Docker del DB, può risolvere `mariadb-server-1` direttamente senza bisogno di IP o configurazioni aggiuntive. Si noti la differenza con `localhost` o `127.0.0.1`, quando l'applicazione client (ASP.NET Core ad esempio) è in esecuzione direttamente sull'host: in quel caso la stringa di connessione per MariaDB sarebbe del tipo `Server=localhost;Port=3306;Database=myappdb;User=devuser;Password=devpass;`.

#### Esempio completo di progetto con Dev Container con rete esterna e DB separato

Un esempio completo di progetto con Dev Container con rete esterna e DB separato è disponibile nel progetto di esempio [docker-compose-with-existing-db-and-network](../../devcontainers-samples/docker-compose-with-existing-db-and-network/README.md).

### Caso 2: Stack completo in Compose (app + MariaDB nello stesso docker-compose.yml)

Scenario: nello stesso docker compose si definiscono sia l'app (ASP.NET Core) sia il database MariaDB. È l'approccio più replicabile perché lo stack è definito "as code".

Esempio di `.devcontainer/docker-compose.yml` (Minimal API + MariaDB):

```yaml
name: my-minimal-api-full-example
services:
  app:
    build:
      context: ..
      dockerfile: .devcontainer/Dockerfile
    volumes:
      # Bind mount dell'intera workspace dentro /workspaces.
      # Il suffisso ':cached' è un hint di performance (soprattutto su Docker Desktop).
      - ../..:/workspaces:cached
    # Devcontainer pattern: tieni il container vivo e lancia manualmente l'app
    # (es. `dotnet run --project src/MyApi/MyApi.csproj`).
    command: sleep infinity
    env_file:
      - ../.env # Carica le variabili d'ambiente dal file .env
    depends_on:
      mariadb:
        condition: service_healthy
    networks:
      - my-net # Rete creata dal compose (non esterna)

  mariadb:
    # Best practice: pin di una versione specifica per evitare cambiamenti non controllati.
    # (Puoi aggiornare la tag quando vuoi fare upgrade espliciti.)
    image: mariadb:11.4
    restart: unless-stopped
    env_file:
      - ../.env
    # Didattica: esponiamo MariaDB sull'host per facilitare test con tool esterni (DBeaver/Workbench).
    # Best practice in ambienti reali: evita di esporre il DB, oppure limita l'accesso (firewall/VPN/SG).
    # Se sull'host hai gia' un DB su 3306, cambia MARIADB_HOST_PORT (es. 3307).
    #
    # Modalita' "secure" (consigliata per ambienti reali): nessuna porta pubblicata sull'host.
    # Per attivarla, commenta o rimuovi la sezione `ports` qui sotto.
    ports:
      - "${MARIADB_HOST_PORT:-3306}:3306"
    environment:
      # Inizializzazione DB (prima esecuzione volume)
      MARIADB_ROOT_PASSWORD: ${MARIADB_ROOT_PASSWORD:-root}
      MARIADB_DATABASE: ${MARIADB_DATABASE:-pizza_store}
      MARIADB_USER: ${MARIADB_USER:-pizza_user}
      MARIADB_PASSWORD: ${MARIADB_PASSWORD:-pizza_password}
    volumes:
      # Volume nominato per persistenza dei dati di MariaDB.
      - mariadb-data:/var/lib/mysql
      # Script di provisioning (schema/seed/grants) eseguiti SOLO al primo bootstrap del volume.
      - ./db-init:/docker-entrypoint-initdb.d:ro
    networks:
      - my-net
    healthcheck:
      # MariaDB image include uno script di healthcheck dedicato.
      # --connect: verifica che si riesca a connettere
      # --innodb_initialized: verifica che InnoDB sia inizializzato
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 20s
      timeout: 10s
      retries: 5
      start_period: 60s

  # Tooling one-shot: esegue gli script di provisioning contro un DB gia' avviato.
  # Utile quando:
  # - hai cancellato solo il database (DROP DATABASE) ma non il volume
  # - vuoi riallineare schema/seed/grants senza ricreare l'intero volume
  #
  # Esecuzione (da host):
  #   docker compose -f .devcontainer/docker-compose.yml run --rm db-provision
  db-provision:
    image: mariadb:11.4
    profiles: ["tools"]
    depends_on:
      mariadb:
        condition: service_healthy
    env_file:
      - ../.env
    environment:
      MARIADB_PROVISION_HOST: mariadb
      MARIADB_PROVISION_PORT: 3306
    networks:
      - my-net
    volumes:
      - ./db-init:/db-init:ro
    entrypoint:
      - bash
      # bash flags:
      # -l = login shell (carica /etc/profile e profili utente, se presenti)
      # -c = esegue il comando/script passato come stringa (qui sotto nel blocco `|`)
      - -lc
      # YAML: questo elemento di lista e' una stringa multilinea (literal block).
      # In pratica passiamo un mini-script a `bash -lc` preservando i newline.
      - |
        # Bash strict mode (fail-fast):
        # -e  : esci se un comando fallisce
        # -u  : errore se usi variabili non definite
        # pipefail: fallisce anche se si rompe un comando in una pipeline
        set -euo pipefail
        echo "[db-provision] Running provisioning scripts against ${MARIADB_PROVISION_HOST}:${MARIADB_PROVISION_PORT}..."
        for f in /db-init/*; do
          case "$f" in
            *.sh)
              echo "[db-provision] bash $f";
              bash "$f";
              ;;
            *.sql)
              echo "[db-provision] mariadb < $f";
              mariadb -h"${MARIADB_PROVISION_HOST}" -P"${MARIADB_PROVISION_PORT}" -uroot -p"${MARIADB_ROOT_PASSWORD}" < "$f";
              ;;
            *)
              echo "[db-provision] Skipping $f";
              ;;
          esac
        done
        echo "[db-provision] Done."

networks:
  my-net:
    # Best practice (isolamento): NON fissare un nome globale.
    # Compose creerà una rete namespaced tipo: <project>_my-net
    # Vantaggi: niente collisioni con altri progetti, stack eseguibili in parallelo.
    #
    # Se invece vuoi una rete condivisa tra più compose/progetti, puoi fissare un nome globale:
    # name: my-net
    driver: bridge

volumes:
  mariadb-data:
    # Best practice (isolamento): lascia che Compose crei un volume namespaced tipo:
    # <project>_mariadb-data
    # Vantaggi: persistenza per progetto, niente riuso accidentale di dati.
    #
    # Se invece vuoi condividere/riusare volutamente lo stesso volume tra progetti:
    # name: mariadb-data
    driver: local

```

Esempio di `.devcontainer/devcontainer.json`:

```json
{
  "name": "ASP.NET Core - full example",
  "dockerComposeFile": "docker-compose.yml",
  "service": "app",
  "workspaceFolder": "/workspaces/${localWorkspaceFolderBasename}",
  //alternativa se non usiamo il docker-compose.yml
  //   "build": {
  //     "dockerfile": "Dockerfile",
  //     "context": ".."
  //   },
  // Features pre-costruite (database, cli tools, etc.)
  "features": {
    "ghcr.io/devcontainers/features/github-cli:1": {},
    "ghcr.io/devcontainers/features/docker-in-docker:2": {},
    "ghcr.io/devcontainers/features/node:1": {
      "version": "lts"
    },
    "ghcr.io/stu-bell/devcontainer-features/open-code:0": {},
    "ghcr.io/stu-bell/devcontainer-features/claude-code:0": {}
  },
  // Porte da forwardare (API standard + Hot Reload)
  "forwardPorts": [
    5000,
    5001,
    8080,
    8081
  ],
  "portsAttributes": {
    "5001": {
      "protocol": "https",
      "label": "API HTTPS"
    },
    "5000": {
      "protocol": "http",
      "label": "API HTTP"
    }
  },
  // Estensioni VS Code essenziali
  "customizations": {
    "vscode": {
      "extensions": [
        // .NET / C#
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "formulahendry.dotnet-test-explorer",
        "josefpihrt-vscode.roslynator", // Analizzatori codice C#
        // Docker
        "ms-azuretools.vscode-docker",
        // GitHub
        "GitHub.vscode-pull-request-github",
        "GitHub.copilot-chat",
        // HTML / CSS
        "formulahendry.auto-close-tag",
        "formulahendry.auto-rename-tag",
        "pranaygp.vscode-css-peek",
        // JavaScript / TypeScript
        "dbaeumer.vscode-eslint",
        "esbenp.prettier-vscode",
        "christian-kohler.path-intellisense",
        "usernamehw.errorlens",
        // Utilità varie
        "redhat.vscode-yaml",
        "pkief.material-icon-theme",
        "aaron-bond.better-comments",
        "humao.rest-client",
        // AI Assistants
        "sst-dev.opencode",
        "Anthropic.claude-code",
        "RooVeterinaryInc.roo-cline",
        // Database
        "cweijan.vscode-mysql-client2"
      ],
      "settings": {
        "terminal.integrated.defaultProfile.linux": "bash",
        "editor.formatOnSave": true,
        "omnisharp.enableRoslynAnalyzers": true,
        "editor.inlineSuggest.enabled": true,
        "debug.javascript.autoAttachFilter": "disabled",
        "http.proxySupport": "off",
        "files.watcherExclude": {
          "**/node_modules/**": true,
          "**/.git/objects/**": true
        },
        "files.useExperimentalFileWatcher": true // Usa un watcher alternativo
      }
    }
  },
  // Mounts della configurazione
  "mounts": [
    // Mount della configurazione di Claude Code
    "source=${localWorkspaceFolder}/.claude-config,target=/mnt/claude-config,type=bind,consistency=cached",
    // Mount della configurazione di OpenCode
    "source=${localWorkspaceFolder}/.opencode-config,target=/mnt/opencode-config,type=bind,consistency=cached"
  ],
    // Post-create commands: 
    "postCreateCommand": "bash -c 'if ls *.sln >/dev/null 2>&1; then dotnet restore && dotnet dev-certs https --trust; else echo \"Nessun progetto trovato. Salto restore...\"; fi || true && node /workspaces/${localWorkspaceFolderBasename}/.devcontainer/init-claude.cjs && node /workspaces/${localWorkspaceFolderBasename}/.devcontainer/init-opencode.cjs'",
  // Utente non-root per sicurezza
  "remoteUser": "vscode",
  // Variabili d'ambiente
  "containerEnv": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS": "https://+:5001;http://+:5000"
  }
}
```

Nota: in questo caso *non* serve conoscere IP o configurare DNS: `Server=mariadb` funziona perché Compose crea una rete e fornisce la risoluzione DNS del nome del servizio.

#### Esempio completo di progetto con Dev Container, Database e rete gestiti da Docker Compose

Un esempio completo di progetto con Dev Container con rete esterna e DB separato è disponibile nel progetto di esempio [docker-compose-with-existing-db-and-network](../../devcontainers-samples/docker-compose-full-example/README.md).

### Template Dev Container per Docker Compose (multi-container) - Starter project

Per facilitare l'adozione dei Dev Containers con Docker Compose, è disponibile un template di progetto starter che include una configurazione di base per un'app ASP.NET Core e un database MariaDB, già predisposto per essere eseguito con Docker Compose.
Il template è disponibile al seguente link: [Dev Container Docker Compose Starter](../../devcontainers-samples/docker-compose-full-starter/README.md).

## Prebuilds

Uno degli aspetti critici nell'adozione dei Dev Containers (soprattutto in classe o in team numerosi) è il tempo necessario per ottenere un ambiente pronto: build dell’immagine, installazione di tool e dipendenze, restore, ecc.

I **prebuilds** di GitHub Codespaces servono a fare questo lavoro **prima** (in background, lato GitHub) così quando uno studente/sviluppatore crea il Codespace lo trova già “caldo”.

In pratica: invece di costruire tutto al primo avvio del Codespace, GitHub prepara una base pre-costruita per quella repo/configurazione.

### Vantaggi dei prebuilds

L'utilizzo dei prebuilds offre numerosi vantaggi:

- Riduzione del “cold start” (creazione Codespace) da minuti a secondi

- Meno errori dovuti a download/registries temporaneamente lenti

- Onboarding più veloce: tutti partono con lo stesso ambiente

- Maggiore prevedibilità (specie con immagini/features versionate)

### Configurazione GitHub Codespaces

GitHub Codespaces supporta nativamente i prebuilds tramite **impostazioni del repository** (non è obbligatorio creare una GitHub Action).

Guida operativa (UI):

1. Aprire il repository su GitHub
2. Andare in **Settings**
3. Andare in **Codespaces**
4. Trovare la sezione **Prebuilds**
5. Creare una configurazione scegliendo:
   - branch (es. `main`)
   - regione
   - configurazione devcontainer da usare (se il repo ne ha più di una)
   - trigger (su push e/o schedulazione)

Da quel momento GitHub genera prebuild automaticamente. Le nuove Codespaces create su quel branch useranno la prebuild più recente.

#### Nota su secrets e file `.env`

I prebuilds devono essere **riproducibili** e **sicuri**:

- Non includere segreti nel repository.
- In Codespaces usare **Codespaces Secrets** per le variabili sensibili.
- Evitare configurazioni che falliscono se un file `.env` non esiste al momento della build/creazione.
  - Strategia tipica: usare default `${VAR:-valore}` in `docker-compose.yml` e poi sovrascrivere via Secrets.

#### Progetto docker-compose-full-starter-codespace-ready

Esempio pratico già “prebuild-friendly”:

- Progetto sample: [docker-compose-full-starter-codespace-ready](../../devcontainers-samples/docker-compose-full-starter-codespace-ready/README.md)
  - niente bind mount dipendenti dall’host
  - niente `.env` obbligatorio all’avvio (usa default `${VAR:-...}`)
  - Secrets consigliati per il DB

#### Opzionale: build/push immagine via GitHub Actions

Questa opzione serve se si vuole:

- riusare la **stessa immagine** del Dev Container fuori da Codespaces (es. sviluppo locale, CI, altri ambienti)
- ridurre i tempi di build grazie al **cache** su registry
- avere un artefatto “pubblicato” (immagine) versionabile e riutilizzabile

È un approccio **separato** dai prebuild nativi di Codespaces (che si configurano dalla UI):

- **Prebuild Codespaces** = GitHub prepara *un ambiente di Codespaces*.
- **Build/push immagine** = GitHub Actions prepara *un'immagine Docker* su un registry.

Si può usare l’action `devcontainers/ci` per costruire l’immagine del devcontainer e pubblicarla su **GHCR** (GitHub Container Registry).

##### Prerequisiti

1. Il repository deve avere GitHub Actions abilitato.
2. Bisogna avere permessi per pubblicare pacchetti su GHCR (in org potrebbe essere limitato agli admin).
3. Nel workflow impostare i permessi minimi necessari:
   - `contents: read` (per fare checkout)
   - `packages: write` (per push su GHCR)

Nota: normalmente basta `GITHUB_TOKEN` (non serve creare PAT) se `packages: write` è concesso.

##### Procedura operativa

1. Creare il file `.github/workflows/devcontainer-image.yml` nel repository.
2. Incollare questo workflow (adattare solo il nome immagine se necessario).
3. Fare commit e push su `main` (o eseguire manualmente da Actions via `workflow_dispatch`).
4. Verificare che l’immagine sia stata pubblicata:
   - repo GitHub → tab **Packages** (oppure profilo/org → **Packages**)
   - dovresti vedere un package container con nome `<repo>/devcontainer` (o quello che hai scelto)

##### Workflow esempio (build + push su GHCR)

```yaml
name: Prebuild Dev Container
on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read
  packages: write

jobs:
  prebuild:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      # Login a GHCR usando il token automatico del workflow
      - name: Login to GHCR
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build devcontainer image
        uses: devcontainers/ci@v0.3
        with:
          # Nome completo immagine su GHCR (owner/repo)
          imageName: ghcr.io/${{ github.repository }}/devcontainer

          # Cache su registry: velocizza build successive (soprattutto con Dockerfile + apt-get)
          cacheFrom: ghcr.io/${{ github.repository }}/devcontainer

          # Pubblica sempre su push e su run manuale
          push: always
```

##### Come usarla (opzionale)

Se si vuole che il Dev Container **usi direttamente** l’immagine pubblicata, si può:

- sostituire in `devcontainer.json` la sezione `build` con `image`.

Esempio:

```json
{
  "name": "My Dev Container (prebuilt image)",
  "image": "ghcr.io/<OWNER>/<REPO>/devcontainer:latest"
}
```

Note pratiche:

- Se l’immagine è **privata**, l’utente che apre il Dev Container deve essere autenticato a GHCR (in Codespaces di solito è già ok per immagini nello stesso org/repo, ma dipende dalle policy).
- Con l’immagine prebuildata, il build locale diventa quasi istantaneo, ma `postCreateCommand` / `postStartCommand` possono comunque richiedere tempo (restore, seed db, ecc.).

##### Troubleshooting rapido

- Se il job fallisce con errori di push su GHCR:
  - controllare `permissions: packages: write`
  - verificare che l’org consenta a GitHub Actions di pubblicare packages
  - verificare che il package non sia stato creato con visibilità/blocco che impedisce nuove versioni
- Se il proprio devcontainer non è in root (monorepo):
  - mantenere il workflow, ma **verificare nella doc dell’action** `devcontainers/ci` quali parametri usare per puntare a un `devcontainer.json` in sottocartella (alcune versioni supportano opzioni tipo `configFile`/`subFolder`).

#### Uso in classe: template, fork e lavoro a gruppi

Qui sotto ci sono tre modalità pratiche per far partire **ogni studente** (o un **gruppo di studenti**) da un proprio repository (anche privato) e creare un **Codespace personale**.

Prima di scegliere, occorre chiarire 2 cose:

- **Permessi Git**: con accesso “read” gli studenti possono creare un Codespace e modificare file, ma non possono fare push sul repo del docente.
- **Chi paga / quali crediti si consumano**: in genere il consumo Codespaces segue il **proprietario del repository** da cui si crea il Codespace.
  - repo nello **spazio personale dello studente** → tipicamente consuma i **crediti/benefit dello studente** (es. Student Pack)
  - repo nella propria **organizzazione** (es. GreppiDev) → tipicamente consuma il **budget/limiti dell’organizzazione**

Nota: i dettagli precisi dipendono dal piano e dalle policy (org settings). Per essere sicuri al 100%, controllare in GitHub: **Organization Settings → Billing / Codespaces** e **Organization Settings → Codespaces → Policies**.

##### Opzione 1: Template repository (consigliata)

Obiettivo: ogni studente crea un **suo repository** (copia iniziale) e quindi il suo Codespace è legato al **suo account**.

###### Cosa fai il docente una sola volta

1. Apri il proprio repository su GitHub.
1. Va in **Settings → General**.
1. Nella sezione **Template repository**, abilita *Template repository*.
1. (Repo privato) Assicursi che gli studenti possano “vederlo”:

   - Opzione A: invitarli come **outside collaborator** con permesso **Read**
   - Opzione B: aggiungerli a un **Team** dell’organizzazione con permesso **Read** sul repo

###### Cosa fa ogni studente (Template)

1. Apre il repo del docente.
1. Clicca **Use this template**.
1. Sceglie dove creare il repo:

   - nel proprio **account personale** (consigliato se si vuole che usino i crediti del loro account)
   - oppure in una organizzazione a cui hanno accesso

1. Sceglie nome e visibilità (di solito *Private*).
1. Una volta creato il repo, va su **Code → Codespaces → Create codespace on main**.

###### Note importanti

- Un template è una **copia iniziale**: non c’è “collegamento automatico” col repo docente. Se si aggiorna il template dopo, gli studenti dovranno aggiornare manualmente (es. copiando le modifiche o iniziando un nuovo repo dal template aggiornato).

##### Opzione 2: Fork (se consentito)

Obiettivo: ogni studente lavora su un **fork** del repo del docente e può sincronizzarlo facilmente con l’upstream.

###### Cosa fai il docente

1. Verifica che sia consentito fare fork del repository (soprattutto se è privato):

   - in una organizzazione potrebbe essere disabilitato per policy.

1. (Se serve) abilita l’opzione di forking nelle impostazioni dell’org/repo (dipende dalle policy).

###### Cosa fa ogni studente (Fork)

1. Apre il repo del docente.
1. Clicca **Fork** e seleziona il proprio account come destinazione.
1. Sul fork (repo dello studente) crea il Codespace: **Code → Codespaces → Create codespace**.
1. Per allinearsi agli aggiornamenti del docente:

   - usa **Sync fork** dalla UI GitHub (quando disponibile)
   - oppure configura `upstream` e fa `git fetch upstream` + merge/rebase.

###### Costi/crediti (Fork, in genere)

- Se il fork è nel loro account personale, il consumo Codespaces ricade tipicamente sul loro account.

##### Opzione 3: Lavoro a gruppi (collaborazione)

Obiettivo: un gruppo di 2–4 studenti collabora sullo stesso progetto, ma **senza** condividere lo stesso Codespace (ognuno avrà il suo Codespace, sullo stesso repo).

La modalità più gestibile in classe è: **un repository per gruppo**, creato a partire dal repo del docente (template o copia), con permessi di scrittura solo al gruppo.

###### Setup consigliato (repo per gruppo nell’organizzazione GreppiDev)

1. (Una tantum) Creare un **Team** per ciascun gruppo:

   - Organization → **Teams** → *New team*
   - aggiungere gli studenti del gruppo

1. Per ogni gruppo, creare un repository “di gruppo” partendo dal repo del docente:

   - se il repo del docente è template: aprire il template → **Use this template** → owner = *GreppiDev* → nome tipo, ad esempio `pizza-store-gruppo-1`
   - altrimenti: creare un nuovo repo vuoto e copiare il contenuto (import o push iniziale)

1. Impostare i permessi del repo di gruppo:

   - Repo → **Settings → Collaborators and teams**
   - aggiungere il Team del gruppo con permesso **Write** (o **Maintain** se vuoi che gestiscano impostazioni non sensibili)

1. (Consigliato) Proteggere `main` per evitare conflitti:

   - Repo → **Settings → Branches**
   - aggiungere una branch protection rule su `main`
   - richiedere Pull Request (anche 1 approvazione, opzionale) e impedire push diretti

1. Gli studenti creano ognuno il proprio Codespace dal repo di gruppo:

   - Repo di gruppo → **Code → Codespaces → Create codespace**

###### Come collaborano (workflow minimo)

1. Ogni studente lavora su un branch: `git checkout -b feature/nome-cognome`
1. Push sul repo di gruppo.
1. Aprono Pull Request verso `main`.
1. Merge dopo review (anche rapida tra compagni).

###### Costi/crediti (Gruppi, in genere)

- Se il repo “di gruppo” sta nell’organizzazione GreppiDev, il consumo Codespaces ricade tipicamente sul budget/limiti dell’organizzazione.
- Per tenere i costi sotto controllo: limita la macchina consentita e imposta idle timeout in **Organization Settings → Codespaces → Policies**.

## Best Practices

L'adozione efficace dei Dev Containers richiede l'applicazione di best practices consolidate. Di seguito vengono presentate le raccomandazioni fondamentali per ottenere il massimo da questa tecnologia.

### Organizzazione del progetto

1. **Versionamento**: Includere sempre la cartella .devcontainer nel repository
2. **Documentazione**: Aggiungere un README.md nella cartella .devcontainer che spieghi la configurazione
3. **Modularità**: Utilizzare Features per strumenti comuni, Dockerfile per personalizzazioni specifiche

### Sicurezza

- Non includere credenziali o secrets nel file devcontainer.json

- Utilizzare variabili d'ambiente per configurazioni sensibili

- Aggiornare regolarmente le immagini base per includere patch di sicurezza

- Specificare versioni esplicite delle immagini e delle Features

### Performance

- Minimizzare il numero di layer nel Dockerfile

- Utilizzare .dockerignore per escludere file non necessari

- Sfruttare il caching delle immagini Docker

- Considerare l'uso di prebuilds per ambienti complessi

### Esempio di struttura progetto

```text
my-project/
├── .devcontainer/
│   ├── devcontainer.json
│   ├── Dockerfile
│   ├── docker-compose.yml
│   └── README.md
├── .vscode/
│   ├── launch.json
│   └── tasks.json
├── src/
│   └── ...
├── tests/
│   └── ...
└── MyProject.sln

```
