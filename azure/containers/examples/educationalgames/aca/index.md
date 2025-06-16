# Guida Passo Passo al Deployment su Azure Container Apps (ACA) - Esempio `EducationalGames` 🛠️

- [Guida Passo Passo al Deployment su Azure Container Apps (ACA) - Esempio `EducationalGames` 🛠️](#guida-passo-passo-al-deployment-su-azure-container-apps-aca---esempio-educationalgames-️)
  - [Prerequisiti](#prerequisiti)
  - [Passo 1: Creare un Azure Container Registry (ACR)](#passo-1-creare-un-azure-container-registry-acr)
  - [Passo 2: Preparare ed effettuare la Push di Immagini Docker su ACR](#passo-2-preparare-ed-effettuare-la-push-di-immagini-docker-su-acr)
  - [Passo 3: Creare un Ambiente Azure Container Apps (ACA Environment)](#passo-3-creare-un-ambiente-azure-container-apps-aca-environment)
  - [Passo 4: Creare una Condivisione File di Azure (Azure Files) per la Persistenza di MariaDB 📁](#passo-4-creare-una-condivisione-file-di-azure-azure-files-per-la-persistenza-di-mariadb-)
    - [Come Funziona Azure Files](#come-funziona-azure-files)
    - [Passo 4.1: Creare una Condivisione File di Azure per le Chiavi di Data Protection della WebApp 🔑](#passo-41-creare-una-condivisione-file-di-azure-per-le-chiavi-di-data-protection-della-webapp-)
  - [Passo 5: Definire e Gestire i Segreti in ACA 🤫](#passo-5-definire-e-gestire-i-segreti-in-aca-)
  - [Passo 6: Distribuire l'App Container per MariaDB 💾](#passo-6-distribuire-lapp-container-per-mariadb-)
  - [Gestione dello Scaling Orizzontale, Bilanciamento del Carico e Stato dell'Applicazione in ACA](#gestione-dello-scaling-orizzontale-bilanciamento-del-carico-e-stato-dellapplicazione-in-aca)
    - [Scalabilità Orizzontale e Bilanciamento del Carico Automatici](#scalabilità-orizzontale-e-bilanciamento-del-carico-automatici)
    - [Affinità di Sessione (Session Affinity)](#affinità-di-sessione-session-affinity)
    - [Condivisione delle Chiavi di Data Protection di ASP.NET Core](#condivisione-delle-chiavi-di-data-protection-di-aspnet-core)
    - [Configurazione Applicativa (`Program.cs`) per Data Protection:](#configurazione-applicativa-programcs-per-data-protection)
  - [Passo 7: Distribuire l'App Container per la WebApp ASP.NET Core 🌐](#passo-7-distribuire-lapp-container-per-la-webapp-aspnet-core-)
  - [Passo 8: Configurazione del Dominio Personalizzato 🔖](#passo-8-configurazione-del-dominio-personalizzato-)
    - [Gestione dei Certificati TLS/SSL in ACA](#gestione-dei-certificati-tlsssl-in-aca)
    - [Procedura di Configurazione](#procedura-di-configurazione)
  - [Passo 9: Configurazione Porte per Invio Email (MailKit con SMTP Google) 📧](#passo-9-configurazione-porte-per-invio-email-mailkit-con-smtp-google-)
  - [Passo 10: Configurazione Accesso Autenticato con Google e Microsoft Entra ID 🔐](#passo-10-configurazione-accesso-autenticato-con-google-e-microsoft-entra-id-)
  - [Passo 11: Verifica e Troubleshooting ✅](#passo-11-verifica-e-troubleshooting-)
  - [Conclusione](#conclusione)

Questa guida è stata sviluppata per accompagnare il lettore nella distribuzione di un'applicazione ASP.NET Core e MariaDB su `Azure Container Apps (ACA)`. In particolare, nell'esempio di codice fornito, verrà utilizzato il progetto `EducationalGames` per effettuare il deployment su Azure Container Apps.

## Prerequisiti

- **Account Azure:** Con una sottoscrizione attiva.

- **Azure CLI:** Installata e configurata. Si esegue `az login` per l'autenticazione.

- **Docker Desktop:** Installato se è necessario buildare le immagini Docker localmente.

- **Immagini Docker:**

    - L'immagine Docker per la `webapp` ASP.NET Core (basata sul `Program.cs` refactored per la gestione flessibile delle Data Protection Keys).

    - L'immagine Docker ufficiale di `mariadb` (es. `mariadb:11.4` o una versione specifica).

- **File `.env` (per sviluppo locale):** Contenente i segreti e le configurazioni. Per ACA, questi valori verranno configurati come segreti e variabili d'ambiente del servizio.

- **Un dominio personalizzato (opzionale):** Se si desidera configurare un dominio personalizzato.

## Passo 1: Creare un Azure Container Registry (ACR)

ACR è un registro Docker privato in Azure dove è possibile archiviare e gestire le immagini container.

```sh
# Variabili (personalizzare questi valori)
RESOURCE_GROUP="myACAResourceGroup" # Gruppo di Risorse per tutti i servizi
LOCATION="italynorth" # Scegliere una region Azure
ACR_NAME="myuniqueacrname$RANDOM" # Nome univoco per l'ACR

# Crea un Gruppo di Risorse (se non esiste già)
az group create --name $RESOURCE_GROUP --location $LOCATION

# Crea Azure Container Registry (SKU Basic è sufficiente per iniziare)
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic --admin-enabled true

# Ottieni le credenziali di accesso (necessarie se non si usa l'identità gestita per il pull da ACA)
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
# ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username --output tsv) # Opzionale se si usano credenziali ACR
# ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value --output tsv) # Opzionale

echo "ACR Login Server: $ACR_LOGIN_SERVER"
```

## Passo 2: Preparare ed effettuare la Push di Immagini Docker su ACR

1. **Login ad ACR (dalla macchina locale dove è installato Docker):**

    ```sh
    az acr login --name $ACR_NAME
    ```

2. Effettuare la build dell'immagine della WebApp:

    Assicurarsi che il Dockerfile della webapp sia corretto e che il Program.cs sia stato aggiornato per la gestione flessibile delle Data Protection Keys e per leggere la configurazione nel formato Sezione:SottoSezione:Chiave.

    ```sh
    # Navigare nella directory del Dockerfile della webapp (es. ./EducationalGames/EducationalGames)
    # cd path/to/your/webapp/project
    docker build -t webapp:latest .
    ```

3. **Taggare le immagini per ACR:**

    ```sh
    # Effettua il tagging della webapp
    docker tag webapp:latest $ACR_LOGIN_SERVER/webapp:latest
    ```

4. **Effettuare la Push dell'immagine della webapp su ACR:**

    ```sh
    docker push $ACR_LOGIN_SERVER/webapp:latest
    ```

## Passo 3: Creare un Ambiente Azure Container Apps (ACA Environment)

L'ambiente ACA fornisce un confine di rete isolato per le app container.

```sh
# Variabili
ACA_ENV_NAME="myACAEnvironment"
LOG_ANALYTICS_WORKSPACE_NAME="myACALogAnalytics-$RANDOM"

# Crea un Log Analytics Workspace (necessario per l'ambiente ACA)
az monitor log-analytics workspace create\
  --resource-group $RESOURCE_GROUP\
  --location $LOCATION\
  --workspace-name $LOG_ANALYTICS_WORKSPACE_NAME

LOG_ANALYTICS_CLIENT_ID=$(az monitor log-analytics workspace show --query customerId -g $RESOURCE_GROUP -n $LOG_ANALYTICS_WORKSPACE_NAME --out tsv)
LOG_ANALYTICS_CLIENT_SECRET=$(az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $RESOURCE_GROUP -n $LOG_ANALYTICS_WORKSPACE_NAME --out tsv)

# Crea l'Ambiente Azure Container Apps
az containerapp env create\
  --name $ACA_ENV_NAME\
  --resource-group $RESOURCE_GROUP\
  --location $LOCATION\
  --logs-workspace-id $LOG_ANALYTICS_CLIENT_ID\
  --logs-workspace-key $LOG_ANALYTICS_CLIENT_SECRET
```

## Passo 4: Creare una Condivisione File di Azure (Azure Files) per la Persistenza di MariaDB 📁

MariaDB necessita di storage persistente per i suoi dati. In questo contesto, Azure Files gioca un ruolo cruciale.

### Come Funziona Azure Files

Azure Files è un servizio di Azure che offre condivisioni di file completamente gestite nel cloud, accessibili tramite protocolli standard come Server Message Block (SMB) e Network File System (NFS). Per le applicazioni containerizzate come quella descritta in questa guida (in particolare per MariaDB), Azure Files agisce come uno storage di rete persistente.

- **Persistenza dei Dati:** I container, per loro natura, possono essere effimeri (cioè possono essere fermati, avviati, o spostati). Se i dati di un database come MariaDB fossero memorizzati solo all'interno del file system del container, andrebbero persi ogni volta che il container viene ricreato o riavviato. Azure Files risolve questo problema fornendo una posizione di storage esterna e duratura.

- **Montaggio come Volume:** L'app container di MariaDB monterà una condivisione Azure Files come un volume direttamente nel suo file system (ad esempio, nel percorso `/var/lib/mysql` dove MariaDB si aspetta di trovare i suoi file di dati). Ciò significa che MariaDB legge e scrive i suoi dati direttamente sulla condivisione Azure Files, proprio come farebbe su un disco locale.

- **Accesso e Condivisione:** Sebbene in questo scenario la condivisione sia usata principalmente da una singola istanza di MariaDB (o dalle sue repliche, se scalata), Azure Files supporta l'accesso simultaneo da più client, rendendolo versatile anche per altri casi d'uso.

- **Gestione Semplificata:** Azure si occupa della gestione dell'hardware sottostante, dell'applicazione di patch e della manutenzione, riducendo l'onere operativo.

In sintesi, usando Azure Files, si garantisce che i dati del database MariaDB persistano indipendentemente dal ciclo di vita del container, assicurando la durabilità e la disponibilità dei dati.

Per informazioni dettagliate sui costi di Azure Files, è possibile consultare la pagina ufficiale:

- [Prezzi di File di Azure](https://azure.microsoft.com/it-it/pricing/details/storage/files/ "null")

Ora, si procede con la creazione della condivisione file.

```sh
# Variabili
STORAGE_ACCOUNT_NAME="mystorageaccount$RANDOM" # Nome univoco per l'account di storage
FILE_SHARE_NAME_MARIADB="mariadb-data" # Nome della condivisione file per MariaDB

# Crea un Account di Storage (se non ne hai già uno che vuoi riutilizzare)
az storage account create\
  --name $STORAGE_ACCOUNT_NAME\
  --resource-group $RESOURCE_GROUP\
  --location $LOCATION\
  --sku Standard_LRS\
  --kind StorageV2

# Ottieni la chiave dell'account di storage (necessaria per montare la condivisione)
STORAGE_ACCOUNT_KEY=$(az storage account keys list --resource-group $RESOURCE_GROUP --account-name $STORAGE_ACCOUNT_NAME --query "[0].value" --output tsv)

# Crea la Condivisione File per MariaDB
az storage share create\
  --name $FILE_SHARE_NAME_MARIADB\
  --account-name $STORAGE_ACCOUNT_NAME\
  --account-key $STORAGE_ACCOUNT_KEY\
  --quota 5 # Dimensione in GB, adattare secondo necessità

echo "Storage Account Name: $STORAGE_ACCOUNT_NAME"
echo "File Share Name for MariaDB: $FILE_SHARE_NAME_MARIADB"
echo "Storage Account Key: $STORAGE_ACCOUNT_KEY" # Salvare questa chiave, verrà usata come segreto
```

### Passo 4.1: Creare una Condivisione File di Azure per le Chiavi di Data Protection della WebApp 🔑

Per garantire che l'autenticazione basata su cookie funzioni correttamente in un ambiente con più istanze della webapp (scalabilità orizzontale), le chiavi di Data Protection di ASP.NET Core devono essere condivise tra tutte le istanze. Azure Files può essere utilizzato anche per questo scopo.

```sh
# Variabili
# Si riutilizza lo stesso STORAGE_ACCOUNT_NAME e STORAGE_ACCOUNT_KEY del Passo 4.
FILE_SHARE_NAME_DATAPROTECTION="webapp-dataprotection-keys"

# Crea la Condivisione File per le Data Protection Keys
az storage share create\
  --name $FILE_SHARE_NAME_DATAPROTECTION\
  --account-name $STORAGE_ACCOUNT_NAME\
  --account-key $STORAGE_ACCOUNT_KEY\
  --quota 1 # 1 GB è più che sufficiente per le chiavi

echo "File Share Name for Data Protection Keys: $FILE_SHARE_NAME_DATAPROTECTION"
```

## Passo 5: Definire e Gestire i Segreti in ACA 🤫

È necessario tradurre le variabili sensibili in segreti ACA. I segreti in ACA sono a livello di app.

I segreti principali da gestire per l'applicazione "EducationalGames" includono (i nomi dei segreti in ACA sono minuscoli e con trattini):

- `mariadb-root-password`: La password di root per MariaDB.

- `azure-storage-account-key`: La chiave dell'account di storage Azure Files.

- `smtp-password`: La password per l'account SMTP (usata per `EmailSettings:Password`).

- `auth-google-clientid`: Il Client ID per Google (usato per `Authentication:Google:ClientId`).

- `auth-google-clientsecret`: Il Client Secret per Google (usato per `Authentication:Google:ClientSecret`).

- `auth-microsoft-clientid`: Il Client ID per Microsoft (usato per `Authentication:Microsoft:ClientId`).

- `auth-microsoft-clientsecret`: Il Client Secret per Microsoft (usato per `Authentication:Microsoft:ClientSecret`).

Questi verranno definiti nel parametro `--secrets` del comando `az containerapp create` e poi referenziati nelle variabili d'ambiente con `secretref:NOME_SEGRETO_ACA`.

Le altre configurazioni da `appsettings.json` che non sono strettamente segreti (come `EmailSettings:Server`, `Authentication:Microsoft:TenantId`, ecc.) verranno passate direttamente come variabili d'ambiente nel Passo 7.

## Passo 6: Distribuire l'App Container per MariaDB 💾

```sh
# Variabili
MARIADB_APP_NAME="mariadb-server"
MARIADB_IMAGE="mariadb:11.4"
MARIADB_PASSWORD_VALUE="ChangeThisSecurePassword123!" # SOSTITUIRE con una password robusta

# Crea l'app container per MariaDB
az containerapp create\
  --name $MARIADB_APP_NAME\
  --resource-group $RESOURCE_GROUP\
  --environment $ACA_ENV_NAME\
  --image $MARIADB_IMAGE\
  --min-replicas 0 \ # Per risparmio costi in demo; considerare 1 per produzione
  --max-replicas 1\
  --secrets\
    mariadb-root-password="$MARIADB_PASSWORD_VALUE"\
    azure-storage-account-key="$STORAGE_ACCOUNT_KEY"\
  --env-vars\
    MYSQL_ROOT_PASSWORD=secretref:mariadb-root-password\
  --azure-file-volume-account-name $STORAGE_ACCOUNT_NAME\
  --azure-file-volume-account-key secretref:azure-storage-account-key\
  --azure-file-volume-share-name $FILE_SHARE_NAME_MARIADB\
  --azure-file-volume-mount-path /var/lib/mysql\
  --target-port 3306\
  --ingress internal\
  --cpu 0.5\
  --memory 1Gi
```

- :memo: **Nota sulla Scalabilità a Zero per MariaDB (--min-replicas 0)**:

    Impostare il numero minimo di repliche a 0 per MariaDB permette di massimizzare il risparmio sui costi durante i periodi di inattività. Tuttavia, è fondamentale comprendere le implicazioni:

    - **Latenza di Cold Start Significativa:** La prima richiesta al database dopo un periodo di inattività subirà un ritardo mentre l'istanza si avvia.

    - **Resilienza della WebApp:** La webapp dovrebbe gestire potenziali timeout iniziali.

    - **Persistenza dei Dati:** I dati sono sicuri su Azure Files.

    - **Uso in Demo vs. Produzione:** Accettabile per demo se la latenza è compresa; sconsigliato per database primari in produzione.

## Gestione dello Scaling Orizzontale, Bilanciamento del Carico e Stato dell'Applicazione in ACA

Prima di distribuire la webapp, è importante comprendere come Azure Container Apps gestisce la scalabilità e come configurare l'applicazione ASP.NET Core per funzionare correttamente in un ambiente multi-istanza.

### Scalabilità Orizzontale e Bilanciamento del Carico Automatici

Azure Container Apps è progettato per la scalabilità. Quando si imposta `min-replicas` e `max-replicas`, ACA gestisce automaticamente:

- **Scalabilità Orizzontale (Horizontal Scaling):** Aumenta (scale-out) o diminuisce (scale-in) il numero di istanze (repliche) del container della webapp in base a regole di scalabilità.

- **Bilanciamento del Carico (Load Balancing):** ACA include un bilanciatore del carico gestito e integrato. Quando ci sono più istanze della webapp in esecuzione, il traffico in ingresso (richieste HTTP/HTTPS) viene distribuito automaticamente tra queste istanze attive. **La strategia di bilanciamento predefinita e unica è il Round Robin**, che distribuisce le richieste in modo sequenziale tra le repliche integre disponibili. Non è possibile configurare altre strategie come Least Connections direttamente sul bilanciatore di ACA, poiché è un servizio completamente gestito.

### Affinità di Sessione (Session Affinity)

Nonostante il bilanciamento del carico sia Round Robin, è possibile influenzare il modo in cui il traffico viene dirottato verso una specifica istanza per un dato utente. Per applicazioni che utilizzano l'autenticazione basata su cookie, come "EducationalGames", ACA offre l'**affinità di sessione** (nota anche come "sticky sessions").

- **Come Funziona:** Quando l'affinità di sessione è abilitata, il bilanciatore del carico di ACA aggiunge un cookie alla prima risposta inviata a un client. Le richieste successive da quel client che includono questo cookie vengono instradate alla stessa istanza dell'applicazione per la durata della sessione.

- **Quando Abilitarla:** È cruciale per applicazioni che mantengono uno stato in memoria per ogni sessione. Anche se le Data Protection Keys sono condivise, abilitare l'affinità di sessione può migliorare le prestazioni evitando che ogni richiesta venga potenzialmente gestita da un'istanza diversa.

- **Come si abilita:** Si configura a livello di ingress dell'app container con il parametro `--enable-session-affinity`.

### Condivisione delle Chiavi di Data Protection di ASP.NET Core

Perché l'autenticazione basata su cookie funzioni tra più istanze, le chiavi di Data Protection devono essere condivise. Si utilizzerà la condivisione Azure Files creata nel "Passo 4.1".

### Configurazione Applicativa (`Program.cs`) per Data Protection:

Il file `Program.cs` dell'applicazione "EducationalGames" deve essere configurato per leggere il percorso delle chiavi di Data Protection da una variabile d'ambiente (`DATA_PROTECTION_KEYS_PATH`) quando non in sviluppo. Per ACA, questa variabile d'ambiente verrà impostata su `/mnt/dataprotectionkeys` (il path di montaggio della condivisione Azure Files).

## Passo 7: Distribuire l'App Container per la WebApp ASP.NET Core 🌐

Ora si distribuisce la webapp, configurando segreti, variabili d'ambiente e volumi.

```sh
# Variabili
WEBAPP_NAME="myaspnetapp"
WEBAPP_IMAGE="$ACR_LOGIN_SERVER/webapp:latest" # Immagine buildata con Program.cs aggiornato

# Valori Esempio per i segreti e variabili (SOSTITUIRE CON I PROPRI VALORI REALI)
# MARIADB_PASSWORD_VALUE è già definita
# STORAGE_ACCOUNT_KEY è già stata ottenuta
SMTP_PASSWORD_VALUE="your_actual_smtp_app_password"
AUTH_GOOGLE_CLIENTID_VALUE="your_actual_google_client_id"
AUTH_GOOGLE_CLIENTSECRET_VALUE="your_actual_google_client_secret"
AUTH_MICROSOFT_CLIENTID_VALUE="your_actual_microsoft_client_id"
AUTH_MICROSOFT_CLIENTSECRET_VALUE="your_actual_microsoft_client_secret"
MICROSOFT_TENANT_ID_VALUE="your_actual_microsoft_tenant_id"

# Valori per variabili d'ambiente non segrete
SMTP_SENDER_EMAIL_VALUE="your_sender_email@example.com"
SMTP_USERNAME_VALUE="your_smtp_username@example.com"

# Stringa di connessione per MariaDB (dal Program.cs: "EducationalGamesConnection")
DB_USER="root"
DB_NAME="educationalgamesdb" # Nome del database come da appsettings.json
CONNECTION_STRING_VALUE="Server=mariadb-server;Port=3306;Database=$DB_NAME;Uid=$DB_USER;Pwd=secretref:mariadb-root-password;AllowPublicKeyRetrieval=true;Pooling=true;"

# Crea l'app container per la WebApp
az containerapp create\
  --name $WEBAPP_NAME\
  --resource-group $RESOURCE_GROUP\
  --environment $ACA_ENV_NAME\
  --image $WEBAPP_IMAGE\
  --registry-server $ACR_LOGIN_SERVER\
  --registry-username $(az acr credential show --name $ACR_NAME --query username --output tsv)\
  --registry-password $(az acr credential show --name $ACR_NAME --query passwords[0].value --output tsv)\
  --min-replicas 0\
  --max-replicas 3\
  --secrets\
    mariadb-root-password="$MARIADB_PASSWORD_VALUE"\
    azure-storage-account-key="$STORAGE_ACCOUNT_KEY"\
    smtp-password="$SMTP_PASSWORD_VALUE"\
    auth-google-clientid="$AUTH_GOOGLE_CLIENTID_VALUE"\
    auth-google-clientsecret="$AUTH_GOOGLE_CLIENTSECRET_VALUE"\
    auth-microsoft-clientid="$AUTH_MICROSOFT_CLIENTID_VALUE"\
    auth-microsoft-clientsecret="$AUTH_MICROSOFT_CLIENTSECRET_VALUE"\
  --env-vars\
    ASPNETCORE_ENVIRONMENT=Production\
    ASPNETCORE_URLS=http://+:8080\
    DATA_PROTECTION_KEYS_PATH=/mnt/dataprotectionkeys\
    "ConnectionStrings__EducationalGamesConnection=$CONNECTION_STRING_VALUE"\
    EmailSettings__Server="smtp.gmail.com"\
    EmailSettings__Port="587"\
    EmailSettings__SenderName="Educational Games Staff"\
    "EmailSettings__SenderEmail=$SMTP_SENDER_EMAIL_VALUE"\
    "EmailSettings__Username=$SMTP_USERNAME_VALUE"\
    EmailSettings__Password=secretref:smtp-password\
    Authentication__Google__ClientId=secretref:auth-google-clientid\
    Authentication__Google__ClientSecret=secretref:auth-google-clientsecret\
    Authentication__Microsoft__ClientId=secretref:auth-microsoft-clientid\
    Authentication__Microsoft__ClientSecret=secretref:auth-microsoft-clientsecret\
    "Authentication__Microsoft__TenantId=$MICROSOFT_TENANT_ID_VALUE"\
  --azure-file-volume-account-name $STORAGE_ACCOUNT_NAME\
  --azure-file-volume-account-key secretref:azure-storage-account-key\
  --azure-file-volume-share-name $FILE_SHARE_NAME_DATAPROTECTION\
  --azure-file-volume-mount-path /mnt/dataprotectionkeys\
  --target-port 8080\
  --ingress external\
  --enable-session-affinity\
  --cpu 0.5\
  --memory 1Gi\
  --transport http1

# Ottieni l'URL pubblico della webapp
WEBAPP_URL=$(az containerapp show --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP --query properties.configuration.ingress.fqdn --output tsv)
echo "La WebApp è disponibile su: https://$WEBAPP_URL"
```

## Passo 8: Configurazione del Dominio Personalizzato 🔖

Quando si configura un dominio personalizzato per un'applicazione web distribuita su Azure Container Apps, è fondamentale gestire anche il certificato TLS/SSL per abilitare HTTPS e garantire comunicazioni sicure.

### Gestione dei Certificati TLS/SSL in ACA

Azure Container Apps offre due approcci principali per la gestione dei certificati:

1. **Certificato gestito da Container Apps (Opzione Consigliata per Semplicità):**

    - Azure Container Apps può **creare e gestire automaticamente un certificato TLS/SSL gratuito** per il dominio personalizzato.

    - Questo significa che Azure si occupa del provisioning del certificato, della sua installazione e, cosa molto importante, del suo **rinnovo automatico** prima della scadenza.

    - Questa opzione semplifica notevolmente la configurazione e la manutenzione di HTTPS.

    - Quando si aggiunge un dominio personalizzato e si valida la proprietà, si può scegliere questa opzione.

2. **Caricare un Certificato Personalizzato:**

    - Se si possiede già un certificato TLS/SSL, è possibile caricarlo (solitamente in formato `.pfx`).

    - In questo caso, l'utente è responsabile del rinnovo e del caricamento.

Per la demo, l'utilizzo di un **certificato gestito da Container Apps** è la via più diretta.

### Procedura di Configurazione

1. **Prerequisiti:** Nome di dominio registrato e accesso alla configurazione DNS.

2. **Aggiungere i Record DNS:** Configurare i record `CNAME` e `TXT` forniti da ACA nel proprio provider DNS.

3. **Aggiungere il Dominio Personalizzato e Configurare il Certificato in ACA:** Nel portale Azure, aggiungere il dominio all'app container, validarlo e scegliere l'opzione per un **"Certificato gestito da Container Apps"**.

4. **Aggiornare gli URI di redirect per l'autenticazione:** Aggiornare gli URI di redirect nelle console Google/Microsoft con il nuovo dominio personalizzato.

## Passo 9: Configurazione Porte per Invio Email (MailKit con SMTP Google) 📧

Per inviare email tramite MailKit usando server SMTP esterni come Google, solitamente **non è richiesta alcuna configurazione specifica a livello di infrastruttura di rete in ACA per le connessioni in uscita.**

**Cosa è necessario assicurare a livello di applicazione:**

1. **Configurazione MailKit:** Corretta configurazione in ASP.NET Core con server (`smtp.gmail.com`), porta (`587` o `465`), e credenziali (indirizzo email e **Password per le app** di Google, conservata come segreto ACA).

2. **Codice ASP.NET Core:** Utilizzo corretto di `SmtpClient` con le impostazioni e credenziali appropriate.

## Passo 10: Configurazione Accesso Autenticato con Google e Microsoft Entra ID 🔐

Si utilizzerà l'**Opzione A: Autenticazione gestita dall'applicazione ASP.NET Core**, poiché offre maggiore controllo ed è più completa didatticamente, specialmente con la gestione delle Data Protection Keys.

1. **Registrare l'Applicazione sui Portali degli Identity Provider:**

    - **Google:** Creare credenziali ID client OAuth nella Google Cloud Console, specificando gli URI di reindirizzamento corretti (es. `https://<tuo-dominio-aca>/signin-google`).

    - **Microsoft Entra ID:** Registrare una nuova applicazione in Microsoft Entra ID, configurando gli URI di reindirizzamento (es. `https://<tuo-dominio-aca>/signin-microsoft`) e creando un client secret.

2. **Installare i Pacchetti NuGet necessari nella WebApp:**

    ```sh
    dotnet add package Microsoft.AspNetCore.Authentication.Google
    dotnet add package Microsoft.AspNetCore.Authentication.MicrosoftAccount
    ```

3. Configurare l'Autenticazione in `Program.cs`:

    Assicurarsi che il `Program.cs` sia configurato per leggere le credenziali dal sistema di configurazione di ASP.NET Core, che a sua volta leggerà i segreti e le variabili d'ambiente impostate in ACA. La logica specifica del Program.cs fornito per AddGoogle e AddMicrosoftAccount (con la gestione del tenant) verrà rispettata da questa configurazione.

4. Salvare Client ID e Client Secret come Segreti in ACA:

    Come definito nel Passo 5 e usato nel Passo 7, i segreti vengono mappati alle variabili d'ambiente nel formato corretto (Authentication__Provider__Key).

## Passo 11: Verifica e Troubleshooting ✅

1. **Controllare i Log:**

    - **Portale Azure:** Accedere all'ambiente ACA -> "Log Analytics" o alla singola app container -> "Log stream" o "Log".

    - **Azure CLI:**

        ```sh
        # Log in tempo reale per la webapp
        az containerapp logs show --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP --follow

        # Log in tempo reale per MariaDB
        az containerapp logs show --name $MARIADB_APP_NAME --resource-group $RESOURCE_GROUP --follow
        ```

2. **Accedere alla WebApp:** Aprire l'URL `https://$WEBAPP_URL` (o il dominio personalizzato) nel browser.

3. **Verificare la Connessione al Database:** Assicurarsi che la webapp possa connettersi a MariaDB e eseguire operazioni CRUD.

4. **Verificare l'Autenticazione e la Sessione tra Istanze (se scalata):** Se si scala la webapp a più istanze, provare a fare login e navigare. L'autenticazione dovrebbe persistere.

5. **Verificare l'Invio Email:** Testare le funzionalità di invio email.

## Conclusione

Questa guida dovrebbe fornire una base solida per distribuire un'applicazione su Azure Container Apps. È importante ricordare di adattare i nomi delle risorse, le versioni delle immagini e le configurazioni specifiche alle proprie esigenze. 