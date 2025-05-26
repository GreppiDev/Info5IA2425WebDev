
# Deployment di Applicazioni ASP.NET Core con Docker, Docker Compose e MariaDB

- [Deployment di Applicazioni ASP.NET Core con Docker, Docker Compose e MariaDB](#deployment-di-applicazioni-aspnet-core-con-docker-docker-compose-e-mariadb)
  - [1. Introduzione al Deployment e Prerequisiti](#1-introduzione-al-deployment-e-prerequisiti)
    - [1.1. Panoramica del deployment di applicazioni web moderne](#11-panoramica-del-deployment-di-applicazioni-web-moderne)
    - [1.2. Setup dell'ambiente di sviluppo](#12-setup-dellambiente-di-sviluppo)
    - [1.3. Creazione e struttura di un progetto ASP.NET Core Minimal API di riferimento (con `wwwroot`)](#13-creazione-e-struttura-di-un-progetto-aspnet-core-minimal-api-di-riferimento-con-wwwroot)
  - [2. Deployment Tradizionale: Build di Release](#2-deployment-tradizionale-build-di-release)
    - [2.1. Il processo di pubblicazione con `dotnet publish`](#21-il-processo-di-pubblicazione-con-dotnet-publish)
    - [2.2. Esecuzione dell'applicazione pubblicata (Kestrel)](#22-esecuzione-dellapplicazione-pubblicata-kestrel)
    - [2.3. Analisi dell'output di pubblicazione](#23-analisi-delloutput-di-pubblicazione)
    - [2.4. Richiamo ai meccanismi di configurazione di una applicazione ASP.NET Core (già analizzati in precedenza)](#24-richiamo-ai-meccanismi-di-configurazione-di-una-applicazione-aspnet-core-già-analizzati-in-precedenza)
    - [2.5. Configurazione di una applicazione ASP.NET in fase di release - `ASPNETCORE_URLS` vs `--urls` vs  `ASPNETCORE_HTTP_PORTS` vs `ASPNETCORE_HTTPS_PORTS`](#25-configurazione-di-una-applicazione-aspnet-in-fase-di-release---aspnetcore_urls-vs---urls-vs--aspnetcore_http_ports-vs-aspnetcore_https_ports)
  - [3. Containerizzazione con Docker: Le Basi](#3-containerizzazione-con-docker-le-basi)
    - [3.1. Introduzione a Docker: Immagini vs Container (richiami)](#31-introduzione-a-docker-immagini-vs-container-richiami)
    - [3.2. Scrittura di un `Dockerfile` per ASP.NET Core](#32-scrittura-di-un-dockerfile-per-aspnet-core)
      - [3.2.1. **Best Practice**: Multi-Stage Builds per ottimizzazione e sicurezza](#321-best-practice-multi-stage-builds-per-ottimizzazione-e-sicurezza)
      - [3.2.2. Comandi essenziali di un Dockerfile](#322-comandi-essenziali-di-un-dockerfile)
        - [`FROM`](#from)
        - [`WORKDIR`](#workdir)
        - [`COPY`](#copy)
        - [`RUN`](#run)
        - [`ENV`](#env)
        - [`EXPOSE`](#expose)
        - [`ENTRYPOINT`](#entrypoint)
        - [`CMD` e `ENTRYPOINT` in dettaglio](#cmd-e-entrypoint-in-dettaglio)
        - [`USER` in dettaglio](#user-in-dettaglio)
        - [`ARG` in dettaglio](#arg-in-dettaglio)
        - [`ENV` in dettaglio](#env-in-dettaglio)
    - [3.3. L'importanza del file `.dockerignore`](#33-limportanza-del-file-dockerignore)
    - [3.4. Costruzione di un'immagine: `docker build` e primo test di avvio](#34-costruzione-di-unimmagine-docker-build-e-primo-test-di-avvio)
    - [3.5. Pubblicazione di un'immagine su un registro](#35-pubblicazione-di-unimmagine-su-un-registro)
      - [3.5.1 Pubblicazione di un'immagine su Docker Hub](#351-pubblicazione-di-unimmagine-su-docker-hub)
      - [3.5.2 Pubblicazione di un'immagine su Azure Container Registry](#352-pubblicazione-di-unimmagine-su-azure-container-registry)
    - [3.6. Cenni sulla generazione automatica di `Dockerfile` (Visual Studio e altri strumenti)](#36-cenni-sulla-generazione-automatica-di-dockerfile-visual-studio-e-altri-strumenti)
    - [3.7. Confronto tra il Dockerfile generato da Containers (VS Code Plugin) e il Dockerfile semplice a due stadi scritto manualmente](#37-confronto-tra-il-dockerfile-generato-da-containers-vs-code-plugin-e-il-dockerfile-semplice-a-due-stadi-scritto-manualmente)
      - [Dockerfile completo (e complesso) - Analisi comando per comando](#dockerfile-completo-e-complesso---analisi-comando-per-comando)
      - [Confronto Dockerfile semplice (2 stadi) vs Dockerfile complesso (4 stadi)](#confronto-dockerfile-semplice-2-stadi-vs-dockerfile-complesso-4-stadi)
  - [4. Creazione Rapida di Immagini con .NET SDK (\>= .NET 7)](#4-creazione-rapida-di-immagini-con-net-sdk--net-7)
    - [4.1. Il comando `dotnet publish /t:PublishContainer`](#41-il-comando-dotnet-publish-tpublishcontainer)
      - [4.1.1 Il comando `dotnet publish /t:PublishContainer` con parametri da command line](#411-il-comando-dotnet-publish-tpublishcontainer-con-parametri-da-command-line)
      - [4.1.2 Immagini `Chiseled` (ottimizzate) del runtime per .NET](#412-immagini-chiseled-ottimizzate-del-runtime-per-net)
      - [4.1.3 Configurazione del `dotnet publish` tramite file `.csproj`](#413-configurazione-del-dotnet-publish-tramite-file-csproj)
        - [4.1.3.1 Pubblicazione dell'immagine generata mediante `dotnet publish` - configurazione nel `.csproj`](#4131-pubblicazione-dellimmagine-generata-mediante-dotnet-publish---configurazione-nel-csproj)
      - [4.1.4 Pubblicazione dell'immagine generata mediante `dotnet publish` con parametri nella command line](#414-pubblicazione-dellimmagine-generata-mediante-dotnet-publish-con-parametri-nella-command-line)
    - [4.2. Vantaggi, limiti e scenari d'uso](#42-vantaggi-limiti-e-scenari-duso)
    - [4.3. Confronto con l'approccio basato su `Dockerfile` tradizionale](#43-confronto-con-lapproccio-basato-su-dockerfile-tradizionale)
  - [5. Gestione dei Container Docker](#5-gestione-dei-container-docker)
    - [5.1. Avvio di un container: `docker run`](#51-avvio-di-un-container-docker-run)
    - [5.2. Port Mapping: Esporre l'applicazione (`-p`)](#52-port-mapping-esporre-lapplicazione--p)
    - [5.3. Networking in Docker](#53-networking-in-docker)
      - [5.3.1. La rete bridge di default (`bridge`)](#531-la-rete-bridge-di-default-bridge)
      - [5.3.2. Creazione e utilizzo di reti personalizzate (`docker network create`)](#532-creazione-e-utilizzo-di-reti-personalizzate-docker-network-create)
    - [5.4. Esecuzione di un container Database (MariaDB)](#54-esecuzione-di-un-container-database-mariadb)
    - [5.5. Collegamento di Container: Connessione App \<-\> Database sulla stessa rete](#55-collegamento-di-container-connessione-app---database-sulla-stessa-rete)
  - [6. Gestione Avanzata della Configurazione e dei Segreti 🔒](#6-gestione-avanzata-della-configurazione-e-dei-segreti-)
    - [6.1. La sfida delle variabili d'ambiente multiple (vs. User Secrets in sviluppo)](#61-la-sfida-delle-variabili-dambiente-multiple-vs-user-secrets-in-sviluppo)
    - [6.2. Utilizzo di file `.env` con Docker Compose](#62-utilizzo-di-file-env-con-docker-compose)
      - [6.2.1. Sostituzione di variabili nel `docker-compose.yml` (`${NOME_VARIABILE}`)](#621-sostituzione-di-variabili-nel-docker-composeyml-nome_variabile)
      - [6.2.2. **Best Practice**: Esclusione dei file `.env` da Git (uso di `.gitignore` e file `.env.example`)](#622-best-practice-esclusione-dei-file-env-da-git-uso-di-gitignore-e-file-envexample)
    - [6.3. La direttiva `env_file` in Docker Compose per caricare variabili da file esterni](#63-la-direttiva-env_file-in-docker-compose-per-caricare-variabili-da-file-esterni)
    - [6.4. **Criticità**: Perché evitare segreti hardcoded nel `docker-compose.yml`](#64-criticità-perché-evitare-segreti-hardcoded-nel-docker-composeyml)
    - [6.5. Panoramica delle soluzioni per la produzione](#65-panoramica-delle-soluzioni-per-la-produzione)
      - [6.5.1. Docker Secrets (per Docker Swarm e Kubernetes)](#651-docker-secrets-per-docker-swarm-e-kubernetes)
      - [6.5.2. Servizi di gestione segreti dei Cloud Provider (es. Azure Key Vault, AWS Secrets Manager, Google Secret Manager)](#652-servizi-di-gestione-segreti-dei-cloud-provider-es-azure-key-vault-aws-secrets-manager-google-secret-manager)
    - [6.6. Configurazione dell'applicazione ASP.NET Core per leggere variabili d'ambiente e segreti da diverse fonti](#66-configurazione-dellapplicazione-aspnet-core-per-leggere-variabili-dambiente-e-segreti-da-diverse-fonti)
  - [7. Orchestrazione con Docker Compose](#7-orchestrazione-con-docker-compose)
    - [7.1. Introduzione a Docker Compose: Perché usarlo per applicazioni multi-container?](#71-introduzione-a-docker-compose-perché-usarlo-per-applicazioni-multi-container)
    - [7.2. Struttura di un file `docker-compose.yml`: `version`, `services`, `networks`, `volumes`](#72-struttura-di-un-file-docker-composeyml-version-services-networks-volumes)
    - [7.3. Definizione dei servizi: `build`, `image`, `ports`, `environment`, `env_file`, `depends_on`, `restart`](#73-definizione-dei-servizi-build-image-ports-environment-env_file-depends_on-restart)
    - [7.4. Persistenza dei dati con i Volumi Docker (`volumes`): named volumes vs bind mounts](#74-persistenza-dei-dati-con-i-volumi-docker-volumes-named-volumes-vs-bind-mounts)
    - [7.5. Configurazione della comunicazione tra servizi tramite nomi di servizio e reti definite](#75-configurazione-della-comunicazione-tra-servizi-tramite-nomi-di-servizio-e-reti-definite)
  - [8. Caso Pratico: Orchestrazione App ASP.NET Core + MariaDB con Docker Compose](#8-caso-pratico-orchestrazione-app-aspnet-core--mariadb-con-docker-compose)
    - [8.1. Creazione del file `docker-compose.yml` completo](#81-creazione-del-file-docker-composeyml-completo)
    - [8.2. Gestione dello stack multi-container: `docker-compose up`, `down`, `logs`, `ps`, `exec`, `build`](#82-gestione-dello-stack-multi-container-docker-compose-up-down-logs-ps-exec-build)
    - [8.3. Verifica del funzionamento dell'applicazione, della corretta connessione al database e della persistenza dei dati](#83-verifica-del-funzionamento-dellapplicazione-della-corretta-connessione-al-database-e-della-persistenza-dei-dati)
  - [9. Considerazioni Finali e Prossimi Passi](#9-considerazioni-finali-e-prossimi-passi)
    - [9.1. Strategie di gestione della configurazione per diversi ambienti (sviluppo, staging, produzione)](#91-strategie-di-gestione-della-configurazione-per-diversi-ambienti-sviluppo-staging-produzione)
    - [9.2. Cenni su HTTPS all'interno dei container Docker (reverse proxy, certificati)](#92-cenni-su-https-allinterno-dei-container-docker-reverse-proxy-certificati)
    - [9.3. Introduzione al debugging di applicazioni containerizzate](#93-introduzione-al-debugging-di-applicazioni-containerizzate)
    - [9.4. Panoramica delle opzioni di deployment in produzione](#94-panoramica-delle-opzioni-di-deployment-in-produzione)

## 1. Introduzione al Deployment e Prerequisiti

### 1.1. Panoramica del deployment di applicazioni web moderne

Il deployment di applicazioni web è il processo che rende un'applicazione software operativa e accessibile agli utenti finali. Se in passato poteva consistere nella semplice copia di file su un server, le applicazioni moderne richiedono approcci più sofisticati per garantire affidabilità, scalabilità e manutenibilità. Le tendenze attuali nel deployment includono:

- **Containerizzazione**: Tecnologie come Docker permettono di impacchettare un'applicazione e tutte le sue dipendenze (librerie, runtime, file di configurazione) in unità isolate e standardizzate chiamate container. Questo assicura che l'applicazione funzioni in modo consistente in qualsiasi ambiente (sviluppo, test, produzione).

- **Architetture a Microservizi**: Suddivisione di applicazioni monolitiche complesse in un insieme di servizi più piccoli, indipendenti e specializzati. Ogni microservizio può essere sviluppato, installato e scalato autonomamente.

- **Infrastructure as Code (IaC)**: Gestione e provisioning dell'infrastruttura IT (server, reti, database) tramite file di configurazione leggibili dalla macchina (es. Terraform, AWS CloudFormation), piuttosto che attraverso configurazioni manuali. Questo permette automazione, versionamento e replicabilità dell'infrastruttura.

- **CI/CD (Continuous Integration/Continuous Delivery o Deployment)**: Pratiche e strumenti che automatizzano le fasi di build, test e rilascio del software. La CI integra frequentemente il codice prodotto da più sviluppatori, mentre la CD automatizza il rilascio delle modifiche in ambienti di test o produzione.

- **Cloud Computing**: Utilizzo di piattaforme cloud (come Amazon Web Services, Microsoft Azure, Google Cloud Platform) che offrono una vasta gamma di servizi on-demand, dall'hosting di macchine virtuali e container a database gestiti, intelligenza artificiale e molto altro, con modelli di pagamento flessibili.

Questo modulo si focalizzerà sulla containerizzazione con Docker e sull'orchestrazione di applicazioni multi-container con Docker Compose, competenze fondamentali per lo sviluppatore moderno.

### 1.2. Setup dell'ambiente di sviluppo

Per seguire efficacemente questo modulo, è necessario disporre dei seguenti strumenti installati e configurati nel proprio ambiente di sviluppo:

- **.NET SDK**: Software Development Kit per .NET, che include il runtime, le librerie e gli strumenti da riga di comando (`dotnet CLI`) per sviluppare applicazioni ASP.NET Core. È consigliabile utilizzare una versione recente (idealmente dalla versione .NET 7 in poi, poiché alcune funzionalità trattate sono specifiche di queste versioni).

    - **Download**: Dal sito ufficiale Microsoft [dotnet.microsoft.com](https://dotnet.microsoft.com/download "null").

    - **Verifica installazione**: Aprire un terminale o prompt dei comandi ed eseguire:

        ```sh
        dotnet --version
        ```

        Questo comando dovrebbe restituire la versione dell'.NET SDK installata.

- **Docker Desktop (per Windows e macOS) o Docker Engine (per Linux)**: Piattaforma per lo sviluppo, la distribuzione e l'esecuzione di applicazioni all'interno di container. Docker Desktop offre un'interfaccia grafica e un ambiente integrato, mentre Docker Engine è il componente core per Linux.

    - **Download Docker Desktop**: Dal sito ufficiale Docker [www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop "null").

    - **Installazione Docker Engine (Linux)**: Seguire le istruzioni specifiche per la propria distribuzione Linux disponibili su [docs.docker.com](https://docs.docker.com/engine/install/ "null").

    - **Verifica installazione**:

        ```sh
        docker --version
        ```

        Questo comando dovrebbe restituire la versione di Docker installata.

- **Un editor di codice o IDE**: A scelta tra Visual Studio Code (consigliato per la sua leggerezza e le potenti estensioni per Docker e .NET), Visual Studio, JetBrains Rider, o qualsiasi altro editor che supporti lo sviluppo ASP.NET Core.

In questo tutorial si assume di utilizzare Docker Desktop per Windows con l'integrazione della WSL.

### 1.3. Creazione e struttura di un progetto ASP.NET Core Minimal API di riferimento (con `wwwroot`)

Si procederà ora alla creazione di un semplice progetto ASP.NET Core utilizzando il template "Minimal API". Questo tipo di progetto è ideale per API leggere e veloci, e verrà esteso per servire anche file statici da una cartella `wwwroot`.

1. Creazione del progetto:

    Aprire un terminale, navigare nella directory desiderata per il progetto e è seguire i seguenti comandi:

    ```sh
    dotnet new webapi -o MyWebApiApp --no-https
    cd MyWebApiApp
    code .
    ```

    - `dotnet new web`: Crea un nuovo progetto ASP.NET Core basato sul template "Empty" o "Web" (che nelle versioni recenti di .NET è una Minimal API).

    - `-o MyWebApiApp`: Specifica il nome della directory e del progetto come `MyWebApiApp`.

    - `--no-https`: Crea il progetto configurato per HTTP anziché HTTPS. Questo semplifica la configurazione iniziale per Docker; la gestione di HTTPS verrà discussa in seguito.

    - `cd MyWebApiApp`: Entra nella directory del progetto appena creato.
    - Il template webapi crea un progetto di Minimal API ASP.NET esattamente come si farebbe da VS Code, selezionando il template per le API.
    - `code .` apre VS Code sulla cartella del progetto
  
    - :memo: Anche da VS Code è possibile, in fase di creazione del progetto, specificare che non si vuole configurare l'applicazione per l'utilizzo di https, basta cliccare sull'icona a forma di ingranaggio quando si sta per creare il progetto e selezionare l'opzione per https e impostare il valore a false. Tuttavia, anche se si creasse un progetto con il supporto sia a http che a https (che è la configurazione di default di VS Code e del comando dotnet new webapi), non cambierebbe molto per quanto mostrato di seguito.

    - :memo::fire:La scelta di considerare un progetto che non utilizza nativamente https per un container Docker è dovuto a due ragioni:
       1. complessità nel gestire https all'interno di un container Docker (si può fare, ma è un'inutile complicazione). In questo caso il problema principale è costituito dal non trust del browser nella macchina host del certificato https del server all'interno del container.
       2. Nelle applicazioni backend reali, è piuttosto comune non gestire direttamente la terminazione dell'HTTPS all'interno del backend applicativo (nel caso di ASP.NET Core, ciò significherebbe terminare l'HTTPS direttamente nel server Kestrel, incluso nell'eseguibile dell'applicazione). Al contrario, si preferisce spesso delegare questa responsabilità a un componente dedicato, come un reverse proxy e/o un load balancer, posto a monte del server backend.

       ```mermaid
      graph TB
        %% Client Layer
        C1[📱 Mobile Client]
        C2[💻 Web Browser]
        C3[🖥️ Desktop App]
        C4[⚙️ API Client]
        
        %% Load Balancer/Reverse Proxy
        LB[🔄 Reverse Proxy / Load Balancer<br/>NGINX / HAProxy / YARP / Traefik<br/>HTTPS Termination & SSL/TLS]
        
        %% ASP.NET Core Backend Instances
        subgraph "Backend Cluster"
            BE1[🌐 ASP.NET Core<br/>Instance 1<br/>:5001]
            BE2[🌐 ASP.NET Core<br/>Instance 2<br/>:5002]
            BE3[🌐 ASP.NET Core<br/>Instance 3<br/>:5003]
            BE4[🌐 ASP.NET Core<br/>Instance N<br/>:500N]
        end
        
        %% Database Cluster
        subgraph "Database Cluster"
            direction TB
            DB1[(🗄️ Primary DB<br/>PostgreSQL/MariaDB)]
            DB2[(🗄️ Secondary DB<br/>Read Replica)]
            DB3[(🗄️ Secondary DB<br/>Read Replica)]
            
            %% Database internal connections
            DB1 -.->|Replication| DB2
            DB1 -.->|Replication| DB3
        end
        
        %% Client to Load Balancer (HTTPS)
        C1 -->|HTTPS| LB
        C2 -->|HTTPS| LB
        C3 -->|HTTPS| LB
        C4 -->|HTTPS| LB
        
        %% Load Balancer to Backend (HTTP)
        LB -->|HTTP| BE1
        LB -->|HTTP| BE2
        LB -->|HTTP| BE3
        LB -->|HTTP| BE4
        
        %% Backend to Database
        BE1 -->|Write/Read| DB1
        BE1 -->|Read| DB2
        BE1 -->|Read| DB3
        
        BE2 -->|Write/Read| DB1
        BE2 -->|Read| DB2
        BE2 -->|Read| DB3
        
        BE3 -->|Write/Read| DB1
        BE3 -->|Read| DB2
        BE3 -->|Read| DB3
        
        BE4 -->|Write/Read| DB1
        BE4 -->|Read| DB2
        BE4 -->|Read| DB3
        
        %% Styling
        classDef client fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
        classDef proxy fill:#f3e5f5,stroke:#7b1fa2,stroke-width:3px
        classDef backend fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
        classDef database fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
        
        class C1,C2,C3,C4 client
        class LB proxy
        class BE1,BE2,BE3,BE4 backend
        class DB1,DB2,DB3 database
       ```

2. Aggiunta della cartella wwwroot e di un file statico:

    All'interno della directory del progetto MyWebApiApp, creare una nuova cartella chiamata wwwroot. Questa cartella è convenzionalmente usata da ASP.NET Core per servire file statici (HTML, CSS, JavaScript, immagini).

    Dentro wwwroot, creare un file index.html con il seguente contenuto:

    ```html
    <!DOCTYPE html>
    <html lang="it">
    <head>
        <meta charset="utf-8" />
        <title>La Mia App ASP.NET Core</title>
        <style>
            body { font-family: sans-serif; margin: 20px; background-color: #f4f4f4; color: #333; }
            h1 { color: #0056b3; }
            a { color: #007bff; text-decoration: none; }
            a:hover { text-decoration: underline; }
            .container { background-color: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        </style>
    </head>
    <body>
        <div class="container">
            <h1>Benvenuti nella Mia Applicazione ASP.NET Core!</h1>
            <p>Questa è una pagina HTML statica servita dalla cartella <code>wwwroot</code>.</p>
            <p>Testa l'endpoint API: <a href="/hello">/hello</a></p>
            <p>Testa la connessione al database (dopo configurazione): <a href="/dbtest">/dbtest</a></p>
        </div>
    </body>
    </html>
    ```

3. Modifica di Program.cs per servire file statici e definire un endpoint API:

    Aprire il file Program.cs (che si trova nella root del progetto MyWebApiApp) e modificarlo come segue per abilitare il serving di file statici e aggiungere un semplice endpoint API:

    ```cs
    // MyWebApiApp/Program.cs

    var builder = WebApplication.CreateBuilder(args);

    // Aggiungi servizi al container delle dipendenze (se necessario).
    // Esempio: builder.Services.AddControllers();

    var app = builder.Build();

    // Configura la pipeline di richieste HTTP.

    // Abilita il serving di file di default (es. index.html) dalla wwwroot
    app.UseDefaultFiles();
    // Abilita il serving di file statici (es. CSS, JS, immagini) dalla wwwroot
    app.UseStaticFiles();

    // Endpoint API di esempio
    app.MapGet("/hello", () => {
        return Results.Ok(new { Message = "Ciao dal backend ASP.NET Core Minimal API!", Timestamp = DateTime.UtcNow });
    });

    // Endpoint per il test della connessione al database (verrà implementato meglio più avanti)
    app.MapGet("/dbtest", (IConfiguration config) => {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            return Results.Text("Stringa di connessione 'DefaultConnection' non configurata.");
        }
        // Qui andrà la logica di test della connessione effettiva
        return Results.Text($"Tentativo di test con la stringa di connessione (da implementare): {connectionString}");
    });

    app.Run();
    ```

    - `app.UseDefaultFiles()`: Configura l'applicazione per servire un file di default (come `index.html` o `default.htm`) quando si richiede una directory.

    - `app.UseStaticFiles()`: Abilita il middleware per servire file statici dalla cartella `wwwroot`.

    - `app.MapGet("/hello", ...)`: Definisce un endpoint HTTP GET che risponde all'URL `/hello`.

4. Struttura del progetto risultante:

    La struttura della directory MyWebApiApp dovrebbe ora assomigliare a questa:

    ```text
    MyWebApiApp/
    ├── MyWebApiApp.csproj               # File di progetto .NET
    ├── Program.cs                    # Codice principale dell'applicazione
    ├── appsettings.json              # File di configurazione generale
    ├── appsettings.Development.json  # File di configurazione per l'ambiente di sviluppo
    ├── Properties/
    │   └── launchSettings.json       # Impostazioni di avvio per lo sviluppo locale (es. porte)
    └── wwwroot/
        └── index.html                # File HTML statico
    ```

5. Analisi del file di progetto

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Web">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
    </ItemGroup>

    </Project>
    ```

    Si noti che **non è necessario** aggiungere l'ItemGroup con `<Folder Include="wwwroot\" />` nel file di progetto per servire i file statici. Il template `Microsoft.NET.Sdk.Web` include automaticamente la cartella `wwwroot` come parte della convenzione standard di ASP.NET Core.

    Il template `Microsoft.NET.Sdk.Web` include automaticamente la cartella `wwwroot` come parte della convenzione standard di ASP.NET Core.

    **File statici inclusi automaticamente:**

    - I file nella cartella `wwwroot` vengono automaticamente copiati nella cartella di output durante il build (sia Debug che Release) grazie al comportamento predefinito del SDK `Microsoft.NET.Sdk.Web`.

        - **Quando si potrebbe aver bisogno di configurazione esplicita?**

            L'ItemGroup diventa necessario solo in questi casi specifici:

            1. **File con Build Action particolare**: Se si hanno file che richiedono un'azione di build specifica
            2. **File in altre cartelle**: Se si vuole servire file statici da cartelle diverse da `wwwroot`
            3. **Esclusioni specifiche**: Se si vuole escludere alcuni file dal deployment

    **Per il deployment/pubblicazione:**

    Quando si esegue `dotnet publish`, i file in `wwwroot` vengono automaticamente inclusi nella cartella di pubblicazione senza configurazioni aggiuntive.

6. Build ed esecuzione dell'applicazione localmente da command line:

    Dal terminale, all'interno della directory MyWebApiApp, è possibile eseguire una serie di comandi che normalmente sono disponibili in IDE come `VS Code` oppure `Visual Studio`. In particolare, si segnalano i seguenti comandi:
    - [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build): *Builds a project and all of its dependencies.*
    - [`dotnet run`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run): *Runs source code without any explicit compile or launch commands.*
    - [`dotnet restore`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore): *Restores the dependencies and tools of a project.*
    - [`dotnet watch`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-watch): *Restarts or hot reloads the specified application, or runs a specified dotnet command, when changes in source code are detected.*
  
    Per la prova di esecuzione di questo progetto eseguire il comando `dotnet run`:

    ```sh
    dotnet run
    ```

    Questo comando compila ed esegue l'applicazione. Il terminale mostrerà un output simile a:

    ```sh
    Building...
    info: Microsoft.Hosting.Lifetime[14]
          Now listening on: http://localhost:5XXX  (la porta può variare, es. 5000, 5001, 5239, etc.)
    info: Microsoft.Hosting.Lifetime[0]
          Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
          Hosting environment: Development
    info: Microsoft.Hosting.Lifetime[0]
          Content root path: /path/to/your/MyWebApiApp
    ```

    Aprire un browser web e navigare all'URL indicato (es. `http://localhost:5239`). Si dovrebbe visualizzare la pagina `index.html`. Navigando a `http://localhost:5239/hello` si dovrebbe vedere la risposta JSON dall'API.

## 2. Deployment Tradizionale: Build di Release

Durante lo sviluppo di un'applicazione ASP.NET Core, si lavora tipicamente in modalità **Debug**. Questa modalità è ottimizzata per facilitare l'individuazione e la correzione di errori, includendo simboli di debug dettagliati e minori ottimizzazioni del codice. Per il deployment in un ambiente di produzione (o staging), è invece cruciale creare una build di **Release**.

**Principali differenze tra build di Debug e Release**:

- **Ottimizzazione del Codice**:

    - **Debug**: Il codice è compilato con ottimizzazioni minime o disabilitate. Questo mantiene il codice compilato il più vicino possibile al codice sorgente, facilitando il debug step-by-step.

    - **Release**: Il compilatore applica ottimizzazioni significative (es. inlining di funzioni, rimozione di codice morto, riorganizzazione dei loop) per migliorare le prestazioni e ridurre le dimensioni dell'eseguibile. Queste ottimizzazioni possono rendere il debug diretto del codice di release più complesso.

- **Simboli di Debug (`.pdb` files)**:

    - **Debug**: Vengono generati file di simboli di debug completi (`.pdb`). Questi file contengono informazioni che mappano il codice compilato al codice sorgente originale, essenziali per il debugger.

    - **Release**: La generazione dei file `.pdb` può essere configurata. Tipicamente, si possono generare `.pdb` "portabili" o "embedded" che contengono informazioni sufficienti per analizzare stack trace da errori in produzione, ma meno dettagliati di quelli di debug, oppure possono essere omessi per ridurre ulteriormente le dimensioni del pacchetto di deployment.

- **Costanti di Compilazione Condizionale**:

    - In fase di **Debug** la costante di precompilazione `DEBUG` è definita. Questo permette di includere codice specifico per il debug tramite direttive `#if DEBUG ... #endif`.

    - In fase di **Release** la costante `DEBUG` non è definita (o è definita la costante `RELEASE`, a seconda della configurazione del progetto). Il codice all'interno dei blocchi `#if DEBUG` non viene compilato nella build di release.

- **Comportamento dell'Applicazione e Configurazione**:

    - **Debug**: Spesso si usano configurazioni specifiche per lo sviluppo, come `appsettings.Development.json`, che possono contenere stringhe di connessione a database locali, logging verboso, o funzionalità di diagnostica (es. Developer Exception Page).

    - **Release**: Si utilizzano configurazioni per la produzione (es. da `appsettings.Production.json`, variabili d'ambiente), con logging meno verboso, gestione degli errori più robusta per l'utente finale, e connessioni a servizi di produzione.

- **Sicurezza**:

    - **Debug**: Potrebbero essere abilitate funzionalità che espongono informazioni sensibili (es. pagine di eccezione dettagliate) utili per lo sviluppo ma rischiose in produzione.

    - **Release**: Queste funzionalità dovrebbero essere disabilitate, privilegiando la sicurezza e l'esperienza utente.

**Vantaggi architetturali di una build di Release**:

- **Prestazioni Migliori**: Grazie alle ottimizzazioni, l'applicazione risponde più velocemente e utilizza meno risorse (CPU, memoria).

- **Dimensioni Ridotte**: Il pacchetto di deployment è più piccolo, il che significa trasferimenti più rapidi e minor spazio disco occupato.

- **Maggiore Sicurezza**: La rimozione di codice e simboli di debug, e la disattivazione di funzionalità di sviluppo, riducono la superficie d'attacco dell'applicazione.

### 2.1. Il processo di pubblicazione con `dotnet publish`

**Il comando [`dotnet publish`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) è lo strumento standard della CLI .NET per preparare un'applicazione per il deployment**. Questo comando compila l'applicazione, risolve le dipendenze NuGet, e copia i file risultanti (assembly, dipendenze, file di contenuto, runtime se si crea un deployment self-contained) in una directory specificata, pronti per essere trasferiti su un server o impacchettati in un container.

Per creare una build di Release per il progetto `MyWebApiApp`:

1. Aprire un terminale nella directory root del progetto (`MyWebApiApp`).

2. Eseguire il comando:

    ```sh
    dotnet publish -c Release -o ./publish_output

    ```

    - `-c Release`: Specifica che si vuole utilizzare la configurazione di build "Release". Questo attiva le ottimizzazioni e le impostazioni appropriate per una build di produzione.

    - `-o ./publish_output`: Specifica la directory di output dove verranno collocati i file pubblicati. In questo caso, verrà creata una sottodirectory `publish_output` all'interno della directory corrente del progetto. Se la directory non esiste, verrà creata.

    È possibile specificare anche il runtime target (per i self-contained deployments) o il framework (per i framework-dependent deployments), ma per ora ci si atterrà ai default.

### 2.2. Esecuzione dell'applicazione pubblicata (Kestrel)

Una volta che l'applicazione è stata pubblicata, può essere eseguita utilizzando Kestrel, il server web cross-platform integrato in ASP.NET Core.

1. Navigare nella directory di output specificata durante la pubblicazione:

    ```sh
    cd publish_output
    ```

    (Se si era in `MyWebApiApp`, e l'output era `./publish_output`, ora ci si troverà in `MyWebApiApp/publish_output`).

2. Eseguire l'applicazione:

    Il file principale per avviare un'applicazione ASP.NET Core pubblicata (framework-dependent) è la DLL dell'applicazione stessa (es. MyWebApiApp.dll).

    ```sh
    dotnet MyWebApiApp.dll
    ```

    Questo comando utilizza il runtime .NET installato sulla macchina per eseguire l'applicazione. L'applicazione si avvierà e ascolterà sulle porte configurate (tipicamente `http://localhost:5000` o secondo quanto definito in `Properties/launchSettings.json` per il profilo di produzione, o tramite variabili d'ambiente).

    Se si fosse creato un deployment *self-contained* (che include il runtime .NET), si avrebbe un file eseguibile specifico per la piattaforma (es. `MyWebApiApp.exe` su Windows) che potrebbe essere avviato direttamente.

### 2.3. Analisi dell'output di pubblicazione

Esaminando il contenuto della directory `publish_output`, si troveranno i seguenti tipi di file:

- **Assembly dell'applicazione**:

    - `MyWebApiApp.dll`: La libreria a collegamento dinamico (DLL) principale che contiene il codice compilato dell'applicazione.

- **Dipendenze**:

    - DLL delle librerie NuGet da cui il progetto dipende (es. `Microsoft.AspNetCore.OpenApi.dll` se si usa Swagger/OpenAPI, `MySqlConnector.dll` se aggiunto per MariaDB, etc.).

- **File di configurazione del runtime**:

    - `MyWebApiApp.deps.json`: Un file JSON che elenca tutte le dipendenze dell'applicazione, incluse quelle del framework .NET.

    - `MyWebApiApp.runtimeconfig.json`: Un file JSON che specifica le opzioni di configurazione del runtime, come la versione del framework .NET richiesta.

- **File di contenuto**:

    - La cartella `wwwroot` con tutti i suoi file statici (es. `index.html`).

    - File di configurazione come `appsettings.json` e `appsettings.Production.json` (se la build di Release è configurata per usarlo).

- **Simboli di Debug (opzionali per Release)**:

    - `MyWebApiApp.pdb`: Se la generazione di PDB è abilitata per le build di Release, questo file conterrà i simboli di debug.

- **Eseguibile host (per Windows)**:

    - `MyWebApiApp.exe`: Su Windows, viene spesso generato un piccolo eseguibile che funge da host per la DLL principale.

Si noterà l'assenza dei file di codice sorgente (`.cs`), dei file di progetto (`.csproj`), e delle cartelle `obj` e `bin` (che contengono artefatti di build intermedi). La dimensione complessiva della cartella `publish_output` è generalmente ottimizzata per il deployment, contenendo solo il minimo indispensabile per eseguire l'applicazione.

### 2.4. Richiamo ai meccanismi di configurazione di una applicazione ASP.NET Core (già analizzati in precedenza)

ASP.NET Core carica i file di configurazione in questo ordine (dal meno prioritario al più prioritario):

1. **appsettings.json** - Base configuration
2. **appsettings.{Environment}.json** - Environment-specific
3. **User Secrets** - (solo in Development, [già trattati](../../../asp.net/docs/secrets/index.md))
4. **Variabili d'ambiente**
5. **Argomenti da linea di comando**

Le configurazioni successive **sovrascrivono** quelle precedenti.

- **Environment specifici:**

    L'environment viene determinato dalla variabile `ASPNETCORE_ENVIRONMENT`:

    - **Development** → carica `appsettings.Development.json`
    - **Production** → carica `appsettings.Production.json`
    - **Staging**[^1] → carica `appsettings.Staging.json`
    - **Custom** → carica `appsettings.Custom.json`

- **Esempio pratico:**
    - `appsettings.json` (base)

        ```json
        {
        "Logging": {
            "LogLevel": {
            "Default": "Information"
            }
        },
        "ConnectionString": "Server=localhost;Database=MyApp"
        }
        ```

    - `appsettings.Development.json`

        ```json
        {
        "Logging": {
            "LogLevel": {
            "Default": "Debug",
            "Microsoft": "Information"
            }
        },
        "ConnectionString": "Server=localhost;Database=MyApp_Dev"
        }
        ```

    - `appsettings.Production.json`

        ```json
        {
        "Logging": {
            "LogLevel": {
            "Default": "Warning"
            }
        },
        "ConnectionString": "Server=prod-server;Database=MyApp_Prod"
        }
        ```

- **Come viene determinato l'environment:**

    **In Development (Visual Studio/dotnet run):**

    - Default: `ASPNETCORE_ENVIRONMENT=Development`

    **In Production (dopo dotnet publish):**

    - Default: `ASPNETCORE_ENVIRONMENT=Production`

- **Override manuale:**

    ```sh
    # Linux/Mac
    ASPNETCORE_ENVIRONMENT=Staging dotnet MyApp.dll
    # Windows PowerShell
    $env:ASPNETCORE_ENVIRONMENT="Staging"; dotnet MyApp.dll
    ```

- **Risultato finale:**

    Se si è in **Development**, la configurazione finale sarà:

    - LogLevel: "Debug" (da Development.json)
    - ConnectionString: "Server=localhost;Database=MyApp_Dev" (da Development.json)

    Se si è in **Production**, la configurazione finale sarà:

    - LogLevel: "Warning" (da Production.json)
    - ConnectionString: "Server=prod-server;Database=MyApp_Prod" (da Production.json)

- Best practice

- **appsettings.json**: Configurazioni comuni e default
- **appsettings.Development.json**: Debug logging, database locali
- **appsettings.Production.json**: Logging minimo, connessioni production
- **Variabili d'ambiente**: Secrets, connection strings sensibili (non nei file)

### 2.5. Configurazione di una applicazione ASP.NET in fase di release - `ASPNETCORE_URLS` vs `--urls` vs  `ASPNETCORE_HTTP_PORTS` vs `ASPNETCORE_HTTPS_PORTS`

Ci sono diversi modi per cambiare la porta su cui è in ascolto un'applicazione ASP.NET Core pubblicata all'avvio:

1. Variabile d'ambiente `ASPNETCORE_URLS`

    ```sh
    # Esempio in Bash
    ASPNETCORE_URLS="http://localhost:8080"
    dotnet MyWebApiApp.dll
    ```

    ```ps1
    # Esempio in PowerShell
    $env:ASPNETCORE_URLS="http://localhost:8080"
    dotnet MyWebApiApp.dll
    ```

2. Argomento da linea di comando `--urls`

    ```sh
    dotnet MyWebApiApp.dll --urls "http://localhost:8080"
    ```

3. Variabile d'ambiente `ASPNETCORE_HTTP_PORTS` (più semplice)

    ```sh
    # Esempio in Bash
    ASPNETCORE_HTTP_PORTS=8080
    dotnet MyWebApiApp.dll
    ```

    ```ps1
    # Esempio in PowerShell
    $env:ASPNETCORE_HTTP_PORTS=8080
    dotnet MyWebApiApp.dll
    ```

4. File `appsettings.json` (configurazione permanente)

    Nel file `appsettings.json` dell'applicazione applicazione pubblicata:

    ```json
    {"Kestrel":{"Endpoints":{"Http":{"Url":"http://localhost:8080"}}}}
    ```

5. Più porte contemporaneamente

    ```sh
    # Esempio in Bash
    ASPNETCORE_URLS="http://localhost:8080;https://localhost:8443"
    dotnet MyWebApiApp.dll
    ```

    ```ps1
    # Esempio in PowerShell
    $env:ASPNETCORE_URLS="http://localhost:8080;https://localhost:8443"
    dotnet MyWebApiApp.dll
    ```

6. Tutte le interfacce di rete

    ```sh
    # Esempio in Bash
    ASPNETCORE_URLS="http://0.0.0.0:8080"
    dotnet MyWebApiApp.dll
    ```

    ```ps1
    # Esempio in PowerShell
    ASPNETCORE_URLS="http://0.0.0.0:8080"
    dot

**Raccomandazione**: Per un cambio rapido si usano il metodo 2 o 3. Per una configurazione permanente è meglio usare il metodo 4 modificando `appsettings.json` o `appsettings.Production.json`. Il metodo con `ASPNETCORE_HTTP_PORTS` è il più semplice se si vuole cambiare solo la porta HTTP senza specificare l'URL completo.

La differenza principale tra queste due variabili è il **livello di controllo** che offrono:

- `ASPNETCORE_URLS`

    - **Controllo completo**: Specifica URL completi con protocollo, indirizzo IP e porta
    - **Più flessibile**: Permette di configurare HTTP, HTTPS, interfacce specifiche
    - **Sintassi**: URL completi separati da `;`

    ```sh
    # Esempi in Bash
    ASPNETCORE_URLS="http://localhost:8080"
    ASPNETCORE_URLS="https://localhost:8443"
    ASPNETCORE_URLS="http://0.0.0.0:8080"
    ASPNETCORE_URLS="http://localhost:8080;https://localhost:8443"
    ```

    ```ps1
    # Esempi in PowerShell
    $env:ASPNETCORE_URLS="http://localhost:8080"
    $env:ASPNETCORE_URLS="https://localhost:8443"
    $env:ASPNETCORE_URLS="http://0.0.0.0:8080"
    $env:ASPNETCORE_URLS="http://localhost:8080;https://localhost:8443"
    ```

- `ASPNETCORE_HTTP_PORTS`

  - **Solo porte HTTP**: Specifica solo le porte per il traffico HTTP
  - **Più semplice**: Non devi specificare protocollo o indirizzo
  - **Sintassi**: Numeri di porta separati da `;`
  - **Limitato**: Solo per HTTP, non HTTPS

  ```sh
  # Esempi Bash
  ASPNETCORE_HTTP_PORTS=8080
  ASPNETCORE_HTTP_PORTS="8080;8081"
  ```

- `ASPNETCORE_HTTPS_PORTS`
    Esiste anche questa variabile per le porte HTTPS:

    ```sh
    #Esempio Bash
    ASPNETCORE_HTTPS_PORTS=8443
    ```

- **Quando usare quale:**

    **Usare ASPNETCORE_URLS quando:**

    - Si vuole specificare HTTPS
    - Si vuole effettuare il bind su interfacce specifiche (es. `0.0.0.0` per tutte)
    - Si ha bisogno di controllo completo

    **Usare ASPNETCORE_HTTP_PORTS quando:**

    - Si vuole solo cambiare la porta HTTP rapidamente
    - Non servono configurazioni avanzate
    - Si vuole una sintassi più semplice

- **Precedenza:**

    Se si specificano entrambe, `ASPNETCORE_URLS` ha la precedenza e sovrascrive le altre impostazioni.

## 3. Containerizzazione con Docker: Le Basi

La containerizzazione è una tecnologia che rivoluziona il modo in cui le applicazioni vengono sviluppate, distribuite ed eseguite. Docker è la piattaforma leader in questo campo. [Le basi di Docker](../getting-started/index.md) sono già state trattate. Per comodità in questo capitolo vengono richiamati alcuni concetti fondamentali. La [documentazione Microsoft](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container) offre molti dettagli sulla procedura per creare un container a partire da un progetto ASP.NET.

### 3.1. Introduzione a Docker: Immagini vs Container (richiami)

- **Immagine Docker (Image):**

    **Un'immagine Docker è un template read-only**, leggero, autonomo ed eseguibile **che contiene tutto il necessario per eseguire un'applicazione**: il codice, un runtime (es. .NET runtime), librerie, variabili d'ambiente e file di configurazione. **Le immagini sono costruite a partire da un file di istruzioni chiamato Dockerfile**. **Le immagini sono composte da layer (livelli) sovrapposti. Ogni istruzione nel Dockerfile crea un nuovo layer. Questa struttura a layer permette il caching e l'efficienza nella distribuzione e nello storage. Le immagini possono essere basate su altre immagini (immagini base), formando una gerarchia**. Ad esempio, un'immagine per un'applicazione ASP.NET Core sarà tipicamente basata su un'immagine ufficiale Microsoft che contiene già il runtime ASP.NET Core.

- **Container Docker (Container):**

    **Un container è un'istanza eseguibile di un'immagine Docker**. Quando si "esegue" un'immagine, si crea un container. Si possono creare, avviare, fermare, spostare ed eliminare container. Ogni container è isolato dagli altri container e dal sistema host (il server su cui Docker è in esecuzione), ma può comunicare con l'esterno o con altri container attraverso reti configurate. **Dal punto di vista del sistema operativo host, un container è un processo (o un gruppo di processi) isolato**. Quando un container viene modificato (es. un'applicazione scrive un file al suo interno), Docker aggiunge un layer scrivibile sopra l'immagine read-only. Questo layer viene perso quando il container viene eliminato, a meno che non si utilizzino i volumi per la persistenza dei dati.

**Vantaggi architetturali della containerizzazione con Docker**:

- **Consistenza degli Ambienti ("Works on my machine" eliminato)**: Un'applicazione containerizzata si comporta allo stesso modo indipendentemente da dove viene eseguita (laptop dello sviluppatore, server di test, cloud di produzione), poiché l'ambiente è definito nell'immagine.

- **Isolamento**: I container eseguono processi in ambienti isolati. Questo previene conflitti tra dipendenze di diverse applicazioni e migliora la sicurezza.

- **Portabilità**: Le immagini Docker possono essere eseguite su qualsiasi sistema che supporti Docker (Linux, Windows, macOS, cloud).

- **Efficienza delle Risorse**: I container sono molto più leggeri delle macchine virtuali tradizionali (VM) perché condividono il kernel del sistema operativo host e non richiedono un OS completo per ogni istanza. Si avviano più rapidamente e consumano meno risorse.

- **Scalabilità e Deployment Rapido**: È facile creare e distruggere container, rendendo semplice scalare orizzontalmente un'applicazione (aggiungendo più istanze) o aggiornarla.

- **Gestione Semplificata delle Dipendenze**: Tutte le dipendenze sono impacchettate nell'immagine, eliminando la necessità di installarle e configurarle separatamente su ogni server.

- **Supporto ai Microservizi**: Docker è ideale per architetture a microservizi, dove ogni servizio può essere impacchettato e gestito come un container indipendente.

### 3.2. Scrittura di un `Dockerfile` per ASP.NET Core

Un `Dockerfile` è un file di testo che contiene una sequenza di istruzioni (comandi) che Docker utilizza per assemblare automaticamente un'immagine.

#### 3.2.1. **Best Practice**: Multi-Stage Builds per ottimizzazione e sicurezza

Per applicazioni compilate come quelle ASP.NET Core, l'utilizzo di *multi-stage builds* (build multi-stadio) nel `Dockerfile` è una best practice cruciale. Un multi-stage build permette di utilizzare più istruzioni `FROM` all'interno dello stesso `Dockerfile`. Ogni istruzione `FROM` definisce una nuova fase di build che può utilizzare un'immagine base diversa.

**Vantaggi dei Multi-Stage Builds**:

1. **Ottimizzazione della Dimensione dell'Immagine Finale**:

    - La prima fase (es. "build stage") può utilizzare un'immagine base più grande che contiene l'SDK .NET completo, necessario per compilare il codice, ripristinare le dipendenze NuGet e pubblicare l'applicazione.

    - Una fase successiva (es. "runtime stage" o "final stage") può utilizzare un'immagine base molto più piccola, come quella del solo runtime ASP.NET Core, che è ottimizzata per l'esecuzione e non contiene l'SDK.

    - Solo gli artefatti necessari dalla fase di build (i file pubblicati) vengono copiati nell'immagine finale. Questo riduce drasticamente le dimensioni dell'immagine di produzione. Immagini più piccole significano trasferimenti più rapidi, minor spazio di storage e una superficie d'attacco ridotta.

2. **Miglioramento della Sicurezza**:

    - **L'immagine finale non contiene l'SDK .NET, il codice sorgente, strumenti di build o dipendenze di sviluppo non necessarie**. Questo riduce la quantità di software potenzialmente vulnerabile presente nell'immagine di produzione.

3. **Build più Pulite e Organizzate**:

    - Le fasi separano logicamente le problematiche di build da quelle di runtime.

#### 3.2.2. Comandi essenziali di un Dockerfile

Si creerà ora un `Dockerfile` per l'applicazione `MyWebApiApp` utilizzando un approccio multi-stage semplificato a due stadi. Creare un file chiamato `Dockerfile` (senza estensione) nella directory root del progetto `MyWebApiApp`.

```dockerfile
# MyWebApiApp/Dockerfile

# Fase 1: Build (SDK Stage)
# Utilizza l'immagine ufficiale dell'SDK .NET 9.0 come base.
# Scegliere una versione specifica dell'SDK è una buona pratica per build riproducibili.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copia i file .csproj e ripristina le dipendenze come primo passo.
# Questo sfrutta il layer caching di Docker: se i file .csproj e le dipendenze
# non cambiano, Docker riutilizzerà i layer esistenti per questi passaggi,
# velocizzando le build successive quando si modificano solo i file .cs.
COPY *.csproj ./
RUN dotnet restore

# Copia il resto dei file del progetto nella directory di lavoro del container.
COPY . ./

# Pubblica l'applicazione in modalità Release.
# L'output verrà messo nella cartella 'out' all'interno della directory di lavoro (/app/out).
RUN dotnet publish -c Release -o out

# Fase 2: Runtime (Final Stage)
# Utilizza l'immagine ufficiale del runtime ASP.NET Core 9.0, che è più leggera dell'SDK.
# Assicurarsi che la versione del runtime corrisponda a quella usata per la build.
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copia gli artefatti pubblicati dalla fase di build ('build-env') nell'immagine finale.
COPY --from=build-env /app/out .

# Imposta la variabile d'ambiente ASPNETCORE_URLS per far ascoltare Kestrel
# su tutte le interfacce di rete sulla porta 8080 all'interno del container.
# A partire da .NET 8, le immagini base aspnet ascoltano su http://*:8080 di default,
# quindi questa riga è principalmente per chiarezza e compatibilità.
ENV ASPNETCORE_URLS=http://+:8080

# Esponi la porta 8080. Questa è una documentazione per l'utente dell'immagine
# e può essere usata da Docker per mappare automaticamente la porta se richiesto.
# Non pubblica effettivamente la porta sull'host; ciò avviene con 'docker run -p'.
EXPOSE 8080

# Definisci il punto di ingresso per eseguire l'applicazione quando il container si avvia.
# Questo comando esegue 'dotnet MyWebApiApp.dll'.
ENTRYPOINT ["dotnet", "MyWebApiApp.dll"]
```

**Spiegazione dei comandi Docker utilizzati e di altri comandi utili**:

##### `FROM`

- `FROM <image>:<tag> AS <stage_name>`: Specifica l'immagine base da cui partire. `mcr.microsoft.com/dotnet/sdk:8.0` è l'immagine ufficiale Microsoft che contiene l'.NET 8 SDK. `AS build-env` assegna un nome (`build-env`) a questa fase, che può essere referenziato successivamente (es. in `COPY --from=build-env`).

##### `WORKDIR`

- `WORKDIR /app`: Imposta la directory di lavoro predefinita per i comandi successivi (`COPY`, `RUN`, `ENTRYPOINT`) all'interno del container. Se la directory non esiste, viene creata.

##### `COPY`

- `COPY <src> <dest>`: Copia file e directory dal contesto di build di Docker (la directory sull'host dove si esegue `docker build`) nel filesystem del container.

    - `COPY *.csproj ./`: Copia tutti i file con estensione `.csproj` (e simili come `.vbproj`, `.fsproj`) nella directory di lavoro corrente (`/app`) del container.

    - `COPY . ./`: Copia tutti i file e le directory rimanenti dal contesto di build nella directory di lavoro del container.

##### `RUN`

- `RUN <command>`: Esegue un comando nella shell del container durante il processo di build dell'immagine. Viene creato un nuovo layer per ogni comando `RUN`.

    - `RUN dotnet restore`: Ripristina le dipendenze NuGet definite nel file `.csproj`.

    - `RUN dotnet publish -c Release -o out`: Compila l'applicazione in configurazione `Release` e pubblica l'output nella sottodirectory `out` (quindi in `/app/out`).

- `COPY --from=<stage_name> <src> <dest>`: Questo è specifico dei multi-stage builds. Copia file dalla fase di build precedente (`build-env` in questo caso) nell'attuale fase. `COPY --from=build-env /app/out .` copia il contenuto della cartella `/app/out` della fase `build-env` nella directory di lavoro `/app` della fase finale.

##### `ENV`

- `ENV <key>=<value>`: Imposta una variabile d'ambiente all'interno dell'immagine. Questa variabile sarà disponibile quando un container viene eseguito da questa immagine. `ASPNETCORE_URLS=http://+:8080` dice a Kestrel di ascoltare sulla porta 8080 su tutte le interfacce di rete disponibili nel container. Il `+` è un modo per specificare "qualsiasi indirizzo IP" in questo contesto.

##### `EXPOSE`

- `EXPOSE <port>`: Informa Docker che il container ascolterà sulla porta specificata al runtime. Non pubblica la porta automaticamente; serve come documentazione e può essere usata da strumenti automatici o per la mappatura con `docker run -P` (P maiuscola). La mappatura esplicita avviene con `docker run -p <host_port>:<container_port>`.

##### `ENTRYPOINT`

- `ENTRYPOINT ["executable", "param1", "param2"]`: Configura il comando principale che verrà eseguito quando un container viene avviato dall'immagine. In questo caso, `dotnet MyWebApiApp.dll` avvia l'applicazione ASP.NET Core. Questo è il formato "exec form", preferito rispetto allo "shell form" (`ENTRYPOINT dotnet MyWebApiApp.dll`).

**Altri comandi Docker utili:**

##### `CMD` e `ENTRYPOINT` in dettaglio

- **`CMD` e sua interazione con `ENTRYPOINT`**
   - `CMD ["eseguibile", "param1", "param2"]` (oppure `CMD ["param1", "param2"]` se utilizzata in congiunzione con `ENTRYPOINT`): definisce il comando predefinito da eseguire quando un container Docker viene avviato. Questo comando viene eseguito dopo l'avvio del container e, a meno che non venga sovrascritto, è il comando che viene effettivamente eseguito.

     Le istruzioni `ENTRYPOINT` e `CMD` all'interno di un Dockerfile sono entrambe utilizzate per specificare il comando da eseguire all'avvio di un container. Esse presentano, tuttavia, differenze significative riguardo all'interazione con gli argomenti forniti al comando `docker run` e al loro scopo primario.

     L'istruzione **`ENTRYPOINT`** ha la funzione di definire l'eseguibile principale del container. È concepita per comandi che costituiscono l'operazione fondamentale del container e che devono essere eseguiti ad ogni avvio. Gli argomenti forniti al comando `docker run` vengono accodati come parametri all'istruzione `ENTRYPOINT` (se questa è in forma *exec*).

     L'istruzione **`CMD`**, invece, serve a fornire i parametri predefiniti per l'istruzione `ENTRYPOINT` o, in assenza di un `ENTRYPOINT`, l'intero comando predefinito da eseguire. Qualora vengano specificati argomenti nel comando `docker run`:

     - In assenza di `ENTRYPOINT`, l'intero contenuto di `CMD` viene sostituito dagli argomenti di `docker run`.
     - In presenza di un `ENTRYPOINT` (nella sua forma *exec*), `CMD` definisce gli argomenti predefiniti per tale `ENTRYPOINT`. Questi argomenti predefiniti possono essere sovrascritti dagli argomenti forniti al comando `docker run`.

   - Modalità di Interazione tra `ENTRYPOINT` e `CMD`

     La comprensione delle modalità di interazione tra `ENTRYPOINT` e `CMD` è cruciale per un utilizzo efficace:

     1. **Utilizzo esclusivo di `CMD`**: Se il Dockerfile contiene unicamente l'istruzione `CMD`, questa definisce il comando e gli argomenti predefiniti. Qualsiasi comando o argomento specificato dopo il nome dell'immagine nel comando `docker run` sostituirà integralmente il `CMD` definito nel Dockerfile.

         - Dockerfile: `CMD ["/bin/bash"]`
         - Esecuzione `docker run <immagine>`: avvia `/bin/bash`.
         - Esecuzione `docker run <immagine> ls -l`: avvia `ls -l` (l'istruzione `CMD` originale viene ignorata).
     2. **Utilizzo esclusivo di `ENTRYPOINT` (forma *exec*)**: Se il Dockerfile contiene unicamente `ENTRYPOINT ["eseguibile", "param1"]` (noto come forma *exec*), questo sarà il comando eseguito. Gli argomenti aggiuntivi forniti tramite `docker run` verranno accodati a quelli specificati nell'`ENTRYPOINT`.

         - Dockerfile: `ENTRYPOINT ["/usr/sbin/nginx", "-g", "daemon off;"]`
         - Esecuzione `docker run <immagine>`: avvia `/usr/sbin/nginx -g daemon off;`.
         - Esecuzione `docker run <immagine> -c /etc/nginx/nginx.conf`: avvia `/usr/sbin/nginx -g daemon off; -c /etc/nginx/nginx.conf`.
     3. **Combinazione di `ENTRYPOINT` e `CMD` (entrambi in forma *exec*)**: Questa configurazione rappresenta l'approccio più comune e versatile. `ENTRYPOINT` stabilisce l'eseguibile principale, mentre `CMD` fornisce gli argomenti predefiniti per tale eseguibile.

         - Dockerfile:Dockerfile

             ```dockerfile
             ENTRYPOINT ["messaggio"]
             CMD ["Ciao", "Mondo"]
             ```

         - Esecuzione `docker run <immagine>`: avvia `messaggio Ciao Mondo`.
         - Esecuzione `docker run <immagine> Salve Universo`: avvia `messaggio Salve Universo` (gli argomenti di `CMD` vengono sovrascritti, ma l' `ENTRYPOINT` rimane invariato).

   - Formati di Sintassi

     Sia `ENTRYPOINT` che `CMD` supportano due formati di sintassi:

     - **Forma *exec*** (raccomandata): `ENTRYPOINT ["eseguibile", "param1", "param2"]` e `CMD ["eseguibile", "param1", "param2"]` (oppure `CMD ["param1", "param2"]` se utilizzata in congiunzione con `ENTRYPOINT`). Questa sintassi, strutturata come un array JSON, esegue il comando specificato direttamente, senza l'intermediazione di una shell. Ciò significa che il processo avviato sarà il PID 1 all'interno del container (a meno che non si utilizzi un init system).
     - **Forma *shell***: `ENTRYPOINT comando param1 param2` e `CMD comando param1 param2`. Questa sintassi esegue il comando tramite una shell (tipicamente `/bin/sh -c`). Ciò comporta l'interpretazione delle variabili d'ambiente da parte della shell e la possibilità di utilizzare costrutti specifici della shell (es. `&&`, `||`, pipe). Tuttavia, il comando effettivo sarà un figlio della shell.

     :memo::fire:**Nota importante sulla forma *shell***: Qualora si utilizzi la forma *shell* per `ENTRYPOINT`, qualsiasi istruzione `CMD` o argomento fornito a `docker run` verrà ignorato. Per tale ragione, la forma *exec* di `ENTRYPOINT` è generalmente da preferirsi, soprattutto quando si intende utilizzare `CMD` per fornire argomenti predefiniti.

   - Sovrascrittura delle Istruzioni

     Le modalità di sovrascrittura delle istruzioni sono le seguenti:

     - **`CMD`**: Può essere agevolmente sovrascritto specificando un comando e/o argomenti dopo il nome dell'immagine nel comando `docker run`.
     - **`ENTRYPOINT`**: Non viene sovrascritto direttamente dagli argomenti forniti a `docker run`. Per modificare l'`ENTRYPOINT` definito nell'immagine, è necessario utilizzare l'opzione `--entrypoint` nel comando `docker run`.
         - Esempio: `docker run --entrypoint /bin/sh <immagine> -c "echo ciao"`

   - Linee Guida Generali per la Scelta

       Si forniscono di seguito alcune linee guida generali per la scelta tra `ENTRYPOINT` e `CMD`:

     - Si consiglia di utilizzare **`ENTRYPOINT`** quando l'obiettivo è creare un'immagine Docker progettata per eseguire un comando specifico, agendo in modo simile a un eseguibile autonomo. L'immagine è quindi "finalizzata" a tale comando.

         - **Esempio**: Un'immagine che deve sempre avviare un server web specifico: `ENTRYPOINT ["/usr/sbin/apache2ctl", "-D", "FOREGROUND"]`

     - Si consiglia di utilizzare **`CMD`** per specificare i **parametri predefiniti** per un `ENTRYPOINT` (nella sua forma *exec*), oppure per fornire un **comando predefinito completo** se non è presente un `ENTRYPOINT`. Questi valori predefiniti sono concepiti per essere quelli che l'utente dell'immagine è più propenso a sovrascrivere.

         - **Esempio (con `ENTRYPOINT`)**:Dockerfile

             ```dockerfile
             FROM ubuntu
             ENTRYPOINT ["ping"]
             CMD ["localhost"]
             ```

             Questo container, per impostazione predefinita, eseguirà `ping localhost`. Tuttavia, l'utente può eseguire `docker run <immagine> google.com` per effettuare il ping verso `google.com`.
         - **Esempio (solo `CMD`)**:Dockerfile

             ```dockerfile
             FROM alpine
             CMD ["echo", "Benvenuto! Fornisci un comando per eseguirlo."]
             ```

             Eseguendo `docker run <immagine>` verrà visualizzato il messaggio. Eseguendo `docker run <immagine> ls /` verrà elencato il contenuto della directory radice.

   - **La combinazione di `ENTRYPOINT` (nella forma *exec*) e `CMD` rappresenta frequentemente la strategia ottimale** per la creazione di immagini Docker flessibili e dal comportamento prevedibile. `ENTRYPOINT` stabilisce la funzionalità primaria, mentre `CMD` offre valori predefiniti facilmente personalizzabili dall'utente.

   - :memo::fire:**È importante ricordare che, qualora più istruzioni `CMD` o `ENTRYPOINT` siano presenti in un Dockerfile, solo l'ultima di ciascun tipo avrà effetto sulla configurazione finale dell'immagine**.

##### `USER` in dettaglio

- **Istruzione `USER`**
  L'istruzione **`USER`** in un Dockerfile è utilizzata per impostare il nome utente (o UID) e opzionalmente il gruppo utente (o GID) da utilizzare come utente predefinito per tutte le istruzioni successive (`RUN`, `CMD`, `ENTRYPOINT`) all'interno del Dockerfile, nonché per l'esecuzione del processo principale quando un container viene avviato dall'immagine.

  - Scopo Principale dell'Istruzione `USER`

    Lo scopo primario dell'istruzione `USER` è quello di **migliorare la sicurezza** dei container Docker. **Per impostazione predefinita, i comandi all'interno di un container vengono eseguiti con i privilegi dell'utente `root` (UID 0)**. Eseguire processi come `root` all'interno di un container può presentare rischi di sicurezza, poiché un eventuale exploit nel container potrebbe concedere privilegi elevati sull'host Docker (specialmente se altre misure di sicurezza come i namespace degli utenti non sono configurati in modo stringente).

    **Utilizzando `USER`, si può specificare un utente non privilegiato per l'esecuzione delle applicazioni e dei comandi**. Questo aderisce al **principio del privilegio minimo** (Principle of Least Privilege - PoLP), secondo cui un processo dovrebbe avere solo i permessi strettamente necessari per svolgere le proprie funzioni.

    Sintassi

    L'istruzione `USER` accetta le seguenti forme:

    1. **Solo nome utente**:

        ```dockerfile
        USER <nome_utente>
        ```

        Esempio: `USER appuser`

    2. **Nome utente e gruppo**:

        ```dockerfile
        USER <nome_utente>:<nome_gruppo>
        ```

        Esempio: `USER appuser:appgroup`

    3. **Solo UID**:

        ```dockerfile
        USER <uid>
        ```

        Esempio: `USER 1000`

    4. **UID e GID**:

        ```dockerfile
        USER <uid>:<gid>
        ```

        Esempio: `USER 1000:1001`

    :memo::fire:**È importante che l'utente e il gruppo specificati devono esistere all'interno dell'immagine**. Se l'utente o il gruppo non esistono, la build dell'immagine potrebbe non fallire immediatamente, ma l'esecuzione di comandi successivi o l'avvio del container potrebbero generare errori o comportamenti imprevisti.

  - Come e Quando Utilizzare `USER`

    1. **Creazione dell'Utente e del Gruppo**: Prima di utilizzare l'istruzione `USER` per passare a un utente non `root`, è necessario assicurarsi che tale utente (e, opzionalmente, il suo gruppo) esista nell'immagine. Questo viene tipicamente fatto utilizzando comandi come `groupadd` e `useradd` (standard su sistemi basati su Debian/Ubuntu) all'interno di un'istruzione `RUN`.

        ```dockerfile
        # Esempio di immagine Docker creata a partire da alpine

        # Immagine base
        FROM alpine:latest

        # Argomenti per UID e GID per flessibilità
        ARG APP_USER_UID=1000
        ARG APP_USER_GID=1000

        # Creazione del gruppo e dell'utente
        # Si utilizza 'addgroup' e 'adduser' che sono comuni in Alpine
        RUN addgroup -g ${APP_USER_GID} -S appgroup && \
            adduser -u ${APP_USER_UID} -S appuser -G appgroup -h /home/appuser

        # Crea la directory dell'applicazione e imposta i permessi
        WORKDIR /app
        COPY --chown=appuser:appgroup . /app

        # Installa eventuali dipendenze (come root, se necessario, prima di cambiare utente)
        # RUN apk add --no-cache some-dependency

        # Passa all'utente non privilegiato
        USER appuser

        # Verifica l'utente corrente (opzionale, per debug durante la build)
        RUN whoami

        # Imposta il comando predefinito per il container
        ENTRYPOINT ["./my-app-executable"]
        CMD ["--default-arg"]
        ```

        In questo esempio:

          1. Viene creata una coppia `appgroup`/`appuser` con UID/GID specifici. L'opzione `-S` per `addgroup`/`adduser` in Alpine crea un utente/gruppo di sistema senza password e senza login shell interattiva (più sicuro). `-h /home/appuser` crea una home directory.
          2. I file dell'applicazione vengono copiati e i loro permessi vengono assegnati al nuovo utente `appuser` durante l'operazione `COPY` grazie all'opzione `--chown`.
          3. L'istruzione `USER appuser` imposta l'utente `appuser` come utente corrente.
          4. L'istruzione `RUN whoami` (se presente) verrebbe eseguita come `appuser`.
          5. L'`ENTRYPOINT` e il `CMD` verranno eseguiti come `appuser` quando il container viene avviato.

        ```dockerfile
        # Esempio di immagine Docker creata a partire da Ubuntu

        FROM ubuntu:latest # O una versione specifica come ubuntu:22.04

        # Aggiorna i pacchetti e installa eventuali dipendenze necessarie per la creazione dell'utente o l'app
        RUN apt-get update && apt-get install -y --no-install-recommends sudo\
            && rm -rf /var/lib/apt/lists/*

        # Crea un gruppo e un utente specifici
        # -r crea un utente di sistema
        # -g specifica il gruppo primario
        # -m crea la directory home
        # -s /usr/sbin/nologin impedisce il login shell
        RUN groupadd -r appgroup &&\
            useradd -r -g appgroup -m -s /usr/sbin/nologin appuser

        # (altre istruzioni come la copia dei file dell'applicazione, installazione dipendenze, ecc.)
        # Ad esempio, creare la directory dell'applicazione se non copiata direttamente con permessi
        RUN mkdir -p /opt/app && chown appuser:appgroup /opt/app
        WORKDIR /opt/app
        COPY --chown=appuser:appgroup ./mia_applicazione /opt/app/mia_applicazione

        # Passa all'utente non privilegiato
        USER appuser

        # Le istruzioni successive verranno eseguite come 'appuser'
        ENTRYPOINT ["/opt/app/mia_applicazione"]
        CMD ["--config", "/opt/app/config.json"]
        ```

    2. **Posizionamento nel Dockerfile**: L'istruzione `USER` dovrebbe essere inserita il più tardi possibile nel Dockerfile, ma prima di qualsiasi comando (`CMD`, `ENTRYPOINT`) o operazione (`RUN`) che non richieda privilegi di `root`. Tipicamente:

        - Si eseguono come `root` le operazioni di installazione di pacchetti (`apt-get install`), creazione di directory di sistema, e altre configurazioni iniziali.
        - Dopo aver preparato l'ambiente e copiato i file dell'applicazione, e aver impostato le corrette permissioni per l'utente non privilegiato, si utilizza `USER` per passare a tale utente.
        - Le istruzioni `CMD` o `ENTRYPOINT` verranno quindi eseguite con i permessi dell'utente specificato.

  - Impatto sulle Istruzioni Successive

    Una volta che l'istruzione `USER` è stata specificata, tutte le istruzioni `RUN`, `CMD` e `ENTRYPOINT` che seguono nel Dockerfile saranno eseguite nel contesto di quell'utente.

    - **`RUN`**: **I comandi eseguiti tramite `RUN` verranno eseguiti come l'utente specificato**. Questo è importante da considerare se questi comandi necessitano di scrivere in directory o modificare file che richiedono privilegi specifici.
    - **`CMD` e `ENTRYPOINT`**: **Il processo principale del container, definito da `CMD` o `ENTRYPOINT` (o dalla loro combinazione), verrà avviato con l'identità dell'utente specificato**. Questo è l'aspetto più cruciale per la sicurezza in runtime del container.

  - Esempio Completo con Container Ubuntu

    ```dockerfile
    # Immagine base Ubuntu
    FROM ubuntu:22.04

    # Argomenti per UID e GID per flessibilità (opzionale, ma buona pratica)
    ARG APP_USER_UID=1001
    ARG APP_USER_GID=1001
    ARG APP_USER_NAME=appuser
    ARG APP_GROUP_NAME=appgroup

    # Aggiorna la lista dei pacchetti e installa dipendenze minime se necessario
    # Esempio: installazione di 'curl' o altri tool usati da RUN o dall'app
    RUN apt-get update &&\
        apt-get install -y --no-install-recommends curl &&\
        rm -rf /var/lib/apt/lists/*

    # Creazione del gruppo e dell'utente
    # -r per utente/gruppo di sistema
    # -g assegna il gruppo primario
    # -m crea la home directory (se non specificato -d)
    # -d specifica la home directory
    # -s /usr/sbin/nologin per impedire login interattivi (più sicuro)
    RUN groupadd -r -g ${APP_USER_GID} ${APP_GROUP_NAME} &&\
        useradd -r -u ${APP_USER_UID} -g ${APP_GROUP_NAME} -m -d /home/${APP_USER_NAME} -s /usr/sbin/nologin ${APP_USER_NAME}

    # Crea la directory dell'applicazione
    RUN mkdir -p /srv/app &&\
        chown ${APP_USER_NAME}:${APP_GROUP_NAME} /srv/app

    # Imposta la directory di lavoro
    WORKDIR /srv/app

    # Copia i file dell'applicazione e imposta i permessi
    # Assumendo che il Dockerfile sia nella root del progetto e l'app sia in 'src/'
    COPY --chown=${APP_USER_NAME}:${APP_GROUP_NAME} ./src/app.py /srv/app/app.py
    COPY --chown=${APP_USER_NAME}:${APP_GROUP_NAME} ./src/requirements.txt /srv/app/requirements.txt

    # Esempio: installa dipendenze Python come utente non-root in un ambiente virtuale (best practice)
    # Prima si passa all'utente, poi si creano ambienti/installano dipendenze specifiche dell'utente
    USER ${APP_USER_NAME}

    # Esempio: installazione di dipendenze Python in ambiente virtuale come utente non-root
    # RUN python3 -m venv venv
    # RUN . venv/bin/activate && pip install --no-cache-dir -r requirements.txt
    # ENTRYPOINT ["./venv/bin/python", "app.py"]

    # Se non si usa un ambiente virtuale e python3 è globale (e l'utente ha permessi):
    # RUN pip3 install --user --no-cache-dir -r requirements.txt
    # ENTRYPOINT ["python3", "app.py"]

    # Oppure, se le dipendenze sono state installate globalmente da root:
    # ENTRYPOINT ["python3", "app.py"]
    # Per questo esempio, assumiamo che 'app.py' sia un eseguibile e non richieda python direttamente nel comando
    ENTRYPOINT ["./app.py"]
    CMD ["--help"]

    # Verifica l'utente corrente (opzionale, per debug durante la build)
    # RUN whoami # Questo comando verrebbe eseguito come 'appuser'
    ```

    In questo esempio specifico per Ubuntu:

    1. Viene utilizzata un'immagine base `ubuntu:22.04`.
    2. Vengono creati un gruppo (`appgroup`) e un utente (`appuser`) di sistema utilizzando `groupadd -r` e `useradd -r`. L'opzione `-m` per `useradd` crea la directory home dell'utente (qui `/home/appuser`). `/usr/sbin/nologin` è una shell comune per utenti di servizio che non necessitano di login interattivo.
    3. Viene creata una directory `/srv/app` per l'applicazione, e i suoi permessi vengono assegnati al nuovo utente.
    4. I file dell'applicazione (`app.py`, `requirements.txt` da una sottodirectory `src/`) vengono copiati nella `WORKDIR` e i loro permessi sono impostati all'utente `appuser` durante l'operazione `COPY` grazie all'opzione `--chown`.
    5. L'istruzione `USER ${APP_USER_NAME}` imposta l'utente `appuser` come utente corrente per le istruzioni successive.
    6. L'`ENTRYPOINT` e il `CMD` (qui, per un ipotetico `app.py`) verranno eseguiti come `appuser` quando il container viene avviato. Sono mostrati anche commenti per approcci alternativi con Python e virtual environments.
    7. Affinché un file di script Python (come `app.py`) possa essere trattato come un "eseguibile" in questo senso, devono essere soddisfatte principalmente due condizioni all'interno dell'ambiente del container (in questo caso, basato su Ubuntu):

       1. **La "Shebang" Line (Riga Shebang)**:

           - Il file `app.py` deve iniziare con una riga speciale chiamata "shebang". Questa riga indica al sistema operativo quale interprete utilizzare per eseguire lo script.
           - Per uno script Python 3, la shebang line è tipicamente:Python

               ```py
               #!/usr/bin/env python3
               ```

               oppure, se si conosce il percorso esatto dell'interprete Python 3 e si preferisce usarlo direttamente:Python

               ```py
               #!/usr/bin/python3
               ```

           - Quando il sistema tenta di eseguire `./app.py`, legge questa prima riga e sa che deve invocare `/usr/bin/env python3` (che a sua volta troverà l'eseguibile `python3` nel `PATH` del sistema) oppure direttamente `/usr/bin/python3`, passando `app.py` come argomento a tale interprete.

       2. **Permessi di Esecuzione**:

           - Il file `app.py` deve avere i permessi di esecuzione impostati. Nei sistemi Unix-like (come Linux, su cui si basa Ubuntu), questo si ottiene solitamente con il comando `chmod`.
           - Ad esempio, all'interno del Dockerfile o prima di aggiungere il file all'immagine, si potrebbe eseguire:Bash

               ```sh
               chmod +x app.py
               ```

               Oppure, se si copiano i file con `COPY` o `ADD`, si può tentare di preservare i permessi dal sistema host, o impostarli successivamente con `RUN chmod +x /srv/app/app.py`. L'opzione `--chown` in `COPY` gestisce solo la proprietà, non direttamente i permessi di esecuzione in modo granulare (anche se i permessi originali potrebbero essere preservati a seconda del client Docker e del sistema). È buona pratica assicurarsi che i permessi di esecuzione siano impostati nel Dockerfile dopo aver copiato il file, se necessario:Dockerfile

               ```sh
               COPY ./src/app.py /srv/app/app.py
               RUN chmod +x /srv/app/app.py
               ```

               (Nota: nell'esempio precedente, `COPY --chown` è stato usato, ma `chmod` potrebbe comunque essere necessario se il file sorgente non ha già i permessi di esecuzione).

  - Considerazioni Aggiuntive

    - **Permessi sui File**: È fondamentale assicurarsi che l'utente non `root` specificato con `USER` abbia i permessi di lettura, scrittura ed esecuzione necessari per i file e le directory con cui l'applicazione deve interagire. Questo spesso implica l'uso di `chown` e `chmod` (o l'opzione `--chown` in `COPY` o `ADD`) nelle istruzioni `RUN` precedenti al cambio di utente.
    - **Porte Privilegiate**: Gli utenti non `root` non possono associare servizi a porte privilegiate (quelle inferiori alla 1024). Se l'applicazione necessita di ascoltare su una porta come la 80 o la 443, si possono adottare strategie come:
        - Configurare l'applicazione per ascoltare su una porta non privilegiata (es. 8080) e mappare la porta privilegiata dell'host a questa porta non privilegiata durante l'esecuzione del container (es. `docker run -p 80:8080 mia-immagine`).
        - Utilizzare un reverse proxy (come Nginx o Apache) che gira come `root` (o con capacità `CAP_NET_BIND_SERVICE`) per legarsi alla porta privilegiata e inoltrare il traffico all'applicazione che gira come utente non privilegiato su una porta alta.
        - Concedere la capacità `CAP_NET_BIND_SERVICE` al binario dell'eseguibile (usando `setcap` all'interno del Dockerfile, se il kernel dell'host lo supporta e la configurazione di Docker lo permette), ma questa è una soluzione più avanzata e da usare con cautela. L'installazione di `libcap2-bin` (`apt-get install libcap2-bin`) è necessaria per il comando `setcap` su Ubuntu.

    In conclusione, l'istruzione `USER` è uno strumento essenziale per costruire immagini Docker sicure, limitando i privilegi con cui vengono eseguiti i processi all'interno dei container, indipendentemente dal sistema operativo base dell'immagine (come Alpine o Ubuntu).

##### `ARG` in dettaglio

- **Istruzione `ARG` (Argomenti di Build)**

  L'istruzione **`ARG`** definisce una variabile che gli utenti possono passare al builder Docker al momento della costruzione dell'immagine, utilizzando l'opzione `--build-arg <nome_variabile>=<valore>` del comando `docker build`.

  - Scopo Principale di `ARG`

    - **Parametrizzare il processo di build**: Consente di passare valori che influenzano la costruzione dell'immagine, come versioni di software da installare, URL di sorgenti, flag di compilazione, o nomi di utenti/gruppi da creare.
    - **Flessibilità**: Evita di dover modificare il Dockerfile per piccoli cambiamenti nei parametri di build.
    - **Non persistenza**: Le variabili `ARG` sono destinate principalmente all'uso durante la build e, per impostazione predefinita, non sono disponibili come variabili d'ambiente nel container in esecuzione né persistono nei metadati dell'immagine in modo accessibile al runtime.

  - Sintassi di `ARG`

    1. **Senza valore predefinito**:

        ```dockerfile
        ARG nome_variabile
        ```

        Se non viene fornito un valore tramite `--build-arg`, la variabile sarà una stringa vuota.

    2. **Con valore predefinito**:

        ```dockerfile
        ARG nome_variabile=valore_predefinito
        # ad esempio
        ARG UBUNTU_VERSION=22.04
        ```

        Il valore predefinito viene utilizzato se non sovrascritto da `--build-arg`.

  - Ambito di Validità di `ARG`

    - **Prima di `FROM`**: Un `ARG` dichiarato prima della prima istruzione `FROM` è "globale" e può essere utilizzato nell'istruzione `FROM` stessa (es. `ARG TAG=latest FROM ubuntu:${TAG}`). Tuttavia, per utilizzare questo `ARG` *all'interno* di uno stage di build (dopo `FROM`), deve essere ridichiarato (es. `ARG TAG`).
    - **Dopo `FROM`**: Un `ARG` dichiarato all'interno di uno stage di build (dopo `FROM`) è disponibile solo da quel punto fino alla fine dello stage o fino alla successiva istruzione `FROM`.

  - Utilizzo di `ARG`

    Le variabili `ARG` sono accessibili durante la build da istruzioni come `RUN`, `COPY`, `ADD`, `USER`, `ENV`, ecc., utilizzando la sintassi `${nome_variabile}` o `$nome_variabile`.

    ```dockerfile
    ARG USER_NAME=guest
    ARG APP_VERSION=1.0

    FROM alpine
    ARG USER_NAME # Rende USER_NAME disponibile nello stage
    ARG APP_VERSION
    RUN adduser -S "$USER_NAME"
    LABEL version="${APP_VERSION}"
    # ...
    ```

  - Considerazioni su `ARG`

    - I valori di `ARG` non sono disponibili per l'applicazione in esecuzione nel container a meno che non vengano esplicitamente usati per impostare una variabile `ENV` o scritti in un file.
    - **Sicurezza**: I valori degli `ARG` possono essere visibili nella history dell'immagine (`docker history`) se usati in comandi che modificano i layer (es., `RUN echo $MY_ARG > /file`). Non usare `ARG` per passare segreti che non dovrebbero finire nell'immagine. Per segreti necessari solo durante la build, considerare l'uso di `--secret` con Docker BuildKit.

##### `ENV` in dettaglio

- **Istruzione `ENV` (Variabili d'Ambiente)**

  L'istruzione **`ENV`** imposta variabili d'ambiente. Queste variabili sono disponibili sia durante il processo di build (per le istruzioni successive a `ENV` nel Dockerfile) sia per l'applicazione in esecuzione all'interno del container avviato dall'immagine.

  - Scopo Principale di `ENV`

    - **Configurazione Runtime**: Fornisce variabili d'ambiente all'applicazione in esecuzione nel container (es. `NODE_ENV=production`, `DB_HOST=db.example.com`, `API_KEY=...`).
    - **Configurazione Build**: Può essere utilizzata anche da comandi `RUN` durante la build (es. per impostare `PATH`, `JAVA_HOME`, o configurare tool di build).
    - **Persistenza**: Le variabili `ENV` sono "cotte" nell'immagine e fanno parte dei suoi metadati.

  - Sintassi di `ENV`

    1. **Coppia chiave-valore (preferita)**:

        ```dockerfile
        ENV <chiave>=<valore>
        # Ad esempio:
        ENV APP_MODE=production
        ```

    2. **Sintassi alternativa per singola variabile (separata da spazio)**:

        ```dockerfile
        ENV <chiave> <valore>
        # Ad esempio:
        ENV APP_MODE production
        ```

    3. **Multiple variabili in una singola istruzione `ENV`**:

        ```dockerfile
        ENV <chiave1>=<valore1> <chiave2>=<valore2> ...
        # Ad esempio:
        ENV APP_NAME="My App" APP_PORT=8080
        ```

  - Ambito di Validità di `ENV`

    - Una variabile `ENV` è disponibile da quando viene dichiarata in poi, per tutte le istruzioni successive nel Dockerfile (incluso `RUN`, `CMD`, `ENTRYPOINT`).
    - Persiste nell'immagine e viene ereditata da tutti i container avviati da quell'immagine.
    - Può essere sovrascritta al momento dell'avvio del container tramite l'opzione `-e` o `--env` del comando `docker run`.

  - Utilizzo di `ENV`

      ```dockerfile
      FROM ubuntu:22.04

      ENV APP_DIR /opt/app
      ENV DEBIAN_FRONTEND=noninteractive # Configura l'ambiente per apt-get

      WORKDIR ${APP_DIR} # Utilizza la variabile ENV

      RUN apt-get update && apt-get install -y nginx\
          && echo "Applicazione installata in ${APP_DIR}"

      # La variabile APP_DIR sarà disponibile anche per l'applicazione nel container
      CMD ["nginx", "-g", "daemon off;"]
      ```

  - Considerazioni su `ENV`

    - **Sicurezza**: Poiché le variabili `ENV` sono incorporate nell'immagine e visibili (es. con `docker inspect`), non è consigliabile scrivere direttamente segreti sensibili (come password di produzione o chiavi API private) direttamente nel Dockerfile se l'immagine è distribuita pubblicamente o in ambienti non fidati. Per tali segreti, preferire meccanismi di iniezione al runtime (Docker secrets, variabili passate con `docker run -e`, file di configurazione montati come volumi).

- **Interazione e Differenze Chiave: `ARG` vs `ENV`**

    La distinzione fondamentale è:

    - **`ARG`**: Variabile per il **processo di build**, non automaticamente disponibile al runtime.
    - **`ENV`**: Variabile d'ambiente per il **runtime del container** (e disponibile anche durante la build dalle istruzioni successive alla sua definizione).

    | **Caratteristica** | **ARG** | **ENV** |
    | --- |  --- |  --- |
    | **Scopo Principale** | Parametrizzare la build (`docker build`) | Configurare l'ambiente del container (`docker run`) e della build. |
    | **Disponibilità** | Solo durante la build (non nel container finale di default) | Durante la build (dopo la sua definizione) E nel container in esecuzione. |
    | **Persistenza** | No (a meno che usata per impostare `ENV` o scritta in un file) | Sì, incorporata nell'immagine. |
    | **Valore Predefinito** | Sì, `ARG NOME=valore` | Sì, `ENV NOME=valore` (questo è il valore stesso, non un "default") |
    | **Sovrascrittura** | Da `--build-arg` in `docker build` | Da `-e` o `--env` in `docker run` |
    | **Visibilità Segreti** | Può essere visibile nella history dei layer. Sconsigliato per segreti. | Visibile nell'immagine (`docker inspect`). Sconsigliato per segreti scritti direttamente. |

  - Utilizzo di `ARG` per Impostare `ENV`

    Una pratica comune e potente è usare un `ARG` per impostare dinamicamente una variabile `ENV`. Questo permette di passare un valore al momento della build che diventa poi una variabile d'ambiente disponibile per l'applicazione nel container.

    ```dockerfile
    # Accetta una versione dell'API come argomento di build
    ARG API_VERSION_BUILDTIME=v1

    # Imposta una variabile d'ambiente runtime basata sull'ARG
    ENV API_ENDPOINT_VERSION=${API_VERSION_BUILDTIME}

    # L'applicazione nel container può ora leggere la variabile d'ambiente API_ENDPOINT_VERSION
    # Il suo valore sarà 'v1' o quello passato con --build-arg API_VERSION_BUILDTIME=...
    ```

    Al momento della build:

    ```sh
    docker build --build-arg API_VERSION_BUILDTIME=v2-beta -t my-service .
    ```

    Il container avviato da my-service avrà `API_ENDPOINT_VERSION=v2-beta`.

- Argomenti ed `ENV` Predefiniti

  - **`ARG` Predefiniti**: Docker riconosce alcuni `ARG` predefiniti come `HTTP_PROXY`, `HTTPS_PROXY`, `FTP_PROXY`, `NO_PROXY` che, se impostati nell'ambiente del client Docker, possono essere usati automaticamente durante la build per configurare i proxy di rete. È comunque buona norma dichiararli esplicitamente nel Dockerfile se se ne fa affidamento.
  - **`ENV` Predefiniti**: Molte immagini base (es. `ubuntu`, `node`, `python`) forniscono già delle variabili `ENV` preimpostate (come `PATH`, `LANG`, `NODE_VERSION`, ecc.) utili per l'ambiente.

- Esempi Combinati

  ```dockerfile
  # Argomento per la versione di Ubuntu da usare come base (globale per FROM)
  ARG UBUNTU_TAG=22.04
  FROM ubuntu:${UBUNTU_TAG}

  # Argomenti specifici per questo stage di build
  ARG APP_USER=appdev
  ARG APP_VERSION_BUILD="1.0.0-dev"

  # Variabili d'ambiente
  ENV LANG=C.UTF-8\
      LC_ALL=C.UTF-8\
      APP_HOME=/srv/app\
      # Imposta una ENV basata su un ARG
      APP_VERSION=${APP_VERSION_BUILD}

  # Creazione utente basata su ARG
  RUN groupadd -r ${APP_USER} && useradd -r -g ${APP_USER} -d ${APP_HOME} ${APP_USER}

  # Impostazione della directory di lavoro usando ENV
  WORKDIR ${APP_HOME}

  # L'applicazione può usare APP_VERSION e APP_HOME al runtime
  COPY --chown=${APP_USER}:${APP_USER} . .

  USER ${APP_USER}

  ENTRYPOINT ["./start-app.sh"] # start-app.sh può usare $APP_VERSION, $APP_HOME
  ```

  Al momento della build, si potrebbe eseguire:

  ```sh
  docker build --build-arg UBUNTU_TAG=20.04 --build-arg APP_USER=produser --build-arg APP_VERSION_BUILD="2.0.1" -t myapp:latest .
  ```

- Conclusioni

  - Utilizzare **`ARG`** per passare parametri che personalizzano il **processo di build** dell'immagine. I suoi valori non sono intesi per essere accessibili direttamente al runtime del container.
  - Utilizzare **`ENV`** per definire variabili d'ambiente che sono necessarie sia per le istruzioni di build successive sia, e soprattutto, per l'**applicazione in esecuzione** all'interno del container.
  - La combinazione di `ARG` per impostare valori di `ENV` offre un meccanismo flessibile per configurare l'ambiente runtime dell'applicazione al momento della build.

  La scelta e l'uso corretto di `ARG` e `ENV` contribuiscono significativamente alla creazione di immagini Docker flessibili, manutenibili e sicure.



### 3.3. L'importanza del file `.dockerignore`

Similmente al file `.gitignore` usato da Git, un file `.dockerignore` permette di specificare quali file e directory presenti nel *contesto di build* (la directory da cui si esegue `docker build`) devono essere ignorati e non inviati al demone Docker durante il processo di build dell'immagine.

**Perché è importante**:

1. **Velocità di Build**: Evita di inviare file grandi e non necessari (come le directory `bin` e `obj`, la cartella `.git`, `node_modules` se non servono nell'immagine finale) al demone Docker. Meno dati da trasferire significa build più veloci, specialmente se il demone Docker non è locale.

2. **Caching dei Layer**: Se si copiano file che cambiano frequentemente ma non sono necessari per la build (es. file di log locali), si potrebbe invalidare inutilmente il cache dei layer di Docker, rallentando le build successive.

3. **Sicurezza**: Impedisce la copia accidentale di file sensibili (es. file `.env` locali, chiavi private) nell'immagine Docker.

4. **Evitare Sovrascritture Indesiderate**: Previene la copia di file dal contesto di build che potrebbero sovrascrivere file creati da passaggi precedenti nel `Dockerfile`.

Creare un file chiamato `.dockerignore` nella stessa directory del `Dockerfile` (quindi in `MyWebApiApp/`) con il seguente contenuto:

```dockerfile
# MyWebApiApp/.dockerignore

# ===== ARTEFATTI DI BUILD .NET =====
# Ignora le cartelle di build e output generate da .NET
**/bin/
**/obj/

# ===== FILE DEGLI IDE =====
# File specifici di Visual Studio
*.user
*.vspscc
*.suo
.vs/

# File di Visual Studio Code (opzionale)
.vscode/

# ===== FILE DI SVILUPPO =====
# Repository Git (non necessario nell'immagine)
.git/
.gitignore

# File Docker (evita loop di copia)
Dockerfile*
.dockerignore
docker-compose*
compose*

# ===== CONFIGURAZIONI LOCALI =====
# File di ambiente e segreti (SICUREZZA!)
.env
secrets.json
secrets.dev.yaml
**/*.*proj.user

# ===== FILE TEMPORANEI =====
# Log e file temporanei
*.log
**/npm-debug.log

# ===== DOCUMENTAZIONE =====
LICENSE
README.md

```

Questo è un esempio. Adattarlo in base alle necessità specifiche del progetto. Le voci più importanti per un progetto .NET sono `**/bin/` e `**/obj/`.

### 3.4. Costruzione di un'immagine: `docker build` e primo test di avvio

**Una volta che si dispone di un `Dockerfile` e di un file `.dockerignore`, si può costruire l'immagine Docker**.

1. Aprire un terminale nella directory che contiene il `Dockerfile` (in questo caso, `MyWebApiApp/`).

2. Eseguire il comando `docker build`:

    ```sh
    docker build -t mywebapiapp-image:1.0 .
    ```

    - `docker build`: Il comando per costruire un'immagine.

    - `-t mywebapiapp-image:1.0`: L'opzione `-t` (o `--tag`) assegna un nome e un tag all'immagine nel formato `nomeimmagine:tag`.

        - `mywebapiapp-image` è il nome scelto per l'immagine.

        - `1.0` è un tag, tipicamente usato per versionare l'immagine. Se si omette il tag, viene usato `latest` di default.

    - `.`: L'ultimo argomento specifica il **contesto di build**. Il `.` indica la directory corrente. Docker cercherà il `Dockerfile` in questa directory e invierà il contenuto di questa directory (esclusi i file specificati in `.dockerignore`) al demone Docker.

    Durante il processo di build, Docker eseguirà ogni istruzione nel `Dockerfile` sequenzialmente, stampando l'output di ogni passo. Se un passo fallisce, la build si interrompe. Se la build ha successo, l'immagine sarà disponibile localmente. Si può verificare con:

    ```sh
    docker images
    ```

    Questo comando elencherà tutte le immagini Docker presenti sul sistema, inclusa `mywebapiapp-image:1.0`.

    Prima di passare alla pubblicazione dell'immagine su Docker Hub si provi ad eseguire localmente un container docker a partire dall'immagine appena creata. [I dettagli relativi al port mapping](../getting-started/index.md#docker-networking---port-mapping) sono già stati discussi in precedenza e sono anche riportati nei paragrafi successivi con maggiore dettaglio. In questa fase si vuole solo testare il container per verificare che funzioni correttamente.

    ```sh
    docker run --name mywebapiapp-container-run -p 8081:8080 mywebapiapp-image:1.0
    ```

    In questo caso se si prova ad aprire il browser all'indirizzo http://localhost:8081 si vedrà la home page dell'applicazione esattamente come quando si era lanciata l'applicazione dopo la pubblicazione (`dotnet publish`). Nella console dove si è lanciato il comando docker run si dovrebbero invece vedere i messaggi di log dell'applicazione, con un output simile al seguente:

    ```ps1
     ~  docker run --name mywebapiapp-container-run -p 8081:8080 mywebapiapp-image:1.0
    warn: Microsoft.AspNetCore.Hosting.Diagnostics[15]
        Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://+:8080'.
    info: Microsoft.Hosting.Lifetime[14]
        Now listening on: http://[::]:8080
    info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Production
    info: Microsoft.Hosting.Lifetime[0]
        Content root path: /app
    ```

    Per chiudere l'applicazione basta digitare Ctrl+C nella console da cui si è avviata.

### 3.5. Pubblicazione di un'immagine su un registro

Una volta costruita un'immagine localmente, la si può pubblicare su un *registro di container*. Un registro è un repository per archiviare e distribuire immagini Docker. Docker Hub è il registro pubblico più popolare, ma esistono anche registri privati o forniti da piattaforme cloud (es. Azure Container Registry, Amazon ECR, Google Container Registry).

#### 3.5.1 Pubblicazione di un'immagine su Docker Hub

Per pubblicare su Docker Hub:

1. **Creare un Account Docker Hub**: Se non se ne possiede uno, registrarsi su [hub.docker.com](https://hub.docker.com/ "null").

2. **Login a Docker Hub dal terminale**:

    ```sh
    docker login
    ```

    Verranno richiesti username e password di Docker Hub.

3. Taggare l'immagine con il proprio username Docker Hub:

    Per poter fare la push di un'immagine su Docker Hub, questa deve essere taggata con il formato <username_dockerhub>/<nome_immagine>:<tag>.

    Sostituire *ilmiousernamedockerhub* con il proprio username effettivo.

    ```sh
    docker tag mywebapiapp-image:1.0 ilmiousernamedockerhub/mywebapiapp-image:1.0

    ```

    Questo comando crea un alias per l'immagine esistente. `docker images` mostrerà ora entrambe le etichette che puntano alla stessa immagine ID.

4. **Push dell'immagine su Docker Hub**:

    ```sh
    docker push ilmiousernamedockerhub/mywebapiapp-image:1.0

    ```

    Questo comando carica l'immagine sul proprio repository Docker Hub. Una volta completato, l'immagine sarà accessibile pubblicamente (o privatamente, a seconda delle impostazioni del repository su Docker Hub) e potrà essere scaricata da qualsiasi macchina con Docker usando `docker pull ilmiousernamedockerhub/mywebapiapp-image:1.0`.

5. **Repository privato su Docker Hub**

   Per impostazione predefinita quando si effettua una push di una immagine Docker su Docker Hub l'immagine viene inserita in un repository che ha lo stesso nome del tag che si è dato all'immagine e che è pubblica, quindi accessibile da chiunque. Per caricare un'immagine Docker in un repository privato ci sono due opzioni:

  - **Opzione 1: Rendere privato il repository esistente**

      1. **Andare su Docker Hub** (hub.docker.com)
      2. **Accedere** al proprio account
      3. **Trovare il proprio repository** `ilmiousernamedockerhub/mywebapiapp-image`
      4. **Cliccare sul repository** per aprirlo
      5. **Andare su "Settings"** (tab in alto)
      6. **Scorrere fino a "Repository visibility"**
      7. **Selezionare "Private"**
      8. **Cliccare "Make Private"** e confermare

  - **Opzione 2: Creare un nuovo repository privato prima di fare la push**

    Se si vuole creare un repository completamente nuovo e averlo da subito privato:

       1. **Su Docker Hub**, cliccare **"Create Repository"**
       2. **Nome**: `mywebapiapp-private` (o quello che si preferisce)
       3. **Visibility**: Selezionare **"Private"**
       4. **Cliccare "Create"**

    Poi effettuare il tagging e la push:

    ```ps
    # Tag con il nuovo nome
    docker tag mywebapiapp-image:1.0 ilmiousernamedockerhub/mywebapiapp-private:1.0

    # Push del nuovo repository privato
    docker push ilmiousernamedockerhub/mywebapiapp-private:1.0
    ```

  - **Importante da sapere:**

     - **Account gratuito**: Solo 1 repository privato
     - **Repository privato**: Solo tu puoi vedere e scaricare l'immagine
     - **Collaboratori**: Puoi invitare altri utenti a accedere al repository privato
     - **Limiti pull**: Anche per i repository privati ci sono limiti di download

  - **Per verificare che sia privato:**

     - Il repository mostrerà un'icona con il lucchetto 🔒
     - Altri utenti non potranno fare `docker pull` senza autorizzazione

#### 3.5.2 Pubblicazione di un'immagine su Azure Container Registry

- **Prerequisiti ⚙️**

    Prima di iniziare, è necessario assicurarsi di avere a disposizione:

    1. **Un account Azure attivo:** Se non se ne possiede uno, è possibile crearne uno [gratuitamente per studenti](https://azure.microsoft.com/it-it/free/students).
    2. **Azure CLI installata:**
        - **PowerShell:** Può essere installata seguendo le [istruzioni ufficiali Microsoft per Windows](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows).

        ```ps1
            # Nella PowerShell
            winget install --exact --id Microsoft.AzureCLI
        ```

        :memo::fire:**Importante**: Dopo l'installazione di Azure CLI occorre aprire un'altra istanza di PowerShell per poter cominciare ad usare i comandi az. 

        - **WSL (Ubuntu):** Può essere installata seguendo le istruzioni per Linux:Bash

            ```sh
            curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

            ```

    3. **Docker Desktop installato su Windows:** Assicurarsi che Docker Desktop sia in esecuzione e configurato per utilizzare il backend WSL 2 per prestazioni ottimali.
    4. **Un ambiente WSL (es. Ubuntu) configurato.**
    5. **Un progetto di esempio ASP.NET Core Minimal API**

- Passaggi per la Push dell'Immagine Docker su Azure Container Registry

    1. Creazione di un'istanza di Azure Container Registry (ACR) ☁️

        Azure Container Registry è un servizio gestito di registro Docker privato basato su Docker Registry 2.0 open-source.

        Si può creare un ACR tramite il portale Azure, Azure PowerShell o Azure CLI. Di seguito, i passaggi tramite **Azure CLI**, eseguibili sia da PowerShell che dalla bash di WSL.

        Prima di tutto, è necessario accedere al proprio account Azure. I comandi della shell riportati di seguito sono eseguiti nella PowerShell, ma potrebbero essere eseguiti anche nella bash a patto di aver installato Azure CLI:

        ```ps1
        az login
        ```

        Questo comando aprirà una finestra del browser per l'autenticazione.

        Successivamente, creare un gruppo di risorse (se non se ne possiede già uno in cui inserire il registry). Un gruppo di risorse è un contenitore logico in cui vengono distribuite e gestite le risorse di Azure.

        ```ps1
        az group create --name DotnetDemos --location italynorth
        ```

        Se l'operazione di creazione del gruppo di risorse ha successo si dovrebbe avere una risposta del tipo seguente:

        ```json
        {
        "id": "/subscriptions/<IdSottoscrizione>/resourceGroups/DotnetDemos",
        "location": "italynorth",
        "managedBy": null,
        "name": "DotnetDemos",
        "properties": {
            "provisioningState": "Succeeded"
        },
        "tags": null,
        "type": "Microsoft.Resources/resourceGroups"
        }
        ```

        Sostituire `DotnetDemos` con il nome desiderato per il gruppo di risorse e `italynorth` con la località Azure preferita (se non dovesse essere disponibile `italynorth`, provare, ad esempio, `westeurope`).

        Ora, creare l'Azure Container Registry. Il nome del registry deve essere univoco a livello globale.

        ```ps1
        az acr create --resource-group DotnetDemos --name ilmioregistrocontainerunico --sku Basic --admin-enabled true
        ```

        - `--name ilmioregistrocontainerunico`: Sostituire con un nome univoco per il registry (solo caratteri alfanumerici minuscoli).
        - `--sku`: Definisce il livello di servizio. Le opzioni comuni sono `Basic`, `Standard` e `Premium`. `Basic` è sufficiente per iniziare.
        - `--admin-enabled true`: Abilita l'account amministratore, che fornisce credenziali semplici (nome utente e password) per accedere al registry. Questo è utile per scenari di test o sviluppo individuali. Per ambienti di produzione, è consigliabile utilizzare i principal di servizio di Azure.

        Una volta creato, prendere nota del valore `loginServer` restituito nell'output JSON. Sarà qualcosa come `ilmioregistrocontainerunico.azurecr.io`. Questo è l'URL del server di accesso del registry.

        Ad esempio, con il comando:

        ```ps1
        az acr create --resource-group DotnetDemos --name malafronte --sku Basic --admin-enabled true
        ```

        Si ottiene un output simile al seguente:

        ```json
        {
        "adminUserEnabled": true,
        "anonymousPullEnabled": false,
        "autoGeneratedDomainNameLabelScope": "Unsecure",
        "creationDate": "<TimeStamp>",
        "dataEndpointEnabled": false,
        "dataEndpointHostNames": [],
        "encryption": {
            "keyVaultProperties": null,
            "status": "disabled"
        },
        "id": "/subscriptions/<IdSottoscrizione>/resourceGroups/DotnetDemos/providers/Microsoft.ContainerRegistry/registries/malafronte",
        "identity": null,
        "location": "italynorth",
        "loginServer": "malafronte.azurecr.io",
        "metadataSearch": "Disabled",
        "name": "malafronte",
        "networkRuleBypassOptions": "AzureServices",
        "networkRuleSet": null,
        "policies": {
            "azureAdAuthenticationAsArmPolicy": {
            "status": "enabled"
            },
            "exportPolicy": {
            "status": "enabled"
            },
            "quarantinePolicy": {
            "status": "disabled"
            },
            "retentionPolicy": {
            "days": 7,
            "lastUpdatedTime": "<TimeStamp>",
            "status": "disabled"
            },
            "softDeletePolicy": {
            "lastUpdatedTime": "<TimeStamp>",
            "retentionDays": 7,
            "status": "disabled"
            },
            "trustPolicy": {
            "status": "disabled",
            "type": "Notary"
            }
        },
        "privateEndpointConnections": [],
        "provisioningState": "Succeeded",
        "publicNetworkAccess": "Enabled",
        "resourceGroup": "DotnetDemos",
        "roleAssignmentMode": "LegacyRegistryPermissions",
        "sku": {
            "name": "Basic",
            "tier": "Basic"
        },
        "status": null,
        "systemData": {
            "createdAt": "<TimeStamp>",
            "createdBy": "<AzureUserId>",
            "createdByType": "User",
            "lastModifiedAt": "<TimeStamp>",
            "lastModifiedBy": "<AzureUserId>",
            "lastModifiedByType": "User"
        },
        "tags": {},
        "type": "Microsoft.ContainerRegistry/registries",
        "zoneRedundancy": "Disabled"
        }
        ```

    2. Login ad Azure Container Registry da Docker 🔑

        Prima di poter effettuare la push di un'immagine, Docker deve essere autenticato con l'ACR.

        Si può utilizzare il comando `az acr login` che sfrutta le credenziali Azure CLI per autenticare Docker. Questo è il metodo consigliato.

        Eseguire questo comando nella stessa shell (PowerShell o WSL bash) in cui si è eseguito `az login` e dove si eseguiranno i comandi Docker:

        ```ps1
        az acr login --name ilmioregistrocontainerunico
        ```

        Ad esempio, se il registro ACR si chiama `malafronte` (nome del registro ACR senza il suffisso di dominio `.azurecr.io`), occorre eseguire il comando:

        ```ps1
        az acr login --name malafronte
        ```

        Se il comando ha successo, si vedrà il messaggio "Login Succeeded". Docker Desktop (e il demone Docker in WSL) sarà ora configurato per comunicare con l'ACR specificato.

        **Alternativa (meno consigliata per lo sviluppo quotidiano): Login con credenziali Admin**

        Se si è abilitato l'utente amministratore (`--admin-enabled true` durante la creazione dell'ACR), si possono recuperare le credenziali:

        ```ps1
        az acr credential show --name ilmioregistrocontainerunico --resource-group MioGruppoRisorseACR
        ```

        Questo comando restituirà un `username` (che è il nome dell'ACR, es. `ilmioregistrocontainerunico`) e due password (`password` e `password2`).

        Si può quindi usare `docker login`:

        ```ps1
        docker login ilmioregistrocontainerunico.azurecr.io
        ```

        Verrà richiesto il nome utente e una delle password ottenute.

    3. Creazione di una applicazione ASP.NET Core con relativo Dockerfile (questo punto è già stato analizzato nei paragrafi precedenti)
    4. Build dell'immagine Docker a partire dal Dockerfile, oppure in alternativa con l'utilizzo del comando `dotnet publish -c Release /t:PublishContainer` (anche questo aspetto è già stato analizzato nei paragrafi precedenti)
    5. Tagging dell'Immagine per Azure Container Registry 🏷️

        Prima di poter effettuare la push dell'immagine su ACR, è necessario effettuare su di essa un tagging con il nome completo del server di login del proprio ACR.

        Il formato del tag è: `<loginServer>/<nomeImmagine>:<tag>`

        Usare il `loginServer` annotato nel Passaggio 1 (es. `ilmioregistrocontainerunico.azurecr.io`).

        ```ps1
        docker tag mywebapiapp-sdkpublished:1.0 ilmioregistrocontainerunico.azurecr.io/mywebapiapp-sdkpublished:1.0
        ```

        - `mywebapiapp-sdkpublished:1.0`: Il nome e il tag dell'immagine locale.
        - `ilmioregistrocontainerunico.azurecr.io/mywebapiapp-sdkpublished:1.0`: Il nome completo dell'immagine per ACR. Si può usare lo stesso nome e tag, o cambiarli se necessario.

        Nel caso specifico degli esempi fatti in questi paragrafi, l'istruzione per il tagging è:

        ```ps1
        docker tag mywebapiapp-sdkpublished:1.0 malafronte.azurecr.io/mywebapiapp-sdkpublished:1.0
        ```

        **Verifica del nuovo tag (opzionale):**

        ```ps1
        docker images
        ```

        Ora si dovrebbero vedere due immagini con lo stesso ID immagine: una con il tag locale (`mywebapiapp-sdkpublished:1.0`) e una con il tag per ACR (`ilmioregistrocontainerunico.azurecr.io/mywebapiapp-sdkpublished:1.0`).

    6. Push dell'Immagine su Azure Container Registry 🚀

        Ora che l'immagine è taggata correttamente e si è autenticati con ACR (Passaggio 2), si può effettuare il push:

        ```ps1
        docker push ilmioregistrocontainerunico.azurecr.io/mywebapiapp-sdkpublished:1.0
        ```

        Nel caso concreto dell'esempio fatto:

        ```ps1
        docker push malafronte.azurecr.io/mywebapiapp-sdkpublished:1.0
        ```

        Docker caricherà i layer dell'immagine nel proprio Azure Container Registry. Il tempo necessario dipenderà dalla dimensione dell'immagine e dalla velocità della connessione internet.

    7. Verifica dell'Immagine in Azure Container Registry ✅

        Si può verificare che l'immagine sia stata caricata correttamente in diversi modi:

        **a) Tramite il Portale Azure:**

        1. Navigare nel Portale Azure.
        2. Cercare e selezionare il proprio Azure Container Registry (es. `ilmioregistrocontainerunico`).
        3. Nel menu del registry, sotto "Servizi", selezionare "Repository".
        4. Si dovrebbe vedere un repository chiamato `mywebapiapp-sdkpublished` (o il nome immagine scelto).
        5. Cliccando sul repository, si vedranno i tag disponibili (es. `1.0`).

        **b) Tramite Azure CLI:**

        Elencare i repository nel proprio ACR:

        ```ps1
        az acr repository list --name ilmioregistrocontainerunico --output table
        ```

        Nel caso dell'esempio concreto:

        ```ps1
        az acr repository list --name malafronte --output table
        ```

        Si dovrebbe vedere `mywebapiapp-sdkpublished` nell'elenco.

        Elencare i tag per un repository specifico:

        ```ps1
        az acr repository show-tags --name ilmioregistrocontainerunico --repository mywebapiapp-sdkpublished --output table
        ```

        Nel caso dell'esempio concreto:

        ```ps1
        az acr repository show-tags --name malafronte --repository mywebapiapp-sdkpublished --output table
        ```

        Si dovrebbe vedere il tag `1.0`.

    Con questi passaggi, l'immagine Docker del progetto ASP.NET Core Minimal API è stata costruita, taggata e caricata con successo nel proprio Azure Container Registry. Da qui, può essere utilizzata per distribuzioni in servizi Azure come Azure App Service, Azure Kubernetes Service (AKS), Azure Container Instances (ACI), ecc.

### 3.6. Cenni sulla generazione automatica di `Dockerfile` (Visual Studio e altri strumenti)

Moderni IDE come Visual Studio e Visual Studio Code (con l'estensione Docker) offrono funzionalità per generare automaticamente un `Dockerfile` di base quando si aggiunge il supporto Docker a un progetto ASP.NET Core.

- **Visual Studio**:

    - Facendo clic destro sul progetto in Esplora Soluzioni -> Aggiungi -> Supporto Docker...

    - Viene chiesto il sistema operativo target per i container (Linux o Windows).

    - Visual Studio aggiunge un `Dockerfile` (e spesso anche i file per Docker Compose) al progetto.

- **Visual Studio Code**:

    - Installare l'estensione "Containers" di Microsoft.

    - Aprire la palette dei comandi (Ctrl+Shift+P) e cercare "Docker: Add Docker Files to Workspace...".

    - Seguire le istruzioni per selezionare l'applicazione, la piattaforma (.NET: ASP.NET Core), il sistema operativo e la porta.

Questi `Dockerfile` generati automaticamente sono un buon punto di partenza e spesso includono già build multi-stadio. Tuttavia, è sempre fondamentale capire il contenuto del `Dockerfile` per poterlo personalizzare o risolvere problemi. Potrebbero necessitare di aggiustamenti per ottimizzazioni specifiche, gestione delle variabili d'ambiente o inclusione di strumenti aggiuntivi.

- **Esempio di `Dockerfile` per ASP.NET Core Minimal API generato automaticamente in VS Code**
  
  ```dockerfile
    # ===== STAGE 1: BASE =====
    # Immagine di runtime ASP.NET Core 9.0 per la fase finale
    # Questa sarà l'immagine base per l'esecuzione dell'applicazione
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

    # Imposta la directory di lavoro dove verrà eseguita l'app
    WORKDIR /app

    # Espone la porta 8080 per il traffico HTTP
    # Questa è solo documentazione - non pubblica effettivamente la porta
    EXPOSE 8080

    # Configura ASP.NET Core per ascoltare su tutte le interfacce sulla porta 8080
    ENV ASPNETCORE_URLS=http://+:8080

    # Passa all'utente 'app' per motivi di sicurezza (non root)
    USER app

    # ===== STAGE 2: BUILD =====
    # Immagine SDK .NET 9.0 per la compilazione, supporta multi-platform
    FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

    # Parametro per specificare la configurazione di build (default: Release)
    ARG configuration=Release

    # Directory di lavoro per i file sorgente
    WORKDIR /src

    # Copia solo il file .csproj per sfruttare il layer caching di Docker
    # Se le dipendenze non cambiano, Docker riutilizza i layer esistenti
    COPY ["MyWebApiApp.csproj", "./"]

    # Ripristina le dipendenze NuGet del progetto
    RUN dotnet restore "MyWebApiApp.csproj"

    # Copia tutti i file del progetto
    COPY . .

    # Torna nella directory sorgente
    WORKDIR "/src/."

    # Compila il progetto nella configurazione specificata
    # Output nella cartella /app/build
    RUN dotnet build "MyWebApiApp.csproj" -c $configuration -o /app/build

    # ===== STAGE 3: PUBLISH =====
    # Usa l'immagine di build precedente come base
    FROM build AS publish

    # Stesso parametro di configurazione
    ARG configuration=Release

    # Pubblica l'applicazione ottimizzata per il deployment
    # UseAppHost=false evita di creare un eseguibile nativo
    RUN dotnet publish "MyWebApiApp.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

    # ===== STAGE 4: FINAL =====
    # Ritorna all'immagine base di runtime (più leggera, senza SDK)
    FROM base AS final

    # Directory di lavoro per l'applicazione finale
    WORKDIR /app

    # Copia i file pubblicati dalla stage 'publish' nell'immagine finale
    # Questo mantiene l'immagine finale piccola (solo runtime + app)
    COPY --from=publish /app/publish .

    # Comando di avvio dell'applicazione
    # Esegue 'dotnet MyWebApiApp.dll' quando il container si avvia
    ENTRYPOINT ["dotnet", "MyWebApiApp.dll"]
  ```
  
- **Esempio di `.dockerignore` per ASP.NET Core Minimal API generato automaticamente in VS Code**
  
  ```dockerignore
    # MyWebApiApp/.dockerignore

    # ===== FILE SPECIFICI JAVA/ECLIPSE =====
    # File di configurazione Eclipse per progetti Java
    **/.classpath
    **/.project
    **/.settings

    # ===== FILE DOCKER =====
    # Evita di copiare i file Docker nell'immagine (previene loop di build)
    **/.dockerignore
    **/Dockerfile*
    **/docker-compose*
    **/compose*

    # ===== CONFIGURAZIONI AMBIENTE E SVILUPPO =====
    # File di ambiente locale (contengono spesso credenziali sensibili)
    **/.env

    # Repository Git (non necessario nell'immagine finale)
    **/.git
    **/.gitignore

    # ===== FILE SPECIFICI IDE/EDITOR =====
    # File di configurazione Visual Studio
    **/.vs
    **/*.user

    # File di configurazione Visual Studio Code
    **/.vscode

    # File di configurazione strumenti di sviluppo
    **/.toolstarget

    # ===== ARTEFATTI DI BUILD .NET =====
    # Cartelle di output della compilazione .NET
    **/bin
    **/obj

    # File di progetto utente specifici (impostazioni locali IDE)
    **/*.*proj.user

    # ===== FILE DATABASE E MODELLI =====
    # File di modello database (Database Model)
    **/*.dbmdl

    # File JFM (potrebbero essere file temporanei specifici)
    **/*.jfm

    # ===== NODE.JS (per progetti full-stack) =====
    # Dipendenze Node.js (da reinstallare con npm install)
    **/node_modules

    # File di debug NPM
    **/npm-debug.log

    # ===== KUBERNETES/HELM =====
    # Chart Helm per Kubernetes (solitamente gestiti separatamente)
    **/charts

    # File di configurazione Kubernetes per sviluppo
    **/secrets.dev.yaml
    **/values.dev.yaml

    # ===== DOCUMENTAZIONE =====
    # File di documentazione del progetto (non necessari nell'immagine runtime)
    LICENSE
    README.md
  ```

### 3.7. Confronto tra il Dockerfile generato da Containers (VS Code Plugin) e il Dockerfile semplice a due stadi scritto manualmente

#### Dockerfile completo (e complesso) - Analisi comando per comando

- **STAGE 1: BASE**

    ```dockerfile
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
    ```

    - **Cosa fa**: Scarica l'immagine runtime ASP.NET Core 9.0 e la etichetta come "base"

    - **Perché**: Prepara l'immagine finale leggera (solo runtime, senza SDK) che verrà usata nell'ultimo stage

    ```dockerfile
    WORKDIR /app
    ```

    - **Cosa fa**: Crea e imposta `/app` come directory di lavoro

    - **Perché**: Tutte le operazioni successive avverranno in questa cartella

    ```dockerfile
    EXPOSE 8080
    ```

    - **Cosa fa**: Documenta che l'applicazione userà la porta 8080

    - **Perché**: Aiuta Docker a sapere quale porta mappare automaticamente (solo documentazione)

    ```dockerfile
    ENV ASPNETCORE_URLS=http://+:8080
    ```

    - **Cosa fa**: Configura ASP.NET per ascoltare su tutte le interfacce di rete sulla porta 8080

    - **Perché**: Permette al container di ricevere richieste dall'esterno (non solo localhost)

    ```dockerfile
    USER app
    ```

    - **Cosa fa**: Cambia l'utente da root a "app" (utente non privilegiato)

    - **Perché**: **SICUREZZA** - riduce i rischi se il container viene compromesso

- **STAGE 2: BUILD**

    ```dockerfile
    FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
    ```

    - **Cosa fa**: Scarica l'SDK .NET 9.0 supportando build cross-platform

    - **Perché**: `--platform=$BUILDPLATFORM` permette build efficienti su architetture diverse (ARM, x64)

    ```dockerfile
    ARG configuration=Release
    ```

    - **Cosa fa**: Definisce una variabile `configuration` con valore default "Release"

    - **Perché**: Permette di personalizzare la build (es: `docker build --build-arg configuration=Debug`)

    ```dockerfile
    WORKDIR /src
    COPY ["MyWebApiApp.csproj", "./"]
    ```

    - **Cosa fa**: Imposta directory `/src` e copia SOLO il file .csproj

    - **Perché**: **OTTIMIZZAZIONE** - sfrutta il layer caching di Docker per le dipendenze

    ```dockerfile
    RUN dotnet restore "MyWebApiApp.csproj"
    ```

    - **Cosa fa**: Scarica tutte le dipendenze NuGet del progetto

    - **Perché**: Separa il restore dal resto del codice per velocizzare build successive

    ```dockerfile
    COPY . .
    WORKDIR "/src/."
    ```

    - **Cosa fa**: Copia tutto il codice sorgente e conferma la directory di lavoro

    - **Perché**: Dopo il restore, ora serve tutto il codice per la compilazione

    ```dockerfile
    RUN dotnet build "MyWebApiApp.csproj" -c $configuration -o /app/build
    ```

    - **Cosa fa**: Compila il progetto usando la configurazione specificata

    - **Perché**: Crea i file compilati in `/app/build` per il prossimo stage

- **STAGE 3: PUBLISH**

    ```dockerfile
    FROM build AS publish
    ARG configuration=Release
    ```

    - **Cosa fa**: Usa lo stage "build" come base e ridefinisce il parametro configuration 

    - **Perché**: Riutilizza tutto ciò che è già compilato, evitando duplicazioni

    ```dockerfile
    RUN dotnet publish "MyWebApiApp.csproj" -c $configuration -o /app/publish /p:UseAppHost=false
    ```

    - **Cosa fa**: Pubblica l'app ottimizzata per il deployment 

    - **Perché**:

    - Crea una versione ottimizzata per produzione
    - `/p:UseAppHost=false` evita di creare un eseguibile nativo (più piccolo)

- **STAGE 4: FINAL**

    ```dockerfile
    FROM base AS final
    WORKDIR /app
    ```

    - **Cosa fa**: Ritorna all'immagine "base" (solo runtime) e imposta directory

    - **Perché**: L'immagine finale sarà piccola (senza SDK né file temporanei)

    ```dockerfile
    COPY --from=publish /app/publish .
    ```

    - **Cosa fa**: Copia SOLO i file pubblicati dallo stage "publish"

    - **Perché**: L'immagine finale contiene solo ciò che serve per l'esecuzione

    ```dockerfile
    ENTRYPOINT ["dotnet", "MyWebApiApp.dll"]
    ```

    - **Cosa fa**: Definisce il comando di avvio del container

    - **Perché**: Quando il container si avvia, esegue automaticamente l'applicazione

#### Confronto Dockerfile semplice (2 stadi) vs Dockerfile complesso (4 stadi)

- **DOCKERFILE SEMPLICE (2 Stage)**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env  # Build
FROM mcr.microsoft.com/dotnet/aspnet:9.0           # Runtime
```

- **DOCKERFILE COMPLESSO (4 Stage)**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base      # Base runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build       # Build
FROM build AS publish                                 # Publish
FROM base AS final                                    # Final
```

- **MOTIVAZIONI PER LA COMPLESSITÀ**

  - **1. SICUREZZA MIGLIORATA**

  | Aspetto | Semplice | Complesso |
  | --- |  --- |  --- |
  | Utente | root | `USER app` (non privilegiato) |
  | Rischio | Alto | Basso |

  - **2. OTTIMIZZAZIONI AVANZATE**

  | Aspetto | Semplice | Complesso |
  | --- |  --- |  --- |
  | Layer caching | Base | Avanzato (separa restore da build) |
  | Build cross-platform | No | Sì (`--platform=$BUILDPLATFORM`) |
  | Configurabilità | Fissa | Parametrica (`ARG configuration`) |

  - **3. PROCESSO DI BUILD**

  | Fase | Semplice | Complesso |
  | --- |  --- |  --- |
  | Restore | Insieme a tutto | Separato (cache ottimale) |
  | Build | Diretto publish | Build → Publish (2 fasi) |
  | Ottimizzazioni | Base | Avanzate (`UseAppHost=false`) |

  - **4. DIMENSIONE FINALE**

  - **Semplice**: ~200MB (buona)
  - **Complesso**: ~190MB (ottimale grazie a stage separati)

  - **5. MANUTENIBILITÀ**

  - **Semplice**: Più facile da capire
  - **Complesso**: Più modulare, ogni stage ha uno scopo preciso

- **QUANDO USARE QUALE?**

    - **Dockerfile Semplice - Ideale per:**

    - ✅ Progetti di apprendimento
    - ✅ Prototipi veloci
    - ✅ Team piccoli
    - ✅ Applicazioni semplici

    - **Dockerfile Complesso - Ideale per:**

    - ✅ Applicazioni in produzione
    - ✅ Team enterprise
    - ✅ CI/CD avanzate
    - ✅ Massime performance e sicurezza
    - ✅ Build multi-architettura

- **RIASSUNTO BENEFICI DOCKERFILE COMPLESSO**

1. **Sicurezza**: Utente non-root
2. **Performance**: Build cache ottimizzato
3. **Flessibilità**: Parametri configurabili
4. **Efficienza**: Multi-platform support
5. **Modularità**: Stage ben separati
6. **Produzione**: Ottimizzazioni avanzate

## 4. Creazione Rapida di Immagini con .NET SDK (>= .NET 7)

A partire da .NET 7, l'SDK .NET ha introdotto la capacità di creare immagini container direttamente tramite il comando `dotnet publish`, senza la necessità di scrivere manualmente un `Dockerfile` per scenari comuni. Questa funzionalità è stata ulteriormente potenziata in .NET 8. Molti dettagli per questa nuova funzionalità si trovano nei riferimenti:

- [Containerize a .NET app with dotnet publish](https://learn.microsoft.com/en-us/dotnet/core/containers/sdk-publish)
- [dotnet/sdk-container-builds](https://github.com/dotnet/sdk-container-builds)

### 4.1. Il comando `dotnet publish /t:PublishContainer`

Per utilizzare questa funzionalità, il progetto deve avere come target .NET 7 o versioni successive. Il pacchetto `Microsoft.NET.Build.Containers` è responsabile di questa capacità ed è generalmente incluso di default nei progetti .NET 7+. Se non lo fosse, può essere aggiunto manualmente:

```sh
dotnet add package Microsoft.NET.Build.Containers
```

:memo::fire:**Importante:** Il pacchetto `Microsoft.NET.Build.Containers` è già incluso nelle versioni .NET dalla 8+ e **non va incluso** altrimenti si ottiene un errore durante il processo di build.

Per pubblicare l'applicazione direttamente come immagine container, si utilizza il target `PublishContainer` con il comando `dotnet publish`.

#### 4.1.1 Il comando `dotnet publish /t:PublishContainer` con parametri da command line

In questa sezione verranno mostrati alcuni esempi di comandi `dotnet publish` con parametri di configurazione da riga di comando che permetteranno di ottenere direttamente un'immagine Docker nel proprio registro delle immagini locale, oppure direttamente in un registro remoto come Docker Hub oppure Azure Container Registry (ACR).
**Esempio di comando**:

```sh
dotnet publish -c Release --os linux --arch x64 /t:PublishContainer -p:ContainerRepository=mywebapiapp-sdkpublished -p:ContainerImageTag=1.0
```

- `dotnet publish -c Release`: Come prima, pubblica in configurazione Release.

- `--os linux`: Specifica il sistema operativo target per il container (es. `linux`, `win`).

- `--arch x64`: Specifica l'architettura target (es. `x64`, `arm64`).

- `/t:PublishContainer`: Indica a MSBuild di eseguire il target `PublishContainer`.

- `-p:ContainerRepository=mywebapiapp-sdkpublished`: Imposta il nome dell'immagine container risultante.

- `-p:ContainerImageTag=0.1`: Imposta il tag per l'immagine.

L'immagine creata sarà disponibile localmente nel demone Docker, pronta per essere eseguita o per poterne eseguire la push su un registro.

Come esercizio si provi a generare una seconda immagine del progetto `MyWebApiApp`. In questo caso basta eseguire nella cartella del progetto (la cartella che contiene il file .csproj) il comando dotnet publish, specificando i parametri per l'immagine DOcker e il nome che si vuole dare all'immagine:

```sh
dotnet publish -c Release --os linux --arch x64 /t:PublishContainer -p:ContainerRepository=mywebapiapp-sdkpublished -p:ContainerImageTag=1.0
```

Ad esempio, per effettuare la push su Docker Hub dell'immagine precedentemente creata si possono eseguire le istruzioni seguenti (tagging e poi push):

```sh
docker tag mywebapiapp-sdkpublished:1.0 IL_PROPRIO_NOME_UTENTE_DOCKERHUB/mywebapiapp-sdkpublished:1.0
docker push IL_PROPRIO_NOME_UTENTE_DOCKERHUB/mywebapiapp-sdkpublished:1.0
```

#### 4.1.2 Immagini `Chiseled` (ottimizzate) del runtime per .NET

Per specificare un runtime diverso da quello di default per la creazione dell'immagine Docker è possibile utilizzare un'immagine "chiseled" che è ottimizzata per un determinato sistema operativo. Le immagini "chiseled" sono discusse nella [documentazione ufficiale Microsoft](https://learn.microsoft.com/en-us/dotnet/core/docker/container-images). A titolo di esempio, per trovare le immagini "chiseled" per .NET 9.0 si può cercare sulla pagina [dotnet di Docker Hub](https://hub.docker.com/r/microsoft/dotnet) e poi cercare nei "Featured Repos" quello per ["ASP.NET Core Runtime"](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md). Ad esempio per creare da riga di comando una versione "chiseled" dell'immagine dell'applicazione MyWebApiWeb l'istruzione è:

```sh
dotnet publish -c Release --os linux --arch x64 /t:PublishContainer -p:ContainerRepository=mywebapiapp-sdkpublished-chiseled -p:ContainerBaseImage=mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled -p:ContainerImageTag=1.0
```

#### 4.1.3 Configurazione del `dotnet publish` tramite file `.csproj`

Molte delle opzioni di `dotnet publish` possono essere configurate direttamente nel file `.csproj` del progetto per semplificare il comando publish. In alternativa è sempre possibile passare i parametri da command line e sovrascrivere le impostazioni presenti nel file `.csproj`

Aggiungere o modificare il PropertyGroup nel file `MyWebApiApp.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ContainerRepository>mywebapiapp-sdkpublished-csproj-standard</ContainerRepository>
    <ContainerImageTag>1.0.0</ContainerImageTag>

    <!-- <ContainerBaseImage>mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled</ContainerBaseImage> -->
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
  </ItemGroup>
</Project>


```

Con queste proprietà nel `.csproj`, il comando di pubblicazione diventa più semplice:

```sh
dotnet publish -c Release /t:PublishContainer
```

A partire da .NET 8 sono state introdotte immagini base "chiseled" per Ubuntu, che sono ultra-piccole e con una superficie d'attacco minima, ideali per la produzione. La proprietà `ContainerBaseImage` permette di specificarle.

È anche possibile specificare il registro dove pubblicare l'immagine a seguito del comando `dotnet publish`. In questo modo quando si effettua la pubblicazione dell'app (build e creazione dell'immagine locale) viene anche eseguito il tagging con il proprio nome utente sul Container Registry e viene automaticamente fatta una push dell'immagine. In questo caso bisognerebbe specificare alcuni parametri aggiuntivi nella configurazione del progetto, come mostrato di seguito:

##### 4.1.3.1 Pubblicazione dell'immagine generata mediante `dotnet publish` - configurazione nel `.csproj`

1. Configurazione per Docker Hub

    Per pubblicare su Docker Hub, si deve specificare il proprio nome utente Docker Hub nel `ContainerRepository`.

    **Modifiche al `.csproj` per Docker Hub:**

    Aggiungere/modificare queste righe nel `PropertyGroup` del proprio file `.csproj`:

    ```xml
    <ProjectSdk="Microsoft.NET.Sdk.Web">
        <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>

        <!-- <ContainerBaseImage>mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled</ContainerBaseImage> -->
        <ContainerImageTag>1.0.0</ContainerImageTag> 
        <ContainerRegistry>docker.io</ContainerRegistry> 
        <ContainerRepository>IL_PROPRIO_NOME_UTENTE_DOCKERHUB/mywebapiapp-sdkpublished-csproj</ContainerRepository>
        </PropertyGroup>

        <ItemGroup>
            <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0-preview.3.24172.13" />
        </ItemGroup>
    </Project>

    ```

    **Prima di pubblicare su Docker Hub:** Assicurarsi di aver effettuato l'accesso a Docker Hub dal proprio terminale:

    ```sh
    docker login
    ```

    Verranno chiesti il proprio nome utente e la propria password (o un token di accesso).

2. Configurazione per Azure Container Registry (ACR)

    Per pubblicare su ACR, bisogna specificare il nome del server di login del proprio ACR.

    **Modifiche al `.csproj` per Azure Container Registry:**

    Aggiungere/modificare queste righe nel `PropertyGroup` del proprio file `.csproj`:

    ```xml
    <ProjectSdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net9.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings>

        <!-- <ContainerBaseImage>mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled</ContainerBaseImage> -->
        <ContainerImageTag>1.0.0</ContainerImageTag> 
        <ContainerRegistry>NOME_DEL_PROPRIO_ACR.azurecr.io</ContainerRegistry>
        <ContainerRepository>mywebapiapp-sdkpublished-csproj</ContainerRepository>
        </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0-preview.3.24172.13" />
        </ItemGroup>
    </Project>
    ```

    **Prima di pubblicare su Azure Container Registry:** Assicurarsi di aver effettuato l'accesso al proprio ACR. Il modo più comune è tramite Azure CLI:

    1. Se non è stato già fatto, occorre accedere ad Azure mediante azure CLI:

        ```sh
        az login
        ```

    2. Accedere al proprio ACR (questo configurerà Docker con le credenziali necessarie):

        ```sh
        az acr login --name NOME_DEL_PROPRIO_ACR
        ```

    Sostituire `NOME_DEL_PROPRIO_ACR` con il nome del proprio registry.

**Come Usare il comando `dotnet publish` per effettuare il tagging e la push sul registry remoto:**

1. Scegliere la configurazione del registro (Docker Hub o ACR) che si desidera utilizzare e modificare il proprio file `.csproj` di conseguenza.
2. Assicurarsi di aver effettuato il login al registro scelto come descritto sopra.
3. Eseguire il comando di pubblicazione:

    ```sh
    dotnet publish -c Release /t:PublishContainer
    ```

Questo comando costruirà l'immagine con il tagging impostato e la pubblicherà (farà la push) al registro specificato. Se si vuole solo costruire l'immagine localmente senza fare la push, si può commentare o rimuovere la riga `<ContainerRegistry>`. L'immagine verrà comunque taggata secondo le proprietà `ContainerRepository` e `ContainerImageTag` nel proprio Docker locale.

:memo::fire:**Importante**: Quando si specificano `ContainerRegistry` e `ContainerRepository` nel file `.csproj` e si usa il comando `dotnet publish -c Release /t:PublishContainer`, .NET SDK 8+ crea l'immagine e ed effettua direttamente la push al registry remoto **senza** mantenere una copia locale.

Per avere anche una copia locale dell'immagine, ci sono diverse opzioni:

   - Opzione 1: Pubblicare prima localmente, poi effettuare la push

   ```sh
   # Creare l'immagine solo localmente (senza push)
   dotnet publish -c Release /t:PublishContainer /p:ContainerRegistry= /p:ContainerRepository="IL_PROPRIO_NOME_UTENTE/mywebapiapp-sdkpublished-csproj"
   # Poi effettuare manualmente la push
   docker push "IL_PROPRIO_NOME_UTENTE/mywebapiapp-sdkpublished-csproj:1.0.0"
   ```

   - Opzione 2: Usare proprietà MSBuild specifiche

   ```sh
   dotnet publish -c Release /t:PublishContainer /p:PublishContainerToRegistry=false
   ```

   - Opzione 3: Modificare temporaneamente il .csproj

   Commentare le proprietà del registry nel `.csproj`:

   ```xml
   <!-- <ContainerRegistry>docker.io</ContainerRegistry> -->
   <!-- <ContainerRepository>IL_PROPRIO_NOME_UTENTE/mywebapiapp-sdkpublished-csproj</ContainerRepository> -->
   ```

   Poi eseguire:

   ```sh
   dotnet publish -c Release /t:PublishContainer --property ContainerRepository=IL_PROPRIO_NOME_UTENTE/mywebapiapp-sdkpublished-csproj
   ```

   - Opzione 4: Creare due configurazioni separate

   Puoi creare una configurazione condizionale nel `.csproj`:

   ```xml
   <PropertyGroup>
     <ContainerImageTag>1.0.0</ContainerImageTag>
     <ContainerRegistry Condition="'$(PublishToRegistry)' == 'true'">docker.io</ContainerRegistry>
     <ContainerRepository>IL_PROPRIO_NOME_UTENTE/mywebapiapp-sdkpublished-csproj</ContainerRepository>
   </PropertyGroup>
   ```

   Poi usare:

   ```sh
   # Solo locale
   dotnet publish -c Release /t:PublishContainer

   # Locale + push
   dotnet publish -c Release /t:PublishContainer /p:PublishToRegistry=true
   ```

   La **Opzione 4** è probabilmente la più flessibile per uso futuro.

#### 4.1.4 Pubblicazione dell'immagine generata mediante `dotnet publish` con parametri nella command line

Il comando `dotnet publish -c Release /t:PublishContainer` come dimostrato nel paragrafo precedente,  permette anche di taggare e fare la push dell'immagine al registro corretto, dipendentemente dalle proprietà MSBuild definite nel proprio file `.csproj`.

Tuttavia, se si vuole **specificare o sovrascrivere queste proprietà direttamente dalla riga di comando** senza modificare ogni volta il file `.csproj` (molto utile per script di build o pipeline CI/CD), è possibile farlo usando l'opzione `/p:NomeProprieta=Valore`.

Ecco come è possibile usare `dotnet publish` passando le informazioni per Docker Hub o Azure Container Registry tramite la riga di comando. Il proprio file `.csproj` dovrebbe comunque contenere le configurazioni di base come `<TargetFramework>`, `<ContainerBaseImage>`, ecc.

**File `.csproj` di Base (Potrebbe omettere le proprietà specifiche del registro se sono passate sempre da riga di comando):**

XML

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- <ContainerRepository>mywebapiapp-sdkpublished-csproj-standard</ContainerRepository>
    <ContainerImageTag>1.0.0</ContainerImageTag> -->

    <!-- <ContainerBaseImage>mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled</ContainerBaseImage> -->
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
  </ItemGroup>
</Project>
```

1. Pubblicare su Docker Hub tramite Riga di Comando

    Assicurarsi di aver eseguito `docker login` prima.

    ```sh
        # Nella Bash
        dotnet publish -c Release /t:PublishContainer \
        /p:ContainerRegistry=docker.io \
        /p:ContainerRepository="IL_PROPRIO_NOME_UTENTE_DOCKERHUB/mywebapiapp-sdkpublished-csproj" \
        /p:ContainerImageTag="1.0.0"
    ```

    ```ps1
        # In PowerShell
        dotnet publish -c Release /t:PublishContainer `
        /p:ContainerRegistry=docker.io `
        /p:ContainerRepository="IL_PROPRIO_NOME_UTENTE_DOCKERHUB/mywebapiapp-sdkpublished-csproj" `
        /p:ContainerImageTag="1.0.0"
    ```

    **Note:**

    - Sostituire `IL_PROPRIO_NOME_UTENTE_DOCKERHUB` con il proprio nome utente Docker Hub.
    - Sono stati messi tra virgolette i valori con `/` o che potrebbero contenere caratteri speciali, per sicurezza a seconda della shell in uso.
    - `ContainerImageTag` può essere quello che si preferisce (es. `1.0.0`, `latest`, la versione del proprio progetto).

2. Pubblicare su Azure Container Registry (ACR) tramite Riga di Comando

    Assicurarsi di aver eseguito `az acr login --name NOME_DEL_PROPRIO_ACR` prima.

    ```sh
        # Nella Bash
        dotnet publish -c Release /t:PublishContainer \
        /p:ContainerRegistry="NOME_DEL_PROPRIO_ACR.azurecr.io" \
        /p:ContainerRepository="mywebapiapp-sdkpublished-csproj" \
        /p:ContainerImageTag="1.0.0"
    ```

    ```ps1
        # In PowerShell
        dotnet publish -c Release /t:PublishContainer `
        /p:ContainerRegistry="NOME_DEL_PROPRIO_ACR.azurecr.io" `
        /p:ContainerRepository="mywebapiapp-sdkpublished-csproj" `
        /p:ContainerImageTag="1.0.0"
    ```

    **Note:**

    - Sostituire `NOME_DEL_PROPRIO_ACR` con il nome del server di login del proprio Azure Container Registry.
    - Il `ContainerRepository` per ACR può essere più semplice, o includere percorsi come `myapps/mywebapiapp-sdkpublished-csproj`.

**Come Funziona:**

- Il comando `dotnet publish -c Release /t:PublishContainer` è sempre lo stesso.
- Usando `/p:NomeProprieta=Valore`, stai dicendo a MSBuild (il sistema di build usato da `dotnet`) di usare questi valori per le proprietà specificate. **Se queste proprietà esistono anche nel file `.csproj`, quelle passate da riga di comando generalmente hanno la precedenza**.
- Questo approccio dà la flessibilità di decidere dove pubblicare e con quale nome/tag al momento dell'esecuzione del comando, senza dover modificare il file `.csproj` ogni volta.

**Quando usare le proprietà nel `.csproj` vs. Riga di Comando:**

- **Nel `.csproj`**: Utile per impostazioni predefinite, o se si applicano sempre nello stesso posto con la stessa configurazione di base.
- **Riga di comando (`/p:`)**: Ottimo per script, pipeline di CI/CD, o quando si ha bisogno di cambiare frequentemente i parametri di pubblicazione (es. pubblicare su diversi registri, usare tag dinamici).

Ricordare che l'autenticazione (`docker login`, `az acr login`) è un prerequisito separato che configura il proprio ambiente Docker per avere i permessi di fare il push ai registri remoti.

### 4.2. Vantaggi, limiti e scenari d'uso

**Vantaggi**:

- **Semplicità**: Non è necessario scrivere e mantenere un `Dockerfile` per molti scenari comuni.

- **Integrazione con il Build System .NET**: Il processo è completamente integrato con MSBuild e il comando `dotnet publish`.

- **Default Sensati e Best Practice Integrate**:

    - Utilizza immagini base ufficiali Microsoft.

    - Configura automaticamente un utente non-root (`app`) per eseguire l'applicazione nel container, migliorando la sicurezza.

    - Imposta la porta di default (es. 8080 a partire da .NET 8).

    - Produce immagini ottimizzate (simili a quelle di un `Dockerfile` multi-stage ben scritto).

- **Velocità per build semplici**: Può essere più rapido per progetti .NET standard senza personalizzazioni complesse dell'ambiente container.

- **Portabilità del processo di build**: Non dipende dalla presenza di un client Docker per *costruire* l'immagine (anche se Docker è necessario per *eseguirla* o per alcune operazioni di push).

**Limiti**:

- **Minore Flessibilità e Controllo**: Offre meno controllo granulare rispetto a un `Dockerfile` scritto manualmente. Operazioni complesse come l'installazione di dipendenze di sistema operativo aggiuntive (es. `apt-get install ...`), la configurazione di più fasi di build personalizzate, o l'esecuzione di script complessi all'interno dell'immagine sono più difficili o impossibili da realizzare direttamente.

- **Personalizzazione Avanzata**: Sebbene sia possibile personalizzare molti aspetti tramite proprietà MSBuild (es. `ContainerBaseImage`, `ContainerPort`, `ContainerUser`, `ContainerWorkingDirectory`, `ContainerLabel`), configurazioni molto specifiche o complesse possono diventare macchinose da esprimere tramite proprietà MSBuild rispetto alla chiarezza di un `Dockerfile`.

- **Dipendenze di Sistema Complesse**: Se l'applicazione necessita di librerie native o strumenti che non sono inclusi nell'immagine base di .NET (es. `libgdiplus` per System.Drawing, `ffmpeg`, ecc.), un `Dockerfile` è l'approccio più indicato per gestire queste installazioni.

**Scenari d'uso Ideali**:

- Applicazioni ASP.NET Core standard, worker services, o console apps che non richiedono personalizzazioni estese dell'ambiente container.

- Prototipazione rapida e sviluppo, dove la velocità di iterazione è prioritaria.

- Pipeline CI/CD dove la semplicità della configurazione di build è un vantaggio e i requisiti dell'immagine sono standard.

- Sviluppatori o team che preferiscono rimanere all'interno dell'ecosistema di strumenti .NET il più possibile.

### 4.3. Confronto con l'approccio basato su `Dockerfile` tradizionale

| **Caratteristica** | **dotnet publish /t:PublishContainer** | **Dockerfile Tradizionale** |
| --- |  --- |  --- |
| **Complessità di Scrittura** | Bassa (nessun file `Dockerfile` richiesto) | Media/Alta (richiede conoscenza della sintassi Dockerfile) |
| **Flessibilità/Controllo** | Limitato (configurazione tramite proprietà MSBuild) | Alto (controllo completo su ogni layer e comando) |
| **Curva di Apprendimento** | Bassa (si basa su comandi `dotnet` noti) | Più alta (necessaria comprensione dei concetti Docker) |
| **Manutenzione** | Semplice per casi standard; più complesso per personalizzazioni | Richiede manutenzione del `Dockerfile` |
| **Dipendenze OS Aggiuntive** | Difficile/Impossibile da gestire direttamente | Facile da gestire con comandi `RUN` (es. `apt-get install`) |
| **Multi-Stage Build** | Implementato implicitamente dal tool per ottimizzare l'immagine .NET | Configurazione esplicita e completamente personalizzabile |
| **Best Practice Integrate** | Molte (utente non-root, immagini base ottimizzate) | L'utente è responsabile dell'implementazione delle best practice |
| **Ecosistema** | Fortemente integrato con .NET e MSBuild | Standard Docker, agnostico rispetto al linguaggio/framework |

**Quando scegliere quale approccio**:

- **`dotnet publish /t:PublishContainer`**:

    - Per la maggior parte delle applicazioni ASP.NET Core che non hanno requisiti esotici per l'ambiente container.

    - Quando si desidera la massima semplicità e integrazione con gli strumenti .NET.

    - Ottimo per iniziare rapidamente con la containerizzazione.

- **`Dockerfile` tradizionale**:

    - Quando è necessario un controllo completo e granulare sul processo di creazione dell'immagine.

    - Se l'applicazione richiede l'installazione di pacchetti o librerie a livello di sistema operativo.

    - Per build multi-stage complesse che coinvolgono più tecnologie o passaggi non .NET.

    - Quando si lavora in team con competenze Docker consolidate o si necessita di allineamento con pratiche Docker preesistenti.

    - Per sfruttare funzionalità avanzate di Docker non direttamente esposte dalle proprietà MSBuild.

Per il resto di questo tutorial, si continuerà a utilizzare l'approccio basato su `Dockerfile` tradizionale per la sua maggiore versatilità didattica e per illustrare concetti che sono più espliciti e controllabili in questo modo. Tuttavia, è importante essere consapevoli dell'opzione `/t:PublishContainer` come valida alternativa moderna.

## 5. Gestione dei Container Docker

Una volta che un'immagine Docker è stata costruita (o scaricata da un registro), il passo successivo è eseguirla per creare un container.

### 5.1. Avvio di un container: `docker run`

Il comando `docker run` è il comando fondamentale per creare ed avviare un nuovo container da un'immagine specificata.

Utilizzando l'immagine `mywebapiapp-image:1.0` costruita in precedenza:

```sh
docker run --name mywebapiapp-container-run mywebapiapp-image:1.0
```

- `docker run`: Comando per avviare un container.

- `--name mywebapiapp-container-run`: Assegna un nome specifico (`mywebapiapp-container-run`) al container in esecuzione. Questo è opzionale; se omesso, Docker assegnerà un nome generato casualmente (es. `adoring_goldstine`). Avere un nome facilita il riferimento al container in comandi successivi (`docker stop`, `docker logs`, ecc.).

- `mywebapiapp-image:1.0`: Specifica l'immagine e il tag da cui creare il container.

Eseguendo questo comando, l'applicazione ASP.NET Core all'interno del container si avvierà. I log dell'applicazione (output di console) verranno visualizzati direttamente nel terminale corrente. L'applicazione è in esecuzione, ma non è ancora accessibile dall'esterno del sistema Docker (dall'host o da altre macchine) perché le sue porte non sono state "pubblicate".

Per fermare il container (se è in esecuzione in foreground, come in questo caso), premere `Ctrl+C` nel terminale. Il container verrà fermato.

Per rimuovere un container fermato (liberando il nome e le risorse):

```sh
docker rm mywebapiapp-container-run
```

Se si tenta di riutilizzare un nome di container già esistente (anche se fermato) con `docker run --name ...`, si otterrà un errore. È necessario prima rimuovere il container esistente con quel nome.

### 5.2. Port Mapping: Esporre l'applicazione (`-p`)

Per rendere l'applicazione web accessibile dal browser sull'host o da altre macchine sulla rete, è necessario mappare una porta del sistema host a una porta del container su cui l'applicazione è in ascolto.

Nel Dockerfile per `MyWebApiApp`, si è usato `EXPOSE 8080` e `ENV ASPNETCORE_URLS=http://+:8080`. Questo significa che l'applicazione ASP.NET Core all'interno del container ascolta sulla porta 8080.

Per mappare, ad esempio, la porta `8081` dell'host alla porta `8080` del container:

```sh
docker run --name mywebapiapp-container-run -p 8081:8080 mywebapiapp-image:1.0
```

- `-p 8081:8080` (o `--publish 8081:8080`): Mappa la porta.

    - La sintassi è `<host_port>:<container_port>`.

    - Il traffico inviato alla porta `8081` della macchina host verrà inoltrato alla porta `8080` del container `mywebapiapp-container-run`.

Ora, aprendo un browser web e navigando a `http://localhost:8081`, si dovrebbe vedere la pagina `index.html` servita dall'applicazione `MyWebApiApp` in esecuzione nel container.

Se si omette la porta host (es. -p 8080), Docker sceglierà una porta host libera casuale e la mapperà alla porta 8080 del container. Si può vedere quale porta è stata scelta con docker ps.

Se si usa `-P` (P maiuscola), Docker pubblicherà tutte le porte esposte (`EXPOSE`) nel Dockerfile a porte host casuali.

### 5.3. Networking in Docker

I container Docker, per impostazione predefinita, sono isolati ma possono comunicare attraverso le reti Docker.

#### 5.3.1. La rete bridge di default (`bridge`)

Quando Docker viene installato, crea una rete virtuale predefinita chiamata `bridge`. Se non si specifica una rete diversa quando si esegue un container, esso viene connesso a questa rete `bridge` di default.

- Ogni container connesso alla rete `bridge` ottiene un indirizzo IP interno (es. `172.17.0.x`).

- I container sulla stessa rete `bridge` di default *possono* comunicare tra loro usando questi indirizzi IP interni. Tuttavia, questi IP possono cambiare se i container vengono fermati e riavviati, rendendo questo metodo poco affidabile.

- :memo::fire:**Importante**: La rete `bridge` di default **non fornisce una risoluzione DNS automatica basata sui nomi dei container**. Questo significa che un container `app` non può semplicemente contattare un container `db` usando l'hostname `db`.

#### 5.3.2. Creazione e utilizzo di reti personalizzate (`docker network create`)

Per una comunicazione affidabile e basata su nomi tra container, è una **best practice** creare e utilizzare reti bridge personalizzate (user-defined bridge networks).

**Vantaggi delle reti personalizzate**:

- **Migliore Isolamento**: I container connessi a reti personalizzate diverse sono isolati l'uno dall'altro per default. Solo i container sulla stessa rete personalizzata possono comunicare facilmente.

- **Risoluzione DNS Automatica**: Docker fornisce una risoluzione DNS automatica per i container sulla stessa rete personalizzata. Un container può raggiungere un altro container usando il nome del container come hostname. Questo è cruciale per applicazioni multi-container (es. un'applicazione web che deve connettersi a un container database).

- **Configurazione di Rete più Flessibile**: Le reti personalizzate offrono più opzioni di configurazione.

1. **Creare una rete personalizzata**:

    ```sh
    docker network create myapp-network
    ```

    Questo comando crea una nuova rete di tipo bridge (il default) chiamata myapp-network.

    Si può ispezionare la rete con `docker network inspect myapp-network`.

2. Avviare container su questa rete:

    Quando si avvia un container con `docker run`, si può specificare a quale rete connetterlo usando l'opzione `--network`.

    ```sh
    # Esempio concettuale, non ancora per MyWebApiApp e MariaDB
    # docker run --name some-service --network myapp-network some-image
    ```

    Vedremo come applicare questo concetto per connettere `MyWebApiApp` a un database MariaDB.

### 5.4. Esecuzione di un container Database (MariaDB)

Si eseguirà ora un'istanza di MariaDB all'interno di un container Docker. MariaDB è un sistema di gestione di database relazionali open source, fork di MySQL.

1. Scaricare l'immagine MariaDB (se non già presente localmente):

    Docker scaricherà automaticamente l'immagine se non la trova localmente quando si esegue docker run, ma è buona pratica scaricarla esplicitamente o specificare una versione.

    ```sh
    docker pull mariadb:11.4 # Si consiglia di usare un tag di versione specifico anziché 'latest' per la produzione
    ```

    (Sostituire `11.4` con la versione desiderata o usare `latest` per l'ultima stabile). L'elenco delle versioni disponibili è riportato nella [pagina di Docker Hub di MariaDB](https://hub.docker.com/_/mariadb). Al momento in cui si scrivono queste note, la versione `11.4` corrisponde alla `lts` (Long Term Support).

2. Avviare il container MariaDB:

    Si avvierà il container MariaDB con alcune configurazioni di base passate tramite variabili d'ambiente e lo si connetterà alla rete myapp-network.

    ```sh
    # Versione del comando per la Bash
    # Se si usa Powershell il comando è uguale, solo che il carattere per continuare il comando sulla riga successiva è ` e non \
    docker run --name mariadb-container \
        -e MARIADB_ROOT_PASSWORD=mySuperSecretPassword123 \
        -e MARIADB_DATABASE=mywebapiappdb \
        -e MARIADB_USER=mywebapiappuser \
        -e MARIADB_PASSWORD=userPassword456 \
        -p 3306:3306 \
        --network myapp-network \
        -d \
        mariadb:11.4
    ```

    Spiegazione dei parametri:

    - `--name mariadb-container`: Assegna un nome al container del database.

    - `-e <NOME_VARIABILE>=<VALORE>`: Imposta variabili d'ambiente all'interno del container. L'immagine MariaDB ufficiale utilizza queste variabili per la configurazione iniziale:

        - `MARIADB_ROOT_PASSWORD=mySuperSecretPassword123`: Imposta la password per l'utente `root` di MariaDB. **Importante**: Questa è una password segreta. Per lo sviluppo può andare bene così, ma per la produzione i segreti devono essere gestiti in modo più sicuro (vedere la sezione dedicata più avanti).

        - `MARIADB_DATABASE=mywebapiappdb`: Crea automaticamente un database chiamato `mywebapiappdb` al primo avvio del container.

        - `MARIADB_USER=mywebapiappuser`: Crea un nuovo utente chiamato `mywebapiappuser`.

        - `MARIADB_PASSWORD=userPassword456`: Imposta la password per l'utente `mywebapiappuser`. Questo utente avrà pieni permessi sul database `mywebapiappdb`.

    - `-p 3306:3306`: Mappa la porta `3306` della macchina host alla porta `3306` del container. La porta `3306` è la porta standard su cui MariaDB/MySQL ascolta. Mappandola all'host, ci si può connettere al database da strumenti sulla macchina host (es. DBeaver, MySQL Workbench) usando `localhost:3306`.

    - `--network myapp-network`: Connette il container `mariadb-container` alla rete personalizzata `myapp-network` creata in precedenza.

    - `-d` (detached mode): Esegue il container in background (scollegato dal terminale corrente) e stampa l'ID del container. Senza `-d`, il terminale mostrerebbe i log di MariaDB e rimarrebbe bloccato.

    - `mariadb:11.4`: L'immagine MariaDB da utilizzare.

    Dopo aver eseguito il comando, si può verificare che il container sia in esecuzione:

    ```sh
    docker ps
    ```

    Dovrebbe mostrare `mariadb-container` tra i container attivi.

    Per visualizzare i log del container MariaDB (utili per il troubleshooting):

    ```sh
    docker logs mariadb-container
    ```

### 5.5. Collegamento di Container: Connessione App <-> Database sulla stessa rete

Ora che sia l'applicazione `MyWebApiApp` che il database MariaDB possono essere eseguiti e connessi alla stessa rete personalizzata (`myapp-network`), l'applicazione `MyWebApiApp` (quando eseguita anch'essa in un container su tale rete) può connettersi al database MariaDB utilizzando il nome del container del database (`mariadb-container`) come hostname. Questo è possibile grazie alla risoluzione DNS fornita dalle reti Docker personalizzate.

Tuttavia, è fondamentale distinguere due scenari di connessione:

1. L'applicazione `MyWebApiApp` è in esecuzione **direttamente sull'host** (es. durante lo sviluppo con `dotnet run`) e si connette al database MariaDB in esecuzione in un container.
2. L'applicazione `MyWebApiApp` è in esecuzione **all'interno di un container Docker**, sulla stessa rete Docker del container del database.

Questi due scenari richiedono stringhe di connessione diverse.

1. **Configurare le Stringhe di Connessione per Diversi Ambienti**: Per gestire entrambi gli scenari, si utilizzeranno i file di configurazione `appsettings.json` e `appsettings.Development.json`. ASP.NET Core carica questi file in modo gerarchico: `appsettings.json` viene caricato per primo, seguito da `appsettings.{Environment}.json` (dove `{Environment}` è, ad esempio, `Development`), che può sovrascrivere le impostazioni del primo.

    a. **`MyWebApiApp/appsettings.Development.json` (per esecuzione sull'host durante lo sviluppo)**: Questo file verrà utilizzato quando si esegue l'applicazione direttamente sull'host (es. con `dotnet run`, che di default imposta `ASPNETCORE_ENVIRONMENT=Development`). Conterrà la stringa di connessione per accedere al container MariaDB dall'host, utilizzando `localhost` e la porta mappata sull'host dal container MariaDB (es. `3306`).

    ```json
    // MyWebApiApp/appsettings.Development.json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Port=3306;Database=mywebapiappdb;Uid=mywebapiappuser;Pwd=userPassword456;AllowPublicKeyRetrieval=true"
        // NOTA: La 'Port' (qui 3306) deve corrispondere alla porta host mappata
        // nel comando 'docker run' per il container MariaDB (es. -p 3306:3306).
        // Le credenziali (Uid, Pwd, Database) devono corrispondere a quelle usate
        // per avviare il container MariaDB.
      }
    }
    ```

    b. **`MyWebApiApp/appsettings.json` (configurazione di default, usata nel container)**: Questo file conterrà la stringa di connessione che l'applicazione utilizzerà quando è in esecuzione all'interno di un container Docker, connettendosi al servizio `mariadb-container` sulla porta interna `3306`.

    ```json
    // MyWebApiApp/appsettings.json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
        "DefaultConnection": "Server=mariadb-container;Port=3306;Database=mywebapiappdb;Uid=mywebapiappuser;Pwd=userPassword456;AllowPublicKeyRetrieval=true"
        // NOTA: 'Server=mariadb-container' usa il nome del servizio Docker.
        // 'Port=3306' è la porta interna di MariaDB nel suo container.
      }
    }
    ```

    **Analisi della stringa di connessione (comune a entrambe, cambiano Server e Porta per l'accesso)**:

    - `Server`: `localhost` (da host) o `mariadb-container` (da container app).
    - `Port`: `3306` (porta mappata sull'host, esempio) o `3306` (porta interna del container DB).
    - `Database=mywebapiappdb`: Il nome del database.
    - `Uid=mywebapiappuser`: L'utente per la connessione.
    - `Pwd=userPassword456`: La password per l'utente.
    - `AllowPublicKeyRetrieval=true`: Parametro spesso necessario per il provider EF Core per MariaDB/MySQL (`Pomelo.EntityFrameworkCore.MySql`) se SSL non è configurato in modo restrittivo.
2. **Aggiungere i pacchetti NuGet per Entity Framework Core e il provider MariaDB/MySQL a `MyWebApiApp`**: Per utilizzare EF Core con MariaDB, è necessario il pacchetto EF Core principale, gli strumenti e un provider di database specifico. `Pomelo.EntityFrameworkCore.MySql` è un provider popolare e ben mantenuto.

    Aprire un terminale nella directory `MyWebApiApp/` ed eseguire i seguenti comandi:

    Bash

    ```sh
    dotnet add package Microsoft.EntityFrameworkCore
    dotnet add Microsoft.EntityFrameworkCore.Design
    dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.0.0-preview.3.efcore.9.0.0
    ```

    Questo aggiungerà i riferimenti ai pacchetti nel file `MyWebApiApp.csproj`.

3. Definire un `Entity Model` e un `DbContext`:

    EF Core lavora con un modello di dati definito tramite classi C# (entities) e un contesto di database (`DbContext`).

    a. Creare una classe Entity (es. `TestEntry.cs`) nella directory `Models` del progetto `MyWebApiApp`:

    ```cs
    using System.ComponentModel.DataAnnotations;

    namespace MyWebApiApp.Models;
    public class TestEntry
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Message { get; set; }=null!;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    ```

    b. Creare una classe `DbContext` (es. `AppDbContext.cs`) nella directory `Data` del progetto `MyWebApiApp`:

    ```cs
    // MyWebApiApp/Data/AppDbContext.cs

    using Microsoft.EntityFrameworkCore;
    using MyWebApiApp.Models;

    namespace MyWebApiApp.Data;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntry> TestEntries { get; set; }

        // Opzionale: si può ulteriormente configurare il modello qui se necessario
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        //     // Esempio: modelBuilder.Entity<TestEntry>().ToTable("CustomTestEntries");
        // }
    }
    ```

4. Configurare EF Core in `Program.cs` e modificare l'endpoint `/dbtest`:

    - Registrare il `DbContext` nel sistema di `Dependency Injection` di ASP.NET Core e aggiornare l'endpoint `/dbtest` per utilizzare EF Core per interagire con il database.

    - Per inizializzare il database si utilizzeranno le migrations in combinazione con un metodo che `DbInitializer.Initialize` che applica le migrazioni e crea il database se questo non è stato ancora creato. Nella cartella `Data` si aggiunga il file `DbInitializer.cs` con la casse definita di seguito:

      ```cs
      // MyWebApiApp/Data/DbInitializer.cs
      using Microsoft.EntityFrameworkCore;
      using MyWebApiApp.Models;

      namespace MyWebApiApp.Data;

      public static class DbInitializer
      {
          public static async Task Initialize(AppDbContext context)
          {
              // Applica le migrazioni al database
              await context.Database.MigrateAsync();

              // Aggiungi dati di esempio se la tabella è vuota
              if (!await context.TestEntries.AnyAsync())
              {
                  var sampleEntry = new TestEntry
                  {
                      Message = $"Primo record di esempio - {DateTime.UtcNow}"
                  };

                  context.TestEntries.Add(sampleEntry);
                  await context.SaveChangesAsync();
              }
          }
      }
      ```

    - Dopo aver creato il modello dei dati e il `DBContext` e aver verificato che il container del database di MariaDB è in esecuzione, si dovrà eseguire, nella shell posizionata sulla cartella del progetto (dove si trova il file `.csproj`), il seguente comando:

      ```sh
      dotnet ef migrations add InitialCreate
      ```

    - Il file `Program.cs` dovrà essere aggiornato come segue:

      ```cs
      // MyWebApiApp/Program.cs
      using Microsoft.EntityFrameworkCore;
      using MyWebApiApp.Data;
      using MyWebApiApp.Models;
      using System.Text;

      var builder = WebApplication.CreateBuilder(args);

      // 1. Leggi la stringa di connessione da appsettings.json
      var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

      // 2. Aggiungi AppDbContext ai servizi, configurandolo per usare MariaDB/MySQL
      builder.Services.AddDbContext<AppDbContext>(options =>
          options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
      );

      var app = builder.Build();

      // Inizializza il database utilizzando le migrazioni
      using (var scope = app.Services.CreateScope())
      {
          var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
          await DbInitializer.Initialize(context);
      }

      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.MapGet("/hello", () =>
      {
          return Results.Ok(new { Message = "Ciao dal backend ASP.NET Core Minimal API!", Timestamp = DateTime.UtcNow });
      });

      app.MapGet("/dbtest", async (AppDbContext dbContext, IConfiguration config) =>
      {
          var currentConnectionString = config.GetConnectionString("DefaultConnection");
          var sb = new StringBuilder();
          sb.AppendLine("Risultato Test Connessione Database MariaDB con Entity Framework Core:");
          sb.AppendLine($"Stringa di connessione usata: {currentConnectionString}");

          if (string.IsNullOrEmpty(currentConnectionString))
          {
              sb.AppendLine("ERRORE: Stringa di connessione 'DefaultConnection' non trovata o vuota.");
              return Results.Text(sb.ToString());
          }

          try
          {
              // Testa la connessione al database (il database è già stato inizializzato al startup)
              sb.AppendLine("Testando la connessione al database...");

              // Tenta di inserire un nuovo record
              var newEntry = new TestEntry { Message = $"Test EF Core - {DateTime.UtcNow}" };
              dbContext.TestEntries.Add(newEntry);
              await dbContext.SaveChangesAsync();
              sb.AppendLine($"SUCCESSO: Record inserito con ID: {newEntry.Id}");

              // Tenta di leggere il record appena inserito (o l'ultimo)
              var retrievedEntry = await dbContext.TestEntries
                                          .OrderByDescending(e => e.Timestamp)
                                          .FirstOrDefaultAsync();

              if (retrievedEntry != null)
              {
                  sb.AppendLine($"SUCCESSO: Record letto: ID={retrievedEntry.Id}, Messaggio='{retrievedEntry.Message}', Timestamp='{retrievedEntry.Timestamp}'");
              }
              else
              {
                  sb.AppendLine("ATTENZIONE: Nessun record trovato dopo l'inserimento.");
              }

              // Tenta una query LINQ per contare i record
              int count = await dbContext.TestEntries.CountAsync();
              sb.AppendLine($"SUCCESSO: Ci sono {count} voci nella tabella 'TestEntries'.");

          }
          catch (Exception ex) // Cattura eccezioni generiche (MySqlException è inclusa)
          {
              sb.AppendLine($"ERRORE durante l'interazione con il database: {ex.GetType().Name} - {ex.Message}");
              sb.AppendLine("Stack Trace Parziale:");
              sb.AppendLine(ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 500)) + "..."); // Mostra solo una parte per brevità
              sb.AppendLine("\nControllare:");
              sb.AppendLine("1. Che il container 'mariadb-container' sia in esecuzione e sulla stessa rete Docker.");
              sb.AppendLine("2. Che il nome del server ('mariadb-container') e le credenziali nella connection string siano corretti.");
              sb.AppendLine("3. Che il database specificato esista e l'utente abbia i permessi.");
              sb.AppendLine("4. Che il provider Pomelo.EntityFrameworkCore.MySql sia configurato correttamente.");
          }
          return Results.Text(sb.ToString());
      });

      app.Run();
      ```

5. **Ricostruire l'immagine dell'applicazione `MyWebApiApp`**: Dopo aver modificato `appsettings.json`, `appsettings.Development.json`, `Program.cs`, `.csproj` e aggiunto i file C# per EF Core, è necessario ricostruire l'immagine Docker. Nella directory `MyWebApiApp/`:

    ```sh
    docker build -t mywebapiapp-image:1.1 .
    ```

6. **Avviare il container MariaDB (se non già in esecuzione) con mappatura delle porte**: Per poter testare dall'host, è cruciale mappare la porta del container MariaDB a una porta dell'host.

    ```sh
    docker run --name mariadb-container \
        -e MARIADB_ROOT_PASSWORD=mysecretpassword \
        -e MARIADB_DATABASE=mywebapiappdb \
        -e MARIADB_USER=mywebapiappuser \
        -e MARIADB_PASSWORD=userPassword456 \
        -p 3306:3306 \
        --network myapp-network \
        -d \
        mariadb:11.4 # Usare le proprie credenziali e versione
    ```

    **Nota**: `-p 3306:3306` mappa la porta 3306 interna del container MariaDB alla porta `3306` dell'host. Questo è il motivo per cui `appsettings.Development.json` usa `Port=3306`.

7. **Testare la connessione**:

    a. **Test dall'host (Applicazione `MyWebApiApp` in esecuzione sull'host, Database in container)**:
     * Assicurarsi che il container `mariadb-container` sia in esecuzione con la porta mappata come sopra.
     * Nella directory del progetto `MyWebApiApp` sull'host, eseguire: `bash dotnet run` L'applicazione ASP.NET Core partirà sull'host (es. su `http://localhost:5XXX`). `ASPNETCORE_ENVIRONMENT` sarà `Development`, quindi verrà usata la stringa di connessione da `appsettings.Development.json` (`Server=localhost;Port=3306;...`).
     * Aprire un browser e navigare all'URL dell'applicazione host seguito da `/dbtest` (es. `http://localhost:5239/dbtest`, controllare la porta esatta dall'output di `dotnet run`).
     * Si dovrebbe vedere un messaggio di successo che indica l'interazione con il database tramite EF Core.

    b. **Test da container (Applicazione `MyWebApiApp` in container, Database in container)**:
    * Assicurarsi che il container `mariadb-container` sia in esecuzione sulla rete `myapp-network`.
    * Fermare e rimuovere qualsiasi istanza precedente del container dell'applicazione:

        ```sh
       docker stop mywebapiapp-container-run 
       docker rm mywebapiapp-container-run
        ```

    * Avviare il container dell'applicazione `MyWebApiApp` (usando l'immagine `mywebapiapp-image:1.1`) sulla stessa rete, mappando una porta host per l'accesso (es. `8081` sull'host alla `8080` interna del container):

      ```sh
      docker run --name mywebapiapp-container-run \
      -p 8081:8080 \
      --network myapp-network \
      -d \
      mywebapiapp-image:1.1 
      ```

     All'interno di questo container, `ASPNETCORE_ENVIRONMENT` potrebbe non essere `Development` (a meno che non sia impostato esplicitamente nel `Dockerfile` o con `-e`). Se è, ad esempio, `Production` (o non impostato, il che potrebbe portare a default diversi), e se `appsettings.Development.json` non è copiato nell'immagine, verrà usata la stringa di connessione da `appsettings.json` (`Server=mariadb-container;Port=3306;...`).

    * Aprire un browser e navigare all'URL `http://localhost:8081/dbtest`.
    * Anche qui, si dovrebbe vedere un messaggio di successo.

    **Troubleshooting**: In caso di errore in uno dei due scenari, controllare attentamente:

    - I log del container dell'applicazione: `docker logs mywebapiapp-container-run`
    - I log del container del database: `docker logs mariadb-container`
    - La stringa di connessione attiva (stampata dall'endpoint `/dbtest`).
    - Le mappature delle porte e la configurazione della rete Docker.
    - Le credenziali e i nomi del database/server.

Questo approccio con configurazioni separate per lo sviluppo sull'host e l'esecuzione containerizzata offre flessibilità. Docker Compose (Sezione 7) aiuterà a gestire le variabili d'ambiente e la configurazione dei servizi in modo più strutturato, specialmente per l'ambiente containerizzato.

## 6. Gestione Avanzata della Configurazione e dei Segreti 🔒

La gestione sicura ed efficiente della configurazione, specialmente dei dati sensibili (segreti) come password di database, API key, e certificati, è un aspetto critico nello sviluppo e nel deployment di applicazioni.

### 6.1. La sfida delle variabili d'ambiente multiple (vs. User Secrets in sviluppo)

Durante lo sviluppo locale *senza* Docker, ASP.NET Core offre un meccanismo chiamato **User Secrets**. Questo strumento permette di memorizzare dati di configurazione sensibili (come stringhe di connessione con password, API key per servizi esterni) in un file JSON (`secrets.json`) che risiede al di fuori della directory del progetto, tipicamente nel profilo utente dello sviluppatore (es. `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json` su Windows). Questo è molto utile perché impedisce che i segreti vengano accidentalmente committati nel repository Git.

Quando si passa alla containerizzazione con Docker, il meccanismo standard di User Secrets non è direttamente applicabile all'interno del container in esecuzione, poiché il filesystem del container è isolato e non ha accesso al profilo utente dell'host.

Il metodo più comune per passare configurazioni ai container Docker è tramite variabili d'ambiente. Si è già visto questo approccio con -e nel comando docker run per configurare il container MariaDB.

Tuttavia, per un'applicazione con molte impostazioni di configurazione, specificarle tutte sulla riga di comando docker run diventa:

- **Scomodo e Lungo**: La riga di comando può diventare eccessivamente lunga.

- **Soggetto a Errori**: È facile commettere errori di battitura o dimenticare una variabile.

- **Poco Pratico per i Segreti**: Anche se le variabili d'ambiente sono un modo standard, visualizzare segreti direttamente nella riga di comando o negli script di avvio può essere un rischio se questi log o script vengono esposti.

### 6.2. Utilizzo di file `.env` con Docker Compose

Docker Compose (che verrà introdotto in dettaglio nella Sezione 7) offre un modo molto più elegante e gestibile per fornire variabili d'ambiente ai servizi, inclusa la possibilità di utilizzare file `.env`.

Un file `.env` (che sta per "environment") è un semplice file di testo che definisce le variabili d'ambiente, una per riga, nel formato `NOME_VARIABILE=VALORE`. I commenti possono essere aggiunti usando `#`.

Esempio di file .env:

Creare un file chiamato .env nella directory principale del progetto (la stessa directory dove si troverà il file docker-compose.yml).

```env
# MyProjectRoot/.env

# Configurazione Database MariaDB
MARIADB_ROOT_PASSWORD_VAL=myRootSecretFromEnvFile!
MARIADB_DATABASE_VAL=appdb_from_env
MARIADB_USER_VAL=appuser_env
MARIADB_PASSWORD_VAL=appUserPassFromEnvFile!

# Configurazione Applicazione WebApp
WEBAPP_CONTAINER_PORT_VAL=8080
WEBAPP_HOST_PORT_MAPPING_VAL=8088
ASPNETCORE_ENVIRONMENT_NAME=Development

```

#### 6.2.1. Sostituzione di variabili nel `docker-compose.yml` (`${NOME_VARIABILE}`)

Quando si esegue `docker-compose up`, Docker Compose cerca automaticamente un file chiamato `.env` nella directory da cui viene eseguito (o nella directory del progetto specificata). Le variabili definite in questo file `.env` possono essere utilizzate per la **sostituzione di variabili** all'interno del file `docker-compose.yml`.

**Esempio di utilizzo in `docker-compose.yml`**:

```yml
# Esempio parziale di docker-compose.yml
version: '3.8'
services:
  db:
    image: mariadb:10.11
    environment:
      MARIADB_ROOT_PASSWORD: ${MARIADB_ROOT_PASSWORD_VAL} # Sostituito da .env
      MARIADB_DATABASE: ${MARIADB_DATABASE_VAL}         # Sostituito da .env
      MARIADB_USER: ${MARIADB_USER_VAL}                 # Sostituito da .env
      MARIADB_PASSWORD: ${MARIADB_PASSWORD_VAL}         # Sostituito da .env
    ports:
      - "3306:3306" # Nota: la porta host per il DB potrebbe anche venire da .env

  webapp:
    build: ./MyWebApiApp
    ports:
      - "${WEBAPP_HOST_PORT_MAPPING_VAL}:${WEBAPP_CONTAINER_PORT_VAL}" # Sostituzione per le porte
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT_NAME}
      # La Connection String può essere costruita qui o passata interamente
      ConnectionStrings__DefaultConnection: "Server=db;Port=3306;Database=${MARIADB_DATABASE_VAL};Uid=${MARIADB_USER_VAL};Pwd=${MARIADB_PASSWORD_VAL};AllowPublicKeyRetrieval=true"
    depends_on:
      - db

```

In questo esempio, quando docker-compose up viene eseguito, ${MARIADB_ROOT_PASSWORD_VAL} nel docker-compose.yml sarà sostituito con myRootSecretFromEnvFile! (il valore dal file .env), e così via per le altre variabili.

Si può anche fornire un valore di default se la variabile non è definita nel file .env o nell'ambiente della shell, usando la sintassi ${VARIABILE:-default} o ${VARIABILE:?messaggio_errore_se_mancante}.

#### 6.2.2. **Best Practice**: Esclusione dei file `.env` da Git (uso di `.gitignore` e file `.env.example`)

È **assolutamente cruciale non committare mai file `.env` che contengono segreti reali** (password, API key, ecc.) nel proprio repository Git. Questi file sono specifici dell'ambiente e contengono dati sensibili.

La pratica corretta è:

1. Aggiungere .env al file .gitignore:

    Creare o modificare il file .gitignore nella root del progetto (la stessa directory di docker-compose.yml e .env) e aggiungere la riga:

    ```gitignore
    # MyProjectRoot/.gitignore

    # Ignora i file di ambiente locali specifici
    .env
    *.env.local
    ```

    Questo impedisce a Git di tracciare e committare il file `.env`.

2. Creare un file .env.example (o env.template):

    Creare un file di esempio, chiamato tipicamente .env.example, che mostri la struttura e i nomi di tutte le variabili d'ambiente necessarie per eseguire l'applicazione, ma con valori fittizi, segnaposto o vuoti. Questo file .env.example DEVE essere committato nel repository Git.

    Esempio di MyProjectRoot/.env.example:

    ```env
    # MyProjectRoot/.env.example
    # Copiare questo file in .env e sostituire i valori con quelli reali.

    # Configurazione Database MariaDB
    MARIADB_ROOT_PASSWORD_VAL=inserire_password_root_molto_sicura
    MARIADB_DATABASE_VAL=nome_database_applicazione
    MARIADB_USER_VAL=nome_utente_applicazione
    MARIADB_PASSWORD_VAL=password_utente_applicazione_molto_sicura

    # Configurazione Applicazione WebApp
    WEBAPP_CONTAINER_PORT_VAL=8080
    WEBAPP_HOST_PORT_MAPPING_VAL=8080 # Porta sull'host per accedere all'app
    ASPNETCORE_ENVIRONMENT_NAME=Development # Development, Staging, o Production

    ```

    Quando un altro sviluppatore (o una pipeline di CI/CD) clona il repository, dovrà copiare `.env.example` in `.env` e compilare quest'ultimo con i valori appropriati per il proprio ambiente.

### 6.3. La direttiva `env_file` in Docker Compose per caricare variabili da file esterni

Oltre alla sostituzione di variabili da un file `.env` di default, Docker Compose permette di specificare uno o più file da cui caricare le variabili d'ambiente direttamente nell'ambiente di un *servizio specifico*, utilizzando la direttiva `env_file` all'interno della definizione del servizio nel `docker-compose.yml`.

Le variabili caricate tramite `env_file` vengono aggiunte all'ambiente del container del servizio, proprio come se fossero state definite sotto la chiave `environment`.

Esempio di docker-compose.yml con env_file:

Supponiamo di avere due file di ambiente separati:

MyProjectRoot/db.env:

```env
# MyProjectRoot/db.env
MARIADB_ROOT_PASSWORD=anotherSecretRootPassword
MARIADB_DATABASE=specific_db
MARIADB_USER=specific_user
MARIADB_PASSWORD=specific_pass
```

MyProjectRoot/webapp.env:

```env
# MyProjectRoot/webapp.env
ASPNETCORE_ENVIRONMENT=Staging
Logging__LogLevel__Default=Warning # Esempio di override per ASP.NET Core

```

Il `docker-compose.yml` potrebbe essere:

```yml
services:
  db:
    image: mariadb:10.11
    env_file:
      - ./db.env # Carica variabili da db.env per il servizio db
    # Le variabili da db.env (es. MARIADB_DATABASE) sono ora disponibili
    # nell'ambiente del container 'db'.
  webapp:
    build: ./MyWebApiApp
    env_file:
      - ./webapp.env # Carica variabili da webapp.env per il servizio webapp
    environment:
      # Si possono ancora usare variabili globali da un .env principale per sostituzione qui,
      # o costruire la connection string usando variabili che si assume siano definite
      # in webapp.env o in un .env globale.
      # Esempio: ConnectionStrings__DefaultConnection generata da variabili in webapp.env o .env globale
      SomeOtherSetting: "valore fisso"

```

**Ordine di precedenza delle variabili d'ambiente in Docker Compose**:

1. Variabili impostate nella shell da cui si esegue `docker-compose`.

2. Variabili definite nel file `.env` (nella directory del progetto) usate per sostituzione `${VAR}` nel `docker-compose.yml`.

3. Argomenti passati con `docker-compose run -e ...`.

4. Variabili definite nella direttiva `environment` del servizio nel `docker-compose.yml`.

5. Variabili definite nella direttiva `env_file` del servizio nel `docker-compose.yml` (se più file sono specificati in `env_file`, l'ultimo file ha la precedenza per variabili con lo stesso nome).

6. Variabili definite nel `Dockerfile` (es. `ENV`).

Anche i file specificati in `env_file` (se contengono segreti) non dovrebbero essere committati in Git e dovrebbero avere i loro corrispondenti file `.example`.

L'uso combinato di un file `.env` principale per valori comuni e sostituzioni, e `env_file` per configurazioni specifiche di un servizio (specialmente se numerose), può offrire una buona organizzazione.

### 6.4. **Criticità**: Perché evitare segreti hardcoded nel `docker-compose.yml`

È **fondamentale e imperativo non scrivere mai segreti** (password, API key, token di accesso, chiavi di crittografia, ecc.) direttamente (hardcoded) nel file `docker-compose.yml` o nel `Dockerfile`.

**ESEMPIO DA EVITARE ASSOLUTAMENTE**:

```yml
# docker-compose.yml - NON FARE QUESTO!
services:
  db:
    image: mariadb:10.11
    environment:
      MARIADB_ROOT_PASSWORD: "password123SuperSegreta" # <--- SEGRETO HARDCODED!
      MARIADB_DATABASE: "testdb"
      MARIADB_USER: "utente"
      MARIADB_PASSWORD: "altrapasswordsegreta" # <--- ALTRO SEGRETO HARDCODED!

```

**Rischi e problemi dell'hardcoding dei segreti**:

1. **Esposizione nel Controllo Versione (Git)**: I file `docker-compose.yml` e `Dockerfile` sono tipicamente (e correttamente) committati nel repository Git per definire l'infrastruttura e la build dell'applicazione. Se i segreti sono hardcoded, diventano parte della storia del repository, visibili a chiunque abbia accesso al codice sorgente, anche se vengono rimossi in commit successivi (la storia di Git li conserva).

2. **Difficoltà di Rotazione dei Segreti**: Se un segreto viene compromesso o necessita di essere cambiato regolarmente per policy di sicurezza, modificarlo richiede la modifica del file `docker-compose.yml` (o `Dockerfile`), un nuovo commit, e un nuovo deployment. Questo è inefficiente e soggetto a errori.

3. **Violazione del Principio di Separazione delle Preoccupazioni**: La definizione dell'infrastruttura e della build (compiti di `docker-compose.yml` e `Dockerfile`) dovrebbe essere separata dalla gestione dei dati di configurazione sensibili, che sono specifici dell'istanza o dell'ambiente.

4. **Rischio di Esposizione Accidentale**: File contenenti segreti hardcoded possono essere condivisi o esposti accidentalmente più facilmente.

5. **Un Segreto per Tutti gli Ambienti**: Se si usa lo stesso `docker-compose.yml` (o sue varianti) per sviluppo, test e produzione, si potrebbe essere tentati di usare gli stessi segreti hardcoded, il che è una pessima pratica di sicurezza.

Analisi degli esempi didattici e rischi: Molti tutorial e guide online, per motivi di brevità e semplicità, mostrano segreti hardcoded. Questi esempi sono accettabili solo per esperimenti locali, rapidi e personali, e mai per progetti reali, condivisi, o destinati alla produzione. È cruciale comprendere che tali esempi semplificano eccessivamente l'aspetto della sicurezza.

Adottare sempre le pratiche corrette:

- Per lo sviluppo locale con Docker Compose: utilizzare file `.env` esclusi da Git (con un `.env.example` committato).

- Per la produzione: utilizzare sistemi di gestione dei segreti dedicati.

### 6.5. Panoramica delle soluzioni per la produzione

Per ambienti di staging e produzione, i file `.env` locali non sono la soluzione ideale o sufficientemente sicura per la gestione dei segreti. Si devono utilizzare sistemi più robusti e dedicati:

#### 6.5.1. Docker Secrets (per Docker Swarm e Kubernetes)

- **Principi**: Docker Secrets è una funzionalità integrata in Docker (principalmente per la modalità Swarm, ma il concetto è analogo a Kubernetes Secrets) che permette di gestire centralmente dati sensibili e renderli accessibili in modo sicuro solo ai container dei servizi che ne hanno esplicitamente bisogno. I segreti sono criptati a riposo (nel manager di Swarm) e trasmessi ai container autorizzati attraverso un filesystem temporaneo in memoria (tipicamente montato in `/run/secrets/<nome_segreto>` all'interno del container). L'applicazione nel container legge il segreto da questo file.

- **Vantaggi**:

    - Gestione centralizzata e sicura dei segreti.

    - Controllo granulare degli accessi ai segreti per servizio.

    - Rotazione dei segreti più semplice senza dover modificare le immagini o le definizioni dei servizi.

    - I segreti non appaiono in variabili d'ambiente (che possono essere ispezionate) o log.

- **Esempio concettuale (Docker Swarm)**:

    1. Creazione di un segreto:

        ```yml
        printf "MiaPasswordDatabaseUltraSegreta" | docker secret create db_password -

        ```

        (Il `-` indica di leggere da stdin).

    2. Utilizzo nel `docker-compose.yml` (per deployment in Swarm, versione 3.1+):

        ```yml
        services:
          mydb:
            image: mariadb:10.11
            environment:
              # MariaDB può leggere la password da un file
              MARIADB_ROOT_PASSWORD_FILE: /run/secrets/db_password
            secrets:
              - db_password # Rende il segreto 'db_password' accessibile al servizio
        secrets:
          db_password:
            external: true # Indica che il segreto è gestito esternamente da Swarm
            # In alternativa, per sviluppo o test, si può usare 'file: ./db_password.txt'
            # ma questo file .txt non dovrebbe contenere segreti di produzione.

        ```

    L'applicazione MariaDB leggerà la password dal file `/run/secrets/db_password`. Molte immagini ufficiali (come `mariadb`, `postgres`, `mysql`) supportano la lettura di password da file specificando una variabile d'ambiente con suffisso `_FILE`.

#### 6.5.2. Servizi di gestione segreti dei Cloud Provider (es. Azure Key Vault, AWS Secrets Manager, Google Secret Manager)

Le principali piattaforme cloud offrono servizi dedicati e altamente sicuri per la gestione dei segreti:

- **Azure Key Vault** (Microsoft Azure)

- **AWS Secrets Manager** e **AWS Systems Manager Parameter Store** (Amazon Web Services)

- **Google Secret Manager** (Google Cloud Platform)

- **HashiCorp Vault** (soluzione potente, open-source e cloud-agnostic, può essere self-hosted o usata come servizio cloud).

- **Integrazione e Benefici**:

    - **Storage Sicuro e Criptato**: I segreti sono memorizzati in modo sicuro, con crittografia a riposo e in transito.

    - **Controllo Granulare degli Accessi**: Integrazione con i sistemi di Identity and Access Management (IAM) del cloud provider per definire chi (utenti, gruppi, applicazioni, servizi) può accedere a quali segreti e con quali permessi (lettura, scrittura, ecc.).

    - **Audit Trail**: Registrazione dettagliata di chi ha accedito o tentato di accedere ai segreti.

    - **Rotazione Automatica dei Segreti**: Molti di questi servizi supportano la rotazione automatica delle password (es. per database RDS in AWS Secrets Manager).

    - **Versionamento dei Segreti**: Possibilità di mantenere versioni multiple di un segreto.

    - **SDK e Integrazione con Applicazioni**: Forniscono SDK per diverse lingue (incluso .NET) che permettono alle applicazioni di recuperare i segreti al runtime in modo programmatico.

    L'applicazione, quando eseguita in un ambiente cloud (es. in un container su Azure Kubernetes Service, AWS ECS, Google Kubernetes Engine), viene configurata con un'identità (es. Managed Identity su Azure, IAM Role su AWS) che ha i permessi per leggere i segreti dal servizio di gestione. L'applicazione utilizza l'SDK del provider per recuperare i segreti necessari all'avvio o quando servono.

### 6.6. Configurazione dell'applicazione ASP.NET Core per leggere variabili d'ambiente e segreti da diverse fonti

ASP.NET Core ha un sistema di configurazione potente e flessibile che può attingere dati da una varietà di fonti in modo gerarchico. Le fonti di configurazione vengono caricate in un ordine specifico, e i valori provenienti da fonti caricate successivamente sovrascrivono quelli di fonti precedenti se le chiavi sono le stesse.

L'ordine di caricamento predefinito tipico per un'applicazione web ASP.NET Core è:

1. File `appsettings.json`.

2. File `appsettings.{Environment}.json` (es. `appsettings.Development.json`, `appsettings.Production.json`). Il valore di `{Environment}` è determinato dalla variabile d'ambiente `ASPNETCORE_ENVIRONMENT`.

3. User Secrets (attivo principalmente in ambiente di Sviluppo).

4. **Variabili d'ambiente**.

5. Argomenti da riga di comando.

**Questo significa che le variabili d'ambiente sovrascrivono i valori definiti nei file `appsettings.json`**. Questo è il meccanismo chiave che rende la configurazione tramite variabili d'ambiente così efficace con Docker.

Esempio di override:

Se in appsettings.json si ha:

```json
{
  "MyApplication": {
    "FeatureX": {
      "IsEnabled": false,
      "ApiKey": "placeholder_api_key_from_json"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=devdb_json;..."
  }
}

```

E nel container (o nell'ambiente host da cui Docker Compose legge il file `.env`) si impostano le seguenti variabili d'ambiente:

- `ASPNETCORE_ENVIRONMENT=Production`

- `MyApplication__FeatureX__IsEnabled=true` (notare il doppio underscore `__` per rappresentare la gerarchia JSON)

- `MyApplication__FeatureX__ApiKey=REAL_API_KEY_FROM_ENV`

- `ConnectionStrings__DefaultConnection="Server=db-container-prod;Database=proddb_env;..."`

L'applicazione ASP.NET Core:

- Caricherà `appsettings.json` e poi `appsettings.Production.json` (se esiste).

- Sovrascriverà `MyApplication:FeatureX:IsEnabled` con `true`.

- Sovrascriverà `MyApplication:FeatureX:ApiKey` con `REAL_API_KEY_FROM_ENV`.

- Sovrascriverà `ConnectionStrings:DefaultConnection` con la stringa di connessione fornita dalla variabile d'ambiente.

Leggere segreti da file (come per Docker Secrets o file montati):

Se un segreto è fornito come file (es. /run/secrets/db_password da Docker Secrets), l'applicazione deve leggere quel file. ASP.NET Core può essere configurato per aggiungere provider di configurazione basati su file.

```cs
// Esempio in Program.cs per leggere un segreto da un file
// (questo è un esempio base, integrazioni più robuste sono possibili)

var dbPasswordPath = "/run/secrets/db_password"; // Percorso standard per Docker Secrets
if (File.Exists(dbPasswordPath))
{
    string passwordFromFile = File.ReadAllText(dbPasswordPath).Trim();
    // Aggiungere questo valore alla configurazione, magari in modo specifico
    // per costruire la stringa di connessione.
    // Esempio: builder.Configuration.AddInMemoryCollection(new Dictionary<string, string> {
    //    {"Secrets:DbPassword", passwordFromFile}
    // });
    // Oppure, si può modificare una stringa di connessione esistente:
    // var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    // if (connStr != null) {
    //    builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr.Replace("PLACEHOLDER_PASSWORD", passwordFromFile);
    // }
}

```

Molte immagini (come MariaDB) supportano la lettura di password da file tramite variabili d'ambiente come `MARIADB_ROOT_PASSWORD_FILE=/run/secrets/db_password`. Per l'applicazione ASP.NET Core, se si usa un ORM come Entity Framework Core, si può costruire la stringa di connessione dinamicamente al momento della configurazione del DbContext, leggendo il segreto dal file.

Integrazione con Cloud Secret Managers:

Per servizi come Azure Key Vault, AWS Secrets Manager, ecc., si utilizzano i pacchetti NuGet specifici forniti dal cloud provider. Questi pacchetti permettono di aggiungere il gestore di segreti come un provider di configurazione ASP.NET Core.

Esempio per Azure Key Vault (richiede il pacchetto Azure.Extensions.AspNetCore.Configuration.Secrets e Azure.Identity):

```cs
// In Program.cs (semplificato)
if (builder.Environment.IsProduction()) // O basato su una configurazione specifica
{
    var keyVaultEndpoint = builder.Configuration["AzureKeyVaultEndpoint"]; // URL del Key Vault
    if (!string.IsNullOrEmpty(keyVaultEndpoint))
    {
        // Usa DefaultAzureCredential per autenticazione (es. Managed Identity)
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
    }
}

```

Questo caricherà i segreti da Azure Key Vault e li renderà disponibili attraverso l'interfaccia `IConfiguration` standard di ASP.NET Core, sovrascrivendo valori da fonti precedenti se le chiavi corrispondono.

La strategia di gestione della configurazione e dei segreti deve essere pianificata attentamente, privilegiando la sicurezza e la flessibilità per i diversi ambienti di deployment.

## 7. Orchestrazione con Docker Compose

Docker Compose è uno strumento progettato per definire ed eseguire applicazioni Docker multi-container in modo semplice e dichiarativo. Si utilizza un file di configurazione in formato YAML (tipicamente `docker-compose.yml`) per specificare tutti i componenti dell'applicazione (servizi, reti, volumi) e le loro interazioni.

### 7.1. Introduzione a Docker Compose: Perché usarlo per applicazioni multi-container?

Per l'applicazione `MyWebApiApp` e il database MariaDB, finora si sono utilizzati comandi `docker run` separati, `docker network create` manuale, ecc. Questo approccio manuale, sebbene didattico per comprendere i singoli comandi Docker, presenta diversi svantaggi per applicazioni reali:

- **Complessità**: I comandi `docker run` possono diventare molto lunghi e complessi, con numerose opzioni per porte, volumi, reti, variabili d'ambiente.

- **Soggetto a Errori**: È facile dimenticare un'opzione o commettere errori di battitura, portando a comportamenti imprevisti.

- **Non Facilmente Replicabile**: Condividere e replicare un setup multi-container tra diversi sviluppatori o ambienti diventa difficile e inefficiente.

- **Gestione Ingestibile**: Al crescere del numero di servizi (container) che compongono l'applicazione (es. un'app web, un database, una cache Redis, un message broker RabbitMQ, un servizio di logging ELK), gestire tutto con singoli comandi `docker run` diventa impraticabile.

- **Mancanza di Definizione Unica**: Non c'è un singolo posto dove l'intera architettura dell'applicazione (a livello di container) è definita.

**Docker Compose risolve questi problemi offrendo**:

- **Definizione Dichiarativa e Semplificata**: Tutta la configurazione dell'applicazione multi-container (servizi, reti, volumi, dipendenze tra servizi, mappature di porte, variabili d'ambiente) è definita in un unico file `docker-compose.yml`. Questo file agisce come "documentazione eseguibile" dell'architettura.

- **Gestione con Singolo Comando**:

    - `docker-compose up`: Avvia (e costruisce le immagini se necessario) l'intero stack di servizi definito nel file Compose.

    - `docker-compose down`: Ferma e rimuove i container, le reti e (opzionalmente) i volumi creati da Compose.

- **Networking Automatico e Semplificato**: Per impostazione predefinita, Docker Compose crea una rete bridge personalizzata per l'applicazione. Tutti i servizi definiti nel file Compose vengono connessi a questa rete e possono comunicare tra loro utilizzando i nomi dei servizi come hostname (grazie alla risoluzione DNS integrata).

- **Configurazione Consistente e Replicabile**: Il file `docker-compose.yml` può essere versionato in Git insieme al codice sorgente, garantendo che tutti gli sviluppatori e gli ambienti (sviluppo, test) utilizzino la stessa configurazione di base.

- **Orchestrazione Locale Ideale**: È lo strumento perfetto per ambienti di sviluppo e test locali, e può essere utilizzato anche per deployment semplici in produzione per applicazioni di piccole e medie dimensioni (sebbene per produzione su larga scala e con requisiti di alta disponibilità, orchestratori più complessi come Kubernetes siano generalmente preferiti).

- **Sviluppo Iterativo Veloce**: Facilita la modifica, la ricostruzione e il riavvio rapido dei servizi durante lo sviluppo.

### 7.2. Struttura di un file `docker-compose.yml`: `version`, `services`, `networks`, `volumes`

Un file `docker-compose.yml` è scritto in YAML (YAML Ain't Markup Language), un formato di serializzazione dati leggibile dall'uomo. La sua struttura di base è la seguente:

```yml
# docker-compose.yml

services:       # Sezione principale dove vengono definiti i singoli container (chiamati "servizi").
  # Nome del primo servizio (es. l'applicazione web)
  webapp:
    # ... configurazione dettagliata per il servizio 'webapp' ...
    # (es. build dal Dockerfile, immagine da usare, porte, variabili d'ambiente, volumi, dipendenze)

  # Nome del secondo servizio (es. il database)
  db:
    # ... configurazione dettagliata per il servizio 'db' ...

  # ... altri servizi ...

networks:       # (Opzionale, ma raccomandato per esplicitare la configurazione)
                # Definisce le reti personalizzate che i servizi possono utilizzare.
  # Nome della rete personalizzata
  app-net:
    driver: bridge # Tipo di rete (bridge è il default e il più comune per Compose locale).
    # Si possono specificare altre opzioni di rete qui.

volumes:        # (Opzionale, ma essenziale per la persistenza dei dati stateful)
                # Definisce i "named volumes" (volumi nominati) per la persistenza dei dati.
  # Nome del volume per i dati del database
  db_data:
    driver: local # Tipo di driver del volume (local è il default, significa gestito da Docker sull'host).
    # Si possono specificare altre opzioni per il volume qui.

```

- **`version`**: Indica la versione dello schema del file Docker Compose. Diverse versioni supportano diverse funzionalità e sintassi. È importante consultare la documentazione di Docker per la compatibilità.

- **`services`**: È il cuore del file. Ogni chiave di primo livello sotto `services` (es. `webapp`, `db`) definisce un servizio. Il nome del servizio è significativo: viene usato da Docker Compose per creare i container (spesso con un prefisso basato sul nome della directory del progetto) e, crucialmente, come **hostname** per la comunicazione tra i servizi all'interno della rete creata da Compose.

- **`networks`**: Permette di definire esplicitamente le reti che i servizi utilizzeranno. Se omessa, Docker Compose crea automaticamente una rete di default per il progetto (chiamata `nomerootprogetto_default`). È buona pratica definire esplicitamente almeno una rete per chiarezza e maggiore controllo.

- **`volumes`**: Permette di definire "named volumes". Questi sono volumi gestiti da Docker, ideali per la persistenza dei dati di servizi stateful come i database. I dati in un named volume persistono anche se i container vengono rimossi e ricreati.

### 7.3. Definizione dei servizi: `build`, `image`, `ports`, `environment`, `env_file`, `depends_on`, `restart`

All'interno della sezione `services`, ogni servizio (es. `webapp`, `db`) viene configurato con una serie di direttive (chiavi YAML):

- image: <nome_immagine>:<tag>:

    Specifica l'immagine Docker da utilizzare per creare il container del servizio. Docker cercherà prima l'immagine localmente; se non la trova, la scaricherà dal registro configurato (di default, Docker Hub).

    Esempio: image: mariadb:10.11

- build: <percorso_contesto_build> o build: { context: <path>, dockerfile: <nome_Dockerfile_alternativo>, args: { ... }, ... }:

    Indica a Docker Compose di costruire l'immagine per il servizio da un Dockerfile invece di usare un'immagine preesistente.

    - `context`: Il percorso (relativo al `docker-compose.yml` o assoluto) della directory che contiene il `Dockerfile` e i file sorgente per la build (il contesto di build). Esempio: `build: ./MyWebApiApp` (se `MyWebApiApp` contiene il `Dockerfile`).

    - `dockerfile`: (Opzionale) Il nome del `Dockerfile` se è diverso da `Dockerfile` (es. `dockerfile: Dockerfile.dev`).

    - args: (Opzionale) Permette di passare argomenti di build (ARG nel Dockerfile) al processo di build.

        Se si specifica sia build che image, il nome dell'immagine specificato con image verrà usato per taggare l'immagine costruita localmente.

- ports: ["<host_port>:<container_port>", ...]:

    Mappa le porte tra la macchina host e il container, proprio come l'opzione -p di docker run.

    Esempio: ports: ["8088:8080"] (mappa la porta 8088 dell'host alla 8080 del container).

- environment: { NOME_VAR_1: VALORE_1, NOME_VAR_2: VALORE_2, ... } o environment: ["NOME_VAR_1=VALORE_1", "NOME_VAR_2=VALORE_2", ...]:

    Imposta variabili d'ambiente all'interno del container del servizio.

    Esempio:

    ```yml
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      DB_HOST: db # 'db' è il nome del servizio database definito in Compose
      API_KEY: ${EXTERNAL_API_KEY} # Utilizza la sostituzione da .env o dalla shell

    ```

- env_file: <percorso_file_env> o env_file: ["file1.env", "file2.env", ...]:

    Carica variabili d'ambiente da uno o più file esterni specificati. I percorsi sono relativi al docker-compose.yml.

    Esempio: env_file: ./.env.webapp

- depends_on: [<nome_servizio1>, <nome_servizio2>, ...] o forma estesa:

    Definisce le dipendenze tra i servizi. Docker Compose avvierà i servizi nell'ordine specificato dalle dipendenze. Ad esempio, se webapp dipende da db, Compose avvierà db prima di webapp.

    Importante: depends_on di base controlla solo l'ordine di avvio dei container, non garantisce che il servizio dipendente sia effettivamente "pronto" e operativo (es. il database potrebbe essere in fase di inizializzazione).

    Per un controllo più robusto, si può usare la forma estesa con condition:

    ```yml
    depends_on:
      db:
        condition: service_healthy # Attende che il servizio 'db' passi il suo healthcheck
      # cache:
      #   condition: service_started # Attende solo che il container 'cache' sia avviato

    ```

    `service_healthy` richiede che il servizio dipendente (es. `db`) abbia un `healthcheck` definito.

- restart: <policy>:

    Definisce la politica di riavvio per il container del servizio in caso di arresto.

    - `no` (default): Non riavviare automaticamente.

    - `always`: Riavvia sempre il container se si ferma, a meno che non sia stato fermato esplicitamente (es. con `docker-compose stop`).

    - `on-failure`: Riavvia il container solo se esce con un codice di errore (diverso da zero). Si può specificare un numero massimo di tentativi (es. `on-failure:5`).

    - unless-stopped: Riavvia sempre il container se si ferma, tranne quando è stato fermato esplicitamente dall'utente o da un altro processo Docker. Questa è spesso una buona scelta per servizi come i database.

        Esempio: restart: unless-stopped

- **`volumes: [...]`**: Monta volumi o bind mounts nel container (vedi Sezione 7.4).

- **`networks: [<nome_rete1>, ...]`**: Connette il servizio a una o più reti specificate (definite nella sezione `networks` a livello root).

- healthcheck: { test: [...], interval: ..., timeout: ..., retries: ..., start_period: ... }:

    Definisce un comando per verificare lo stato di salute del servizio. Docker eseguirà periodicamente questo comando all'interno del container. Se il comando fallisce un certo numero di volte, il container viene marcato come unhealthy. Questo è usato da depends_on con condition: service_healthy e da orchestratori come Swarm o Kubernetes per gestire i container.

    Esempio per un database: test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]

### 7.4. Persistenza dei dati con i Volumi Docker (`volumes`): named volumes vs bind mounts

I dati all'interno del layer scrivibile di un container sono effimeri: se il container viene rimosso, questi dati vengono persi. Per la persistenza dei dati (es. file di un database, file caricati dagli utenti), Docker offre i **volumi**.

Esistono due tipi principali di montaggi di volumi con Docker Compose:

1. **Named Volumes (Volumi Nominati)**:

    - Sono la **modalità raccomandata** per la persistenza dei dati generati e gestiti dai container (es. dati di un database).

    - Sono completamente gestiti da Docker. Docker crea e gestisce una directory dedicata sul filesystem dell'host (la sua posizione esatta è un dettaglio di implementazione di Docker, es. `/var/lib/docker/volumes/` su Linux).

    - Si definiscono nella sezione `volumes:` a livello root del `docker-compose.yml` (assegnando loro un nome) e poi si referenziano nella configurazione del servizio.

    - **Vantaggi**:

        - **Portabilità**: Indipendenti dalla struttura di directory specifica dell'host. Il `docker-compose.yml` è più portabile tra diverse macchine.

        - **Gestione da Docker**: Possono essere elencati (`docker volume ls`), ispezionati (`docker volume inspect`), rimossi (`docker volume rm`, `docker-compose down -v`), e backuppati più facilmente.

        - **Prestazioni**: Su alcune piattaforme, possono offrire prestazioni migliori per operazioni I/O intensive rispetto ai bind mounts.

        - **Sicurezza**: Meno rischi di interferire accidentalmente con file di sistema dell'host.

    - **Esempio**:

        ```yml
        services:
          db:
            image: mariadb:10.11
            volumes:
              # Mappa il named volume 'mariadb_data' alla directory dati di MariaDB nel container
              - mariadb_data:/var/lib/mysql
        volumes:
          mariadb_data: {} # Definisce il named volume 'mariadb_data'. Docker usa il driver 'local' di default.
                          # Si può anche specificare driver: local esplicitamente.

        ```

        Con questa configurazione, i dati del database MariaDB saranno memorizzati nel volume `mariadb_data`. Anche se il container `db` viene rimosso (`docker-compose down`) e poi ricreato (`docker-compose up`), i dati nel volume `mariadb_data` persisteranno e saranno riutilizzati dal nuovo container.

2. **Bind Mounts**:

    - Montano una directory o un file specifico dal filesystem dell'**host** direttamente in un percorso all'interno del container. Il percorso sull'host è specificato esplicitamente.

    - **Vantaggi**:

        - **Sviluppo Locale (Hot Reloading)**: Molto utili durante lo sviluppo per montare il codice sorgente dell'applicazione direttamente nel container. Se si modifica il codice sull'host, le modifiche sono immediatamente visibili nel container, permettendo a strumenti come `dotnet watch` di ricompilare e ricaricare l'applicazione al volo senza dover ricostruire l'immagine Docker.

        - **Accesso a File di Configurazione dell'Host**: Per fornire file di configurazione specifici dell'host al container.

    - **Svantaggi**:

        - **Dipendenza dall'Host**: Il `docker-compose.yml` diventa dipendente dalla struttura di directory dell'host, rendendolo meno portabile.

        - **Problemi di Permessi**: Possono sorgere problemi di permessi tra l'utente sull'host che possiede i file e l'utente all'interno del container che cerca di accedervi.

        - **Rischio di Sovrascrittura**: Se si monta una directory host in una directory del container che già contiene file dall'immagine, i file dell'immagine in quella directory vengono "oscurati" (non cancellati, ma non accessibili) dai file montati dall'host.

    - **Esempio**:

        ```yml
        services:
          webapp:
            build: ./MyWebApiApp
            volumes:
              # Monta la directory del progetto MyWebApiApp dell'host in /app nel container
              # Utile per sviluppo con 'dotnet watch'.
              - ./MyWebApiApp:/app
              # Si può specificare read-only aggiungendo :ro
              - ./my-custom-config.json:/app/config/custom.json:ro

        ```

        **Attenzione con i Bind Mounts per il codice sorgente**: Se il `Dockerfile` copia il codice sorgente e lo compila (come nel nostro esempio multi-stage), un bind mount della directory sorgente sull'host nella directory `/app` del container (dove risiedono i file pubblicati) potrebbe sovrascrivere gli artefatti di build con il codice sorgente non compilato, a meno che l'entrypoint non sia configurato per compilare/eseguire da sorgente (es. con `dotnet watch run`). Per la produzione, si preferisce sempre avere gli artefatti di build cotti nell'immagine e non usare bind mounts per il codice.

**Scelta tra Named Volumes e Bind Mounts**:

- Per i **dati generati e gestiti dall'applicazione** che devono persistere (es. file di database, upload degli utenti, log che devono sopravvivere al container): usare **Named Volumes**.

- Per fornire **codice sorgente** al container durante lo **sviluppo** (per abilitare hot reload/watchers): usare **Bind Mounts**.

- Per fornire **file di configurazione specifici dell'host** al container: usare **Bind Mounts** (spesso in modalità read-only).

### 7.5. Configurazione della comunicazione tra servizi tramite nomi di servizio e reti definite

Come accennato, uno dei grandi vantaggi di Docker Compose è la semplificazione del networking tra container.

- Quando si esegue `docker-compose up`, Compose crea (se non specificato diversamente) una **rete bridge personalizzata** per il progetto (tipicamente chiamata `nomerootprogetto_default`).

- Tutti i servizi definiti nel file `docker-compose.yml` vengono automaticamente connessi a questa rete.

- All'interno di questa rete, Docker fornisce un **servizio DNS integrato** che permette ai container di risolvere gli hostname degli altri servizi usando i **nomi dei servizi** definiti nel `docker-compose.yml`.

Esempio Pratico:

Nel docker-compose.yml seguente:

```yml
services:
  webapp: # Nome del servizio: 'webapp'
    build: ./MyWebApiApp
    ports:
      - "8088:8080"
    environment:
      # La stringa di connessione usa 'db' come hostname del server MariaDB.
      # 'db' è il nome del servizio database definito sotto.
      ConnectionStrings__DefaultConnection: "Server=db;Port=3306;Database=${MARIADB_DATABASE_NAME};User=${MARIADB_USER_NAME};Password=${MARIADB_USER_PASSWORD_SECRET};AllowPublicKeyRetrieval=true"
    depends_on:
      db:
        condition: service_healthy
    networks:
      - my-app-network # Connette esplicitamente a questa rete

  db: # Nome del servizio: 'db'
    image: mariadb:10.11
    environment:
      MARIADB_ROOT_PASSWORD: ${MARIADB_ROOT_PASSWORD_SECRET}
      MARIADB_DATABASE: ${MARIADB_DATABASE_NAME}
      MARIADB_USER: ${MARIADB_USER_NAME}
      MARIADB_PASSWORD: ${MARIADB_USER_PASSWORD_SECRET}
    volumes:
      - db_volume:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin" ,"ping", "-h", "localhost", "-u", "${MARIADB_USER_NAME}", "-p${MARIADB_USER_PASSWORD_SECRET}"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - my-app-network # Connette esplicitamente a questa rete

volumes:
  db_volume:

networks:
  my-app-network: # Definizione della rete personalizzata
    driver: bridge

```

- Il servizio `webapp` può connettersi al servizio `db` usando l'hostname `db` nella sua stringa di connessione (`Server=db`).

- Docker Compose si occupa di risolvere `db` all'indirizzo IP interno corretto del container del servizio `db` all'interno della rete `my-app-network`.

- Non è necessario conoscere o gestire manualmente gli indirizzi IP dei container.

- La direttiva `networks:` sotto ogni servizio e la definizione `networks:` a livello root assicurano che entrambi i servizi siano sulla stessa rete isolata `my-app-network`. Se si omettono le direttive `networks:` sotto i servizi e la definizione a livello root, Compose creerebbe comunque una rete di default e connetterebbe i servizi ad essa. Definirle esplicitamente è una buona pratica per chiarezza e controllo.

## 8. Caso Pratico: Orchestrazione App ASP.NET Core + MariaDB con Docker Compose

Si applicheranno ora tutti i concetti visti per creare un setup completo con Docker Compose per orchestrare l'applicazione `MyWebApiApp` (ASP.NET Core Minimal API con frontend statico) e un database MariaDB, gestendo la configurazione e i segreti in modo appropriato per lo sviluppo.

Struttura delle directory del progetto (suggerita):

Assicurarsi che il progetto sia strutturato come segue. Se il Dockerfile e .dockerignore sono stati creati dentro MyWebApiApp/, andrà bene. Il docker-compose.yml e i file .env andranno nella directory che contiene MyWebApiApp/. Chiameremo questa directory MyProjectRoot/.

```text
MyProjectRoot/
├── MyWebApiApp/                 # Progetto ASP.NET Core
│   ├── wwwroot/
│   │   └── index.html
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── MyWebApiApp.csproj
│   ├── Program.cs
│   └── Dockerfile            # Dockerfile per MyWebApiApp (creato nella Sezione 3.2.2)
├── .dockerignore             # Globale per il contesto di build di Docker Compose (opzionale, ma utile se si hanno altri file/cartelle nella root)
├── docker-compose.yml        # File di Docker Compose (da creare)
├── .env                      # Variabili d'ambiente (NON COMMETTERE SE CONTIENE SEGRETI REALI) (da creare)
└── .env.example              # Esempio di file .env (DA COMMITTERE) (da creare)

```

1. MyWebApiApp/Dockerfile:

    Si utilizzerà il Dockerfile multi-stage creato nella Sezione 3.2.2. Assicurarsi che sia presente in MyWebApiApp/Dockerfile.

    ```dockerfile
    # MyWebApiApp/Dockerfile (come da Sezione 3.2.2)
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
    WORKDIR /app
    COPY *.csproj ./
    RUN dotnet restore
    COPY . ./
    RUN dotnet publish -c Release -o out
    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build-env /app/out .
    ENV ASPNETCORE_URLS=http://+:8080
    EXPOSE 8080
    ENTRYPOINT ["dotnet", "MyWebApiApp.dll"]

    ```

2. .dockerignore (nella root MyProjectRoot/):

    Questo file .dockerignore è per il contesto di build di Docker Compose se si hanno elementi nella root che non devono essere inviati al demone Docker quando si costruiscono immagini definite in docker-compose.yml (se il contesto è .). Se il Dockerfile di MyWebApiApp ha già il suo .dockerignore interno (come MyWebApiApp/.dockerignore creato nella Sezione 3.3), quello sarà usato per la build specifica di MyWebApiApp. Un .dockerignore a livello di MyProjectRoot è utile se si hanno altri servizi o file che non devono influenzare le build. Per questo esempio, ci si concentrerà sul .dockerignore di MyWebApiApp. Se si vuole un .dockerignore a livello di MyProjectRoot, potrebbe includere:

    ```dockerignore
    # MyProjectRoot/.dockerignore
    .git/
    .vscode/
    .idea/
    *.md # Esempio, se non si vogliono file Markdown nel contesto
    .env # Cruciale se il contesto di build fosse la root e si volesse evitare di copiarlo

    ```

    Per ora, l'importante è che `MyWebApiApp/.dockerignore` esista e sia configurato per escludere `bin/` e `obj/` dal contesto di build di `MyWebApiApp`.

3. MyWebApiApp/appsettings.json:

    Assicurarsi che la stringa di connessione sia presente ma possa essere vuota o un placeholder, poiché verrà sovrascritta dalle variabili d'ambiente fornite da Docker Compose.

    ```json
    // MyWebApiApp/appsettings.json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
        "DefaultConnection": "" // Sarà sovrascritta dalle variabili d'ambiente
      }
    }

    ```

    Il `Program.cs` (come modificato nella Sezione 5.5 per includere `MySqlConnector` e il test di connessione) leggerà `config.GetConnectionString("DefaultConnection")`.

4. MyProjectRoot/.env.example:

    Creare questo file nella directory MyProjectRoot/. Questo file sarà committato in Git.

    ```env
    # MyProjectRoot/.env.example
    # Copiare questo file in .env e compilare con i valori desiderati.

    # === Configurazione Database MariaDB ===
    # Password per l'utente root di MariaDB. Usare una password forte.
    MARIADB_ROOT_PASSWORD_SECRET=cambiami_password_root_super_segreta

    # Nome del database che verrà creato per l'applicazione.
    MARIADB_DATABASE_NAME=mywebapiapp_db_compose

    # Nome dell'utente che l'applicazione userà per connettersi al database.
    MARIADB_USER_NAME=mywebapiapp_user_compose

    # Password per l'utente dell'applicazione. Usare una password forte.
    MARIADB_USER_PASSWORD_SECRET=cambiami_password_utente_app_segreta

    # === Configurazione Applicazione WebApp ===
    # Porta sull'HOST a cui mappare la porta del container dell'applicazione web.
    WEBAPP_HOST_PORT=8088

    # Ambiente ASP.NET Core (Development, Staging, Production).
    ASPNETCORE_ENVIRONMENT_VALUE=Development

    # Porta INTERNA del container su cui l'app ASP.NET Core è in ascolto (definita nel Dockerfile).
    WEBAPP_CONTAINER_INTERNAL_PORT=8080

    # === Configurazione Database Host Port (Opzionale) ===
    # Porta sull'HOST a cui mappare la porta del container MariaDB (per accesso da strumenti esterni).
    MARIADB_HOST_PORT=3307

    ```

5. Creare MyProjectRoot/.env:

    Copiare il contenuto di .env.example in un nuovo file chiamato .env (nella stessa directory MyProjectRoot/). Modificare i valori in .env con le proprie password segrete e configurazioni desiderate.

    Questo file .env NON deve essere committato in Git. Assicurarsi che .env sia nel .gitignore principale del progetto.

    Esempio di MyProjectRoot/.env (con valori fittizi, sostituirli con i propri):

    ```env
    # MyProjectRoot/.env - NON COMMITTARE QUESTO FILE!

    MARIADB_ROOT_PASSWORD_SECRET=MyActualRootPassw0rd!
    MARIADB_DATABASE_NAME=app_db_dev
    MARIADB_USER_NAME=dev_user
    MARIADB_USER_PASSWORD_SECRET=MyActualUserPassw0rd!

    WEBAPP_HOST_PORT=8088
    ASPNETCORE_ENVIRONMENT_VALUE=Development
    WEBAPP_CONTAINER_INTERNAL_PORT=8080

    MARIADB_HOST_PORT=3307

    ```

### 8.1. Creazione del file `docker-compose.yml` completo

Creare il file `docker-compose.yml` nella directory `MyProjectRoot/`. Questo file orchestrerà i servizi `webapp` e `db`.

```yml
# MyProjectRoot/docker-compose.yml

services:
  # Servizio per l'applicazione ASP.NET Core
  webapp:
    build:
      context: ./MyWebApiApp    # Percorso della directory contenente il Dockerfile di MyWebApiApp
      dockerfile: Dockerfile # Nome del Dockerfile da usare (default)
    image: mywebapiapp-compose-final:latest # Nome opzionale per l'immagine costruita localmente
    container_name: mywebapiapp_service_compose # Nome specifico per il container
    ports:
      # Mappa la porta host (da .env, default 8080) alla porta interna del container (da .env, default 8080)
      - "${WEBAPP_HOST_PORT:-8080}:${WEBAPP_CONTAINER_INTERNAL_PORT:-8080}"
    environment:
      # Imposta l'ambiente ASP.NET Core (da .env, default Development)
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT_VALUE:-Development}
      # Costruisce la stringa di connessione usando variabili da .env
      # Il server 'db' è il nome del servizio MariaDB definito sotto.
      ConnectionStrings__DefaultConnection: "Server=db;Port=3306;Database=${MARIADB_DATABASE_NAME};User=${MARIADB_USER_NAME};Password=${MARIADB_USER_PASSWORD_SECRET};AllowPublicKeyRetrieval=true"
      # Per .NET Watcher, se si usano bind mount per lo sviluppo:
      # DOTNET_USE_POLLING_FILE_WATCHER: "true"
      # ASPNETCORE_URLS: http://+:${WEBAPP_CONTAINER_INTERNAL_PORT:-8080} # Già gestito da ENV nel Dockerfile
    volumes:
      # Esempio di bind mount per sviluppo (hot reload dei file statici wwwroot):
      # Attenzione: se si monta l'intera /app, assicurarsi che l'entrypoint gestisca la build se necessario.
      # Per ora, il Dockerfile produce un'immagine di release, quindi i bind mount del codice sono meno critici
      # a meno che non si voglia 'dotnet watch run' come entrypoint.
      - ./MyWebApiApp/wwwroot:/app/wwwroot # Permette di aggiornare i file statici senza rebuild dell'immagine
    depends_on:
      db: # Assicura che il servizio 'db' sia avviato e "healthy" prima di 'webapp'
        condition: service_healthy
    restart: unless-stopped # Riavvia il container a meno che non sia stato fermato esplicitamente
    networks:
      - app_network_compose # Connette il servizio a questa rete definita sotto

  # Servizio per il database MariaDB
  db:
    image: mariadb:10.11 # Usare una versione specifica è una buona pratica
    container_name: mariadb_service_compose # Nome specifico per il container
    environment:
      # Le password e i nomi sono presi dal file .env
      MARIADB_ROOT_PASSWORD: ${MARIADB_ROOT_PASSWORD_SECRET}
      MARIADB_DATABASE: ${MARIADB_DATABASE_NAME}
      MARIADB_USER: ${MARIADB_USER_NAME}
      MARIADB_PASSWORD: ${MARIADB_USER_PASSWORD_SECRET}
    volumes:
      # Volume nominato per la persistenza dei dati di MariaDB.
      # 'db_data_compose' è il nome del volume, '/var/lib/mysql' è il percorso dati in MariaDB.
      - db_data_compose:/var/lib/mysql
    ports:
      # Mappa la porta host (da .env, default 3306) alla porta interna del container (3306)
      # Utile per connettersi al DB da strumenti sull'host (es. DBeaver, MySQL Workbench).
      - "${MARIADB_HOST_PORT:-3306}:3306"
    healthcheck:
      # Comando per verificare se MariaDB è pronto ad accettare connessioni.
      # Usa le credenziali dell'utente dell'applicazione per il ping.
      test: ["CMD", "mysqladmin" ,"ping", "-h", "localhost", "-u", "${MARIADB_USER_NAME}", "-p${MARIADB_USER_PASSWORD_SECRET}"]
      interval: 15s      # Intervallo tra i controlli
      timeout: 10s       # Tempo massimo per considerare il controllo fallito
      retries: 5         # Numero di tentativi falliti prima di marcare come 'unhealthy'
      start_period: 30s  # Periodo di grazia all'avvio prima che i fallimenti contino
    restart: unless-stopped # Riavvia il container a meno che non sia stato fermato esplicitamente
    networks:
      - app_network_compose # Connette il servizio a questa rete definita sotto

# Definizione dei volumi nominati
volumes:
  db_data_compose: # Il nome del volume usato dal servizio 'db'
    driver: local  # Usa il driver 'local' (default), gestito da Docker sull'host

# Definizione delle reti personalizzate
networks:
  app_network_compose: # Il nome della rete usata dai servizi
    driver: bridge     # Usa il driver 'bridge' (default per reti personalizzate locali)

```

**Spiegazioni chiave del `docker-compose.yml`**:

- `services.webapp.build.context: ./MyWebApiApp`: Dice a Compose di cercare il `Dockerfile` nella sottodirectory `MyWebApiApp`.

- `services.webapp.ports`: Utilizza la sostituzione di variabili (`${VAR:-default}`) per prendere i valori dal file `.env`, con fallback a valori di default se non definiti.

- `services.webapp.environment.ConnectionStrings__DefaultConnection`: Costruisce la stringa di connessione usando le variabili definite nel file `.env`. Il server è `db`, che è il nome del servizio MariaDB.

- `services.webapp.volumes`: È stato aggiunto un esempio di bind mount per la cartella `wwwroot`. Questo permette di modificare i file HTML/CSS/JS in `MyWebApiApp/wwwroot` sull'host e vedere le modifiche riflesse nel browser senza dover ricostruire l'immagine `webapp` (il server Kestrel servirà i file aggiornati dal volume montato).

- `services.webapp.depends_on.db.condition: service_healthy`: Questa è una dipendenza robusta. Il servizio `webapp` attenderà finché il servizio `db` non solo è avviato, ma ha anche superato il suo `healthcheck`, indicando che è pronto ad accettare connessioni.

- `services.db.image: mariadb:10.11`: Specifica una versione fissa di MariaDB per build più consistenti.

- `services.db.environment`: Popolato interamente dalle variabili nel file `.env`.

- `services.db.volumes`: Utilizza il volume nominato `db_data_compose` per la persistenza dei dati di MariaDB.

- `services.db.healthcheck`: Definisce un comando (`mysqladmin ping`) per verificare lo stato di MariaDB. Questo è cruciale per la condizione `service_healthy` di `webapp`. Nota l'uso di `-u "${MARIADB_USER_NAME}" -p"${MARIADB_USER_PASSWORD_SECRET}"` per autenticare il ping.

- `volumes.db_data_compose`: Definisce formalmente il volume nominato.

- `networks.app_network_compose`: Definisce la rete bridge personalizzata a cui entrambi i servizi sono connessi.

### 8.2. Gestione dello stack multi-container: `docker-compose up`, `down`, `logs`, `ps`, `exec`, `build`

Tutti i seguenti comandi devono essere eseguiti dal terminale, nella directory `MyProjectRoot/` (dove si trova il file `docker-compose.yml`).

- **Avviare l'intero stack (costruendo le immagini se necessario)**:

    ```sh
    docker-compose up

    ```

    Questo comando avvia i servizi in foreground, mostrando i log aggregati di tutti i container nel terminale. La prima volta che viene eseguito (o se sono state fatte modifiche al Dockerfile o al contesto di build di webapp), Docker Compose costruirà l'immagine webapp. Scaricherà l'immagine mariadb se non presente localmente.

    Per avviare in background (detached mode):

    ```sh
    docker-compose up -d

    ```

- Fermare e rimuovere i container, le reti e i volumi (opzionale per i volumi):

    Per fermare i container e rimuovere i container, le reti create da Compose e i link:

    ```sh
    docker-compose down

    ```

    Per rimuovere anche i **volumi nominati** definiti nella sezione `volumes` del `docker-compose.yml` (come `db_data_compose`):

    ```sh
    docker-compose down -v

    ```

    **ATTENZIONE**: `docker-compose down -v` **cancella permanentemente i dati** memorizzati nel volume `db_data_compose` (quindi il contenuto del database MariaDB). Usare con cautela.

- Visualizzare i log dei servizi:

    Se i container sono in esecuzione (specialmente in detached mode), si possono visualizzare i log:

    ```sh
    docker-compose logs

    ```

    Per seguire i log in tempo reale (come `tail -f`):

    ```sh
    docker-compose logs -f

    ```

    Per visualizzare i log di un servizio specifico (es. `webapp` o `db`):

    ```sh
    docker-compose logs -f webapp
    # In un altro terminale:
    # docker-compose logs -f db

    ```

- **Elencare i container in esecuzione gestiti da Compose per il progetto corrente**:

    ```sh
    docker-compose ps

    ```

    Mostra lo stato dei container (es. `Up`, `Exit 0`, `Up (healthy)`), le porte mappate, ecc.

- Eseguire un comando all'interno di un container in esecuzione:

    Simile a docker exec, ma usando il nome del servizio definito in Compose.

    Ad esempio, per aprire una shell interattiva (sh) nel container del servizio webapp:

    ```sh
    docker-compose exec webapp sh

    ```

    (L'immagine base mcr.microsoft.com/dotnet/aspnet di solito contiene sh. Se contenesse bash, si userebbe bash).

    Una volta dentro la shell del container, si possono esplorare i file, controllare i processi, ecc. Digitare exit per uscire.

    Per connettersi al database MariaDB usando il client `mysql` all'interno del container `db`:

    ```sh
    # Assicurarsi che le variabili d'ambiente siano disponibili nella shell corrente
    # o sostituire i valori direttamente.
    # Il file .env non è letto da 'exec' di default.
    # Si può fare così (esempio per bash/zsh, adattare per cmd/PowerShell):
    # export $(grep -v '^#' .env | xargs) # Carica .env nella shell corrente (con cautela)
    # docker-compose exec db mysql -u "${MARIADB_USER_NAME}" -p"${MARIADB_USER_PASSWORD_SECRET}" "${MARIADB_DATABASE_NAME}"
    # Oppure, più semplicemente, connettersi come root usando la password di root:
    docker-compose exec db mysql -u root -p"${MARIADB_ROOT_PASSWORD_SECRET}"

    ```

    (Verrà chiesta la password se -p è usato senza specificarla direttamente, oppure se si usa MARIADB_ROOT_PASSWORD_SECRET va inserita subito dopo -p).

    Una volta dentro il client mysql, si possono eseguire query SQL (es. SHOW DATABASES; USE nome_db; SHOW TABLES; SELECT \* FROM nome_tabella; exit;).

- Ricostruire le immagini dei servizi:

    Se si apportano modifiche al Dockerfile di un servizio (es. webapp) o ai file nel suo contesto di build che influenzano l'immagine, è necessario ricostruire l'immagine.

    ```sh
    docker-compose build webapp # Ricostruisce solo l'immagine per il servizio 'webapp'
    # Oppure:
    # docker-compose build # Ricostruisce le immagini per tutti i servizi che hanno una sezione 'build'

    ```

    In alternativa, si può usare l'opzione `--build` con `up`:

    ```sh
    docker-compose up --build # Ricostruisce le immagini prima di avviare i servizi
    docker-compose up -d --build # Ricostruisce e avvia in detached mode

    ```

- **Fermare i servizi senza rimuoverli**:

    ```sh
    docker-compose stop

    ```

    I container vengono fermati ma non rimossi. Possono essere riavviati con `docker-compose start`.

- **Avviare servizi precedentemente fermati**:

    ```sh
    docker-compose start

    ```

### 8.3. Verifica del funzionamento dell'applicazione, della corretta connessione al database e della persistenza dei dati

1. Avviare lo stack completo:

    Dalla directory MyProjectRoot/, eseguire:

    ```sh
    docker-compose up -d --build

    ```

    L'opzione `--build` assicura che l'immagine `webapp` sia costruita con le ultime modifiche (incluso il `Program.cs` aggiornato per i test di persistenza, se non già fatto).

2. Controllare i log e lo stato:

    Attendere qualche secondo affinché i servizi si avviino, specialmente il database con il suo healthcheck.

    ```sh
    docker-compose ps

    ```

    Si dovrebbe vedere entrambi i servizi (webapp_service_compose e mariadb_service_compose) con stato Up o Up (healthy).

    Controllare i log per eventuali errori:

    ```sh
    docker-compose logs webapp
    docker-compose logs db

    ```

    Il `healthcheck` per `db` dovrebbe passare, e `webapp` dovrebbe avviarsi dopo aver ricevuto la notifica che `db` è healthy.

3. Accedere all'applicazione web:

    Aprire un browser e navigare all'URL http://localhost:${WEBAPP_HOST_PORT} (es. http://localhost:8088 se si usa il valore 8088 dal file .env di esempio). Si dovrebbe vedere la pagina index.html.

4. Testare la connessione al database:

    Navigare all'endpoint http://localhost:${WEBAPP_HOST_PORT}/dbtest. Si dovrebbe vedere il messaggio di successo della connessione a MariaDB, che include la versione del server.

5. Testare la persistenza dei dati:

    Per testare che i dati scritti nel database persistano anche dopo un riavvio dei container (ma non del volume), è necessario aggiungere degli endpoint all'applicazione MyWebApiApp che permettano di scrivere e leggere dati.

    **Modificare `MyWebApiApp/Program.cs`** per aggiungere due nuovi endpoint (se non già presenti dalla Sezione 5.5, qui sono leggermente migliorati):

    ```cs
    // MyWebApiApp/Program.cs
    // ... (using esistenti: MySql.Data.MySqlClient, System.Text) ...

    // ... (codice esistente per builder, app, UseDefaultFiles, UseStaticFiles, /hello, /dbtest) ...

    // Endpoint per creare una tabella (se non esiste) e inserire una voce
    app.MapPost("/entries", async (IConfiguration config) => {
        string connectionString = config.GetConnectionString("DefaultConnection");
        var response = new StringBuilder();
        response.AppendLine("Risultato Creazione Voce:");
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                // Crea la tabella se non esiste
                using (var commandCreateTable = connection.CreateCommand())
                {
                    commandCreateTable.CommandText = @"
                        CREATE TABLE IF NOT EXISTS TestEntries (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Message VARCHAR(255) NOT NULL,
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";
                    await commandCreateTable.ExecuteNonQueryAsync();
                    response.AppendLine("Tabella 'TestEntries' verificata/creata.");
                }

                // Inserisci una nuova voce
                using (var commandInsert = connection.CreateCommand())
                {
                    string messageContent = $"Voce creata via API alle {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}";
                    commandInsert.CommandText = "INSERT INTO TestEntries (Message) VALUES (@message);";
                    commandInsert.Parameters.AddWithValue("@message", messageContent);
                    int rowsAffected = await commandInsert.ExecuteNonQueryAsync();
                    response.AppendLine($"{rowsAffected} riga(e) inserita(e) con messaggio: '{messageContent}'.");
                }
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            response.AppendLine($"ERRORE: {ex.Message}");
        }
        return Results.Text(response.ToString());
    });

    // Endpoint per leggere tutte le voci dalla tabella
    app.MapGet("/entries", async (IConfiguration config) => {
        string connectionString = config.GetConnectionString("DefaultConnection");
        var entries = new StringBuilder();
        entries.AppendLine("Voci nel database 'TestEntries':\n");
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Message, CreatedAt FROM TestEntries ORDER BY CreatedAt DESC LIMIT 20;";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            entries.Append("Nessuna voce trovata.\n");
                        }
                        while (await reader.ReadAsync())
                        {
                            entries.Append($"ID: {reader.GetInt32(0)}, Messaggio: \"{reader.GetString(1)}\", CreatoIl: {reader.GetDateTime(2):yyyy-MM-dd HH:mm:ss}\n");
                        }
                    }
                }
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            entries.Append($"ERRORE: {ex.Message}\n");
            if (ex.Message.ToLower().Contains("testentries' doesn't exist")) {
                 entries.Append("Suggerimento: La tabella potrebbe non esistere. Prova a fare una richiesta POST a /entries per crearla e inserire la prima voce.\n");
            }
        }
        return Results.Text(entries.ToString());
    });

    app.Run();

    ```

    Passaggi per il test di persistenza:

    a. Ricostruire e riavviare lo stack (se Program.cs è stato modificato):

    ```sh
    docker-compose up -d --build
    ```

    Attendere che i servizi siano healthy.

    b. Inserire alcune voci:

    Usare uno strumento come Postman, curl, o semplicemente il browser (se si cambia MapPost in MapGet per /entries per un test rapido, ma POST è semanticamente corretto per creare risorse) per fare richieste POST all'endpoint http://localhost:${WEBAPP_HOST_PORT}/entries.

    Esempio con curl (eseguire più volte):

    ```sh
     curl -X POST http://localhost:8088/entries
    ```

    (Sostituire 8088 con la porta WEBAPP_HOST_PORT configurata).

    c. Visualizzare le voci inserite:

    Navigare nel browser a http://localhost:${WEBAPP_HOST_PORT}/entries. Si dovrebbero vedere le voci appena create.

    d. Fermare e rimuovere i container (MA NON I VOLUMI):

    ```sh
    docker-compose down
    ```

    Questo comando, senza l'opzione -v, ferma e rimuove i container e le reti, ma lascia intatti i volumi nominati (come db_data_compose).

    e. Riavviare lo stack:

    ```sh
    docker-compose up -d
    ```

    (Non è necessario --build se non ci sono state modifiche al codice o al Dockerfile). I servizi webapp e db verranno ricreati. Il servizio db utilizzerà il volume db_data_compose esistente.

    f. Verificare nuovamente le voci:

    Navigare di nuovo a http://localhost:${WEBAPP_HOST_PORT}/entries.

    Le voci inserite in precedenza dovrebbero essere ancora presenti! Questo dimostra che i dati nel database sono persistiti grazie al volume Docker nominato, anche dopo la rimozione e ricreazione dei container.

    g. Pulizia completa (opzionale):

    Se si vuole cancellare tutto, inclusi i dati nel volume del database:

    ```sh
    docker-compose down -v
    ```

Questo completa il caso pratico, dimostrando un'applicazione ASP.NET Core e un database MariaDB orchestrati con Docker Compose, con configurazione gestita tramite file `.env` e persistenza dei dati garantita da volumi nominati.

## 9. Considerazioni Finali e Prossimi Passi

Questo modulo ha fornito una base solida per la containerizzazione di applicazioni ASP.NET Core e la loro orchestrazione con Docker Compose, coprendo aspetti cruciali come la configurazione, la gestione dei segreti e la persistenza dei dati.

### 9.1. Strategie di gestione della configurazione per diversi ambienti (sviluppo, staging, produzione)

La configurazione di un'applicazione tipicamente varia significativamente tra i diversi ambienti di deployment.

- **Ambiente di Sviluppo (Development)**:

    - **File `appsettings.Development.json`**: Utilizzato da ASP.NET Core per configurazioni specifiche dello sviluppo (es. stringhe di connessione a DB locali, logging verboso, chiavi API di test).

    - **User Secrets**: Per segreti individuali dello sviluppatore, non committati.

    - **File `.env` locali per Docker Compose**: Per fornire variabili d'ambiente allo stack Docker Compose in esecuzione localmente. Questi file `.env` non vengono committati.

    - **Obiettivo**: Massima produttività dello sviluppatore, facilità di debug.

- **Ambiente di Staging/Test**:

    - **File `appsettings.Staging.json`** (o simile): Per configurazioni specifiche dell'ambiente di staging.

    - **Variabili d'ambiente fornite dalla Piattaforma di CI/CD o dall'Host di Staging**: Le configurazioni, inclusi i segreti, sono spesso iniettate come variabili d'ambiente dalla pipeline di build/deploy o dalla piattaforma di hosting.

    - **File `.env` specifici per Staging (gestiti dalla pipeline)**: In alcuni scenari, la pipeline di CI/CD potrebbe popolare un file `.env` sul server di staging.

    - **Sistemi di Gestione Segreti (istanza di test)**: Si potrebbe usare un'istanza di test di Azure Key Vault, HashiCorp Vault, ecc.

    - **Obiettivo**: Simulare l'ambiente di produzione il più fedelmente possibile, testare l'integrazione e le funzionalità prima del rilascio.

- **Ambiente di Produzione (Production)**:

    - **File `appsettings.Production.json`**: Per configurazioni non sensibili specifiche della produzione.

    - **Variabili d'ambiente**: Tutte le configurazioni che variano (endpoint di servizi, feature flags) e **soprattutto tutti i segreti** (password, API key, stringhe di connessione) DEVONO provenire da variabili d'ambiente sicure o, preferibilmente, da un **servizio di gestione dei segreti dedicato** (vedi Sezione 6.5).

    - Le variabili d'ambiente sono iniettate dall'orchestratore di container (Kubernetes, Docker Swarm) o dalla piattaforma di hosting PaaS (Azure App Service, AWS Elastic Beanstalk).

    - **Obiettivo**: Massima sicurezza, stabilità, prestazioni e monitoraggio.

Docker Compose Override Files:

Docker Compose supporta l'uso di file di override, tipicamente docker-compose.override.yml. Questo file, se presente nella stessa directory del docker-compose.yml, viene automaticamente unito alla configurazione principale.

È una pratica comune usare docker-compose.yml per la configurazione di base, comune a tutti gli ambienti, e docker-compose.override.yml per personalizzazioni specifiche dell'ambiente di sviluppo locale (es. montaggio di volumi per il codice sorgente per hot-reload, esposizione di porte diverse, abilitazione di strumenti di debug).

Il file docker-compose.override.yml non dovrebbe essere committato se contiene configurazioni specifiche della macchina o segreti locali (anche se i segreti dovrebbero comunque essere in .env). Spesso, si committa un docker-compose.override.yml.example.

Esempio di `docker-compose.override.yml` (per sviluppo locale):

```yml
# MyProjectRoot/docker-compose.override.yml (NON COMMETTERE se contiene dati sensibili/locali)
services:
  webapp:
    environment:
      ASPNETCORE_ENVIRONMENT: Development # Assicura che sia Development
      DOTNET_USE_POLLING_FILE_WATCHER: "true" # Utile per 'dotnet watch' in alcuni ambienti Docker
      # Potrebbe essere necessario cambiare l'entrypoint per usare 'dotnet watch run'
      # ENTRYPOINT: ["dotnet", "watch", "run", "--no-launch-profile"]
    ports:
      - "5001:8080" # Porta diversa per lo sviluppo locale sull'host
    volumes:
      # Monta l'intero progetto per hot-reload con 'dotnet watch'
      # Assicurarsi che il Dockerfile e l'entrypoint siano compatibili con questo.
      - ./MyWebApiApp:/app
      # Si potrebbe anche montare il pacchetto NuGet cache per velocizzare i restore
      # - ~/.nuget/packages:/root/.nuget/packages:ro
  db:
    ports:
      - "3308:3306" # Porta diversa per il db sull'host per evitare conflitti

```

Questo permette di mantenere il `docker-compose.yml` pulito e focalizzato sulla definizione dei servizi, mentre le personalizzazioni di sviluppo sono isolate.

### 9.2. Cenni su HTTPS all'interno dei container Docker (reverse proxy, certificati)

Finora, per semplicità, si è utilizzata la comunicazione HTTP. In produzione (e spesso anche in staging), **HTTPS è essenziale** per la sicurezza. Ci sono due approcci principali per gestire HTTPS con applicazioni containerizzate:

1. **Terminazione SSL/TLS all'interno del container ASP.NET Core (Kestrel)**:

    - È possibile configurare Kestrel per servire traffico su HTTPS direttamente.
    - Richiede:
        - Ottenere un certificato SSL/TLS valido (es. da un'autorità di certificazione come Let's Encrypt, o un certificato autofirmato per sviluppo).
        - Montare il file del certificato (es. `.pfx`) e la sua password (se protetto) all'interno del container in modo sicuro (es. tramite Docker Secrets o volumi sicuri).
        - Configurare Kestrel in `Program.cs` o tramite `appsettings.json`/variabili d'ambiente per utilizzare il certificato e ascoltare su una porta HTTPS (es. 443 o 8081 nel container). L'immagine base `mcr.microsoft.com/dotnet/aspnet:8.0` ascolta su HTTP (8080) e HTTPS (8081) di default, ma necessita che il certificato di sviluppo sia disponibile e trustato, o un certificato di produzione fornito.
    - **Svantaggi**: Può essere complesso gestire i certificati (rinnovi, distribuzione sicura) direttamente a livello di applicazione, specialmente con più istanze.
2. **Utilizzo di un Reverse Proxy (approccio più comune e raccomandato)**:

    - Un server reverse proxy (es. Nginx, Traefik, Caddy, YARP) viene eseguito davanti all'applicazione ASP.NET Core (spesso in un altro container sulla stessa rete Docker).

    - **Il reverse proxy gestisce la terminazione SSL/TLS**: riceve traffico HTTPS dal mondo esterno sulla porta 443, decripta il traffico, e poi inoltra le richieste all'applicazione ASP.NET Core su una connessione HTTP interna (es. sulla porta 8080 del container `webapp`).

    - **Vantaggi**:

        - **Semplifica la Configurazione dell'Applicazione**: L'applicazione ASP.NET Core può rimanere configurata per HTTP all'interno della rete Docker privata, ignara di HTTPS.
        - **Gestione Centralizzata dei Certificati**: Il reverse proxy si occupa di gestire i certificati SSL/TLS, inclusa l'integrazione con Let's Encrypt per certificati gratuiti e rinnovi automatici (Traefik e Caddy eccellono in questo).
        - **Funzionalità Aggiuntive**: I reverse proxy possono offrire load balancing tra più istanze dell'applicazione, caching, compressione Gzip, riscrittura di URL, limitazione del rate, intestazioni di sicurezza HTTP, ecc.
        - **Offloading SSL**: Libera l'applicazione dal carico computazionale della crittografia/decrittografia SSL/TLS.
    - **Esempio concettuale con un reverse proxy (Traefik) in `docker-compose.yml`**:

        ```yml
        # docker-compose.yml (estratto rilevante per Traefik)
        services:
          # ... webapp e db ...
          proxy:
            image: traefik:v2.10 # o nginx, caddy
            ports:
              - "80:80"   # Porta HTTP pubblica
              - "443:443" # Porta HTTPS pubblica
            volumes:
              - /var/run/docker.sock:/var/run/docker.sock:ro # Per Traefik, per scoprire i container
              # ... altre configurazioni per il proxy (certificati, regole di routing)
              - ./traefik.yml:/etc/traefik/traefik.yml # File di configurazione di Traefik
              - ./certs:/certs # Directory per i certificati (es. gestiti da Traefik/Let's Encrypt)
            networks:
              - app_network_compose # Assumendo che app_network_compose sia la rete definita
            # ... etichette (labels) sui servizi (es. webapp) per configurare Traefik ...

        ```

    - **Esempio concettuale con YARP (Yet Another Reverse Proxy) in `docker-compose.yml`** 🎯: YARP è una libreria .NET per costruire reverse proxy. In un setup Docker Compose, si avrebbe un servizio dedicato per YARP, che sarebbe esso stesso un'applicazione ASP.NET Core.

        ```yml
        # docker-compose.yml (estratto rilevante per YARP)
        services:
          webapp: # La tua applicazione ASP.NET Core principale
            build: ./MyWebApiApp
            container_name: my_actual_app_service
            networks:
              - app_network_compose
            # NOTA: 'webapp' non espone porte direttamente all'host,
            # ma solo alla rete interna per YARP.
            # La porta interna è definita nel Dockerfile di MyWebApiApp (es. 8080).
            # expose:
            #   - "8080"

          yarp-proxy:
            build:
              context: ./MyYarpProxyApp  # Directory del progetto ASP.NET Core per YARP
              dockerfile: Dockerfile    # Dockerfile specifico per l'app YARP
            container_name: yarp_reverse_proxy_service
            ports:
              - "80:8080"   # Mappa la porta 80 dell'host alla porta HTTP (interna 8080) di YARP
              - "443:8443"  # Mappa la porta 443 dell'host alla porta HTTPS (interna 8443) di YARP
            volumes:
              # Monta il file di configurazione di YARP (appsettings.json o un file dedicato)
              # È importante che YARP possa leggere la sua configurazione.
              - ./MyYarpProxyApp/appsettings.Production.json:/app/appsettings.Production.json:ro
              # Volume per i certificati SSL/TLS, se YARP gestisce la terminazione HTTPS
              - ./certs_yarp:/https/certs:ro
            environment:
              ASPNETCORE_ENVIRONMENT: Production
              # YARP ascolterà su queste porte all'interno del suo container
              ASPNETCORE_URLS: "http://+:8080;https://+:8443"
              # Variabili per configurare Kestrel per HTTPS in YARP
              Kestrel__Certificates__Default__Path: "/https/certs/my_yarp_cert.pfx"
              Kestrel__Certificates__Default__Password: "${YARP_CERT_PASSWORD_FROM_ENV}" # Caricata da un file .env
            networks:
              - app_network_compose
            depends_on:
              - webapp # YARP dipende dall'avvio di webapp

        networks:
          app_network_compose:
            driver: bridge

        # Eventuali volumi nominati non sono mostrati qui per brevità

        ```

        **Note per l'esempio YARP**:

        - `MyYarpProxyApp` sarebbe un **progetto ASP.NET Core separato** (minimo o più complesso) che include il pacchetto NuGet `Yarp.ReverseProxy`.
        - Il suo `Program.cs` configurerebbe YARP aggiungendo i servizi necessari e caricando la configurazione delle rotte e dei cluster, tipicamente da `appsettings.json`.
        - L'`appsettings.Production.json` (o un file di configurazione simile) montato nel container `yarp-proxy` conterrebbe la configurazione specifica di YARP. Ad esempio, per inoltrare tutto il traffico a `webapp`:JSON

            ```json
            {
              "ReverseProxy": {
                "Routes": {
                  "appRoute": { // Nome della rotta
                    "ClusterId": "appCluster", // ID del cluster di destinazione
                    "Match": {
                      "Path": "/{**catch-all}" // Inoltra tutte le richieste
                    }
                  }
                },
                "Clusters": {
                  "appCluster": { // Definizione del cluster
                    "Destinations": {
                      "destination1": {
                        // "webapp" è il nome del servizio della tua app principale
                        // definito in docker-compose.yml. Docker Compose e la rete
                        // interna risolveranno 'webapp' all'IP del container corretto.
                        // La porta 8080 è quella su cui MyWebApiApp ascolta all'interno
                        // della rete Docker (come da suo Dockerfile).
                        "Address": "http://webapp:8080/"
                      }
                    }
                  }
                }
              }
            }

            ```

        - La **gestione dei certificati SSL/TLS** per la porta 443 esposta da YARP (qui mappata alla 8443 interna di YARP) dovrebbe essere configurata. L'esempio mostra come Kestrel nel container YARP potrebbe essere configurato per usare un certificato `.pfx` montato tramite un volume. La password del certificato verrebbe fornita tramite una variabile d'ambiente (es. `YARP_CERT_PASSWORD_FROM_ENV`), che a sua volta sarebbe caricata da un file `.env` per sicurezza.
        - Questo approccio con YARP offre una **grande flessibilità** poiché tutta la logica del proxy (routing avanzato, trasformazioni delle richieste/risposte, bilanciamento del carico, autenticazione/autorizzazione a livello di proxy) può essere implementata direttamente in C# all'interno del progetto `MyYarpProxyApp`.

### 9.3. Introduzione al debugging di applicazioni containerizzate

Debuggare un'applicazione .NET in esecuzione all'interno di un container può sembrare complesso, ma gli strumenti moderni lo facilitano:

- **Supporto IDE (Visual Studio, Visual Studio Code, JetBrains Rider)**:

    - Questi IDE offrono un eccellente supporto integrato per il debug di applicazioni .NET containerizzate.

    - Tipicamente, richiedono che il `Dockerfile` sia configurato per il debug. Questo spesso implica:

        - Utilizzare l'immagine base dell'SDK .NET (non solo quella del runtime) nella fase finale per il debug.

        - Installare gli strumenti di debug remoto di .NET (`vsdbg` per Visual Studio/VS Code) all'interno del container.

        - Mantenere i simboli di debug (`.pdb`).

    - L'IDE si occupa di avviare il container (o di collegarsi a uno esistente), deployare gli strumenti di debug, e collegare il debugger al processo .NET in esecuzione all'interno del container. Si possono poi usare breakpoint, ispezionare variabili, ecc., come nel debug locale tradizionale.

    - Visual Studio, quando si aggiunge "Supporto Docker", può generare automaticamente un `Dockerfile.debug` (o configurazioni simili) e profili di avvio per il debug containerizzato.

- **Log dei Container**: Il primo strumento di diagnosi.

    - `docker logs <container_id_or_name>`

    - `docker-compose logs <service_name>` (con `-f` per seguire in tempo reale).

    - Assicurarsi che l'applicazione ASP.NET Core scriva log utili sulla console.

- **Accesso alla Shell del Container**:

    - `docker exec -it <container_id_or_name> sh` (o `bash`)

    - `docker-compose exec <service_name> sh`

    - Permette di ispezionare il filesystem del container, controllare i processi in esecuzione, testare la connettività di rete dall'interno del container, esaminare file di configurazione.

- **`dotnet watch` per Sviluppo Rapido**:

    - Per lo sviluppo, si può modificare il `Dockerfile` (o usare un `Dockerfile.dev`) per usare `dotnet watch run` come `ENTRYPOINT`.

    - Combinato con un bind mount del codice sorgente nel container (come visto nel `docker-compose.override.yml`), questo permette la ricompilazione e il riavvio automatico dell'applicazione all'interno del container ogni volta che si modifica un file sorgente sull'host. Questo accelera notevolmente il ciclo di sviluppo iterativo.

- **Strumenti di Diagnostica .NET**:

    - `dotnet-counters`, `dotnet-trace`, `dotnet-dump`: Questi strumenti globali della CLI .NET possono, in alcuni scenari e con la giusta configurazione, essere usati per monitorare le prestazioni o diagnosticare problemi in processi .NET in esecuzione nei container.

### 9.4. Panoramica delle opzioni di deployment in produzione

Mentre Docker Compose è eccellente per lo sviluppo, il test e anche per deployment semplici in produzione, per applicazioni più grandi, critiche, o che richiedono alta disponibilità, scalabilità avanzata e gestione complessa, si utilizzano orchestratori di container più potenti:

- **Kubernetes (K8s)**:

    - È diventato lo standard de-facto per l'orchestrazione di container su larga scala.

    - Offre funzionalità avanzate come: auto-scaling (scalabilità automatica orizzontale e verticale dei pod), self-healing (riavvio automatico di container falliti), rolling updates e rollback, gestione dichiarativa della configurazione e dei segreti, service discovery e load balancing complessi, gestione dello storage persistente, e molto altro.

    - Ha una curva di apprendimento più ripida di Docker Compose.

    - Disponibile come servizio gestito dai principali cloud provider:

        - **Azure Kubernetes Service (AKS)** su Microsoft Azure.

        - **Amazon Elastic Kubernetes Service (EKS)** su AWS.

        - **Google Kubernetes Engine (GKE)** su Google Cloud.

    - Può anche essere installato e gestito on-premise.

- **Docker Swarm**:

    - È la soluzione di orchestrazione nativa di Docker, integrata nel Docker Engine.

    - È più semplice da configurare e gestire rispetto a Kubernetes, ma offre un set di funzionalità meno esteso.

    - Adatto per casi d'uso meno complessi o per team che desiderano una soluzione di orchestrazione più leggera.

    - Utilizza concetti simili a Docker Compose (gli stack Swarm possono essere deployati da file `docker-compose.yml` con alcune estensioni specifiche per Swarm).

- Piattaforme di Container as a Service (CaaS) e Serverless Containers su Cloud:

    Queste piattaforme astraggono ulteriormente la gestione dell'infrastruttura sottostante.

    - **Azure App Service (for Containers)**: Permette di deployare applicazioni web containerizzate (Linux o Windows) senza doversi preoccupare dei server. Offre scalabilità, bilanciamento del carico, slot di deployment, integrazione con CI/CD.

    - **Azure Container Instances (ACI)**: Per eseguire singoli container o gruppi semplici di container rapidamente, senza un orchestratore completo. Utile per task batch, API semplici.

    - **AWS Elastic Container Service (ECS)**: Servizio di orchestrazione di container completamente gestito da AWS. Può essere usato con istanze EC2 (controllo sull'infrastruttura) o con AWS Fargate.

    - **AWS Fargate**: Un "compute engine serverless" per container. Con Fargate (usato con ECS o EKS), non è necessario provisionare o gestire server; si paga solo per le risorse consumate dai container.

    - **Google Cloud Run**: Piattaforma completamente gestita per eseguire container stateless che scalano automaticamente (anche a zero). Si paga solo quando il codice è in esecuzione. Ideale per API e microservizi.

- **Deployment On-Premise**:

    - Esecuzione di Kubernetes, Docker Swarm, o altre piattaforme di orchestrazione (es. Red Hat OpenShift) sui propri server fisici o macchine virtuali nel proprio data center. Richiede una maggiore competenza operativa per la gestione dell'infrastruttura.

La scelta della piattaforma di deployment in produzione dipende da molti fattori: la complessità e i requisiti di scalabilità dell'applicazione, le competenze del team, il budget, l'ecosistema cloud preferito, e i requisiti di conformità e sicurezza.

Indipendentemente dalla scelta, le immagini Docker costruite (sia con Dockerfile che con dotnet publish /t:PublishContainer) rimangono l'artefatto di deployment fondamentale, garantendo portabilità e consistenza.

[^1]: Lo "staging" di un'applicazione software, o di un sito web, si riferisce alla creazione di un ambiente di test, che è una replica (o una versione di prova) dell'ambiente di produzione, per permettere agli sviluppatori di testare modifiche e aggiornamenti senza influenzare l'ambiente live. In pratica, lo staging è un "sandbox" sicuro dove sperimentare senza rischi per i utenti finali.