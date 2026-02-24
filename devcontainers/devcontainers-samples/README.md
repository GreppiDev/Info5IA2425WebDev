# Dev Containers Samples

## ⚠️ Importante (da leggere prima di iniziare)

Per usare correttamente questi esempi **non lavorare direttamente dentro questo repository “contenitore”**.

È necessario:

1. **Scorporare** la cartella del progetto si vuole usare (ad esempio `basic-container-demo` o `docker-compose-full-example`).
2. **Copiare** quella cartella in un percorso separato.
3. Usare quella cartella come **root di un nuovo repository Git** (`git init` oppure nuovo repo remoto + clone).

Se si aprono i sample come sottocartelle del repository attuale, alcuni progetti possono avere errori all’avvio del Dev Container, in particolare sul **workspace mapping** (mount/percorso workspace).

### ⚠️ Warning: conflitti naming Docker/Compose

Alcuni progetti usano nomi specifici per stack `docker compose` e/o container Docker.
Se nel proprio ambiente esistono già container/stack creati in precedenza con gli stessi nomi, possono verificarsi conflitti in fase di avvio.

Per evitare problemi applicare in alternativa una di queste soluzioni:

- usare un nome Compose diverso (campo `name` nel file `docker-compose.yml`, quando previsto), **oppure**
- eseguire una pulizia completa dell’ambiente Docker del progetto (container, immagini e volumi) se vuoi un ripristino totale.

Questo evita collisioni di naming su rete, container, volumi e risorse correlate.

### ⚠️ Warning: setup a scuola (proxy/rete)

Per **tutti** i progetti, se si lavora in ambiente scolastico con proxy/restrizioni di rete, il setup deve seguire le regole operative già documentate nella sezione:

- [🌐 Dev Containers a scuola (proxy)](./docker-compose-full-example/README.md#-dev-containers-a-scuola-proxy)

Se non si applicano quelle indicazioni, la build del devcontainer, il download dipendenze o l’avvio dei servizi possono fallire. In particolare le indicazioni operative sulla gestione del proxy sono riportate anche in questo README per comodità nel paragrafo seguente.

#### Dev Containers a scuola (proxy)

##### Scenario

In alcune reti scolastiche l’accesso a Internet è mediato da un proxy (es. `http://proxy:3128`).

In questo setup:

- Docker spesso gestisce il proxy in modo “trasparente” per i container: dal container si riesce a navigare/scaricare senza dover configurare proxy applicativi.
- Il problema più comune riguarda VS Code e le sue estensioni, che possono fare richieste di rete e (a seconda di dove girano) non “vedere” automaticamente la stessa configurazione.

Obiettivo: far funzionare insieme:

- app e tool nel container
- estensioni/strumenti legati a VS Code (specialmente quelle basate su Node)

##### Avvio corretto di VS Code su Windows (PowerShell)

1. Aprire Windows PowerShell.
2. Impostare le variabili d’ambiente nella stessa sessione:

    ```powershell
    $env:HTTP_PROXY="http://proxy:3128"
    $env:HTTPS_PROXY="http://proxy:3128"
    $env:NO_PROXY="localhost,127.0.0.1,host.docker.internal"
    ```

3. Dalla stessa shell, aprire VS Code sulla cartella del progetto:

    ```powershell
    code .
    ```

4. In VS Code: `F1` → “Dev Containers: Reopen in Container”.

Così VS Code (lato host) eredita le variabili e, quando serve, anche i processi collegati possono usarle.

##### Perché `http.proxySupport` è impostato su `off`

In `.devcontainer/devcontainer.json` è presente:

```json
"http.proxySupport": "off"
```

Questo forza VS Code (lato Dev Container / VS Code Server) a non gestire un proxy applicativo “proprio”, e a comportarsi come se l’accesso fosse diretto.

Nel contesto “proxy trasparente” di Docker, questo evita la situazione in cui:

- alcune estensioni tentano di usare un proxy configurato in VS Code (o auto-detect)
- mentre la rete del container è già instradata correttamente

Risultato pratico: riduce i casi di estensioni che non riescono a scaricare risorse o che rimangono in timeout per una configurazione proxy incoerente.

##### Problemi tipici e fix rapidi

**Le estensioni non installano / non aggiornano:**

- assicurarsi di lanciare VS Code da PowerShell con `HTTP_PROXY`/`HTTPS_PROXY`/`NO_PROXY` già impostate
- provare `F1` → “Developer: Reload Window”, poi “Dev Containers: Rebuild Container”

**Autenticazioni via proxy:**

- se il proxy richiede credenziali, la stringa proxy potrebbe dover includere user/password (dipende dalle policy della scuola)

**Servizi locali non raggiungibili:**

- verificare che `NO_PROXY` includa `localhost,127.0.0.1,host.docker.internal`

##### 🐛 Debug Node: perché `debug.javascript.autoAttachFilter` è `disabled`

In `.devcontainer/devcontainer.json` è presente:

```json
"debug.javascript.autoAttachFilter": "disabled"
```

Motivo: alcune estensioni (es. assistenti AI e tool che usano Node) avviano processi Node in background. Se l’auto-attach del debugger JS è attivo, VS Code può tentare di “agganciarsi” a quei processi e causare:

- rallentamenti
- comportamenti strani
- errori intermittenti

Scelta consigliata:

- lasciare `disabled` come default nel Dev Container
- abilitarlo solo quando serve davvero fare debug di un’app Node/JS (e poi rimetterlo `disabled`)

### Strumenti consigliati per scaricare una singola cartella

- Repository pubblici: usare [download-directory.github.io](https://download-directory.github.io)
- Per maggiore controllo e soprattutto su repository con accesso riservato: usare gli strumenti descritti in [tools-and-scripts/github-download-directory](https://github.com/malafronte/malafronte-doc-samples/tree/main/tools-and-scripts/github-download-directory)
	- script consigliati: `download-github-folder-optimized.sh` e `download-github-folder-optimized.py`

---

## Panoramica dei progetti

Questa cartella contiene esempi didattici per Dev Containers con ASP.NET Core Minimal API, Docker e MariaDB.

### 1) `basic-container-demo`

- Demo base con Dev Container per ASP.NET Core.
- MariaDB è installato e avviato **dentro** il container di sviluppo.
- Include gestione `.env`, script di bootstrap e setup iniziale orientato a laboratorio.
- Utile per partire rapidamente senza orchestrazione multi-container complessa.

### 2) `docker-compose-full-example`

- Esempio completo con `docker compose`.
- Avvia stack con:
	- container applicativo (devcontainer)
	- container MariaDB
	- rete Docker dedicata
	- volume persistente DB
- Include provisioning DB (schema/seed/grants), varianti permessi DEV/PROD e integrazione con tool AI (Claude/OpenCode).

### 3) `docker-compose-full-starter`

- Starter “pulito” basato su Compose, pensato come template da estendere.
- Struttura simile al full example ma con impostazione più minimale e guidata.
- Ideale per esercitazioni dove vuoi costruire progressivamente API, modello dati e automazioni.

### 4) `docker-compose-full-starter-codespace-ready`

- Variante dello starter pensata per GitHub Codespaces/prebuild.
- Evita dipendenze da path locali host (`localWorkspaceFolder`) e semplifica l’uso cloud.
- Adatta per demo in classe o laboratori remoti con setup ripetibile.

### 5) `docker-compose-with-existing-db-and-network`

- Il devcontainer applicativo si collega a un DB **già esistente**.
- Richiede rete Docker esterna (es. `my-net`) e container DB preconfigurato.
- Utile quando il database è gestito separatamente o condiviso tra più progetti.

---

## Flusso consigliato (rapido)

1. Scegli un progetto dalla lista sopra.
2. Estrai/copia **solo quella cartella** fuori da questo repository.
3. Inizializza un nuovo repository Git nella cartella estratta.
4. Apri la cartella in VS Code.
5. Configura `.env` partendo da `.env.example` (se previsto dal sample).
6. Esegui `Dev Containers: Reopen in Container`.

---

## Note finali

- Ogni sample ha un proprio `README.md` con istruzioni operative dettagliate.
- Mantieni i progetti separati: riduce errori di path, mount e naming Docker.
- Non committare file sensibili (`.env`, token, chiavi API).
