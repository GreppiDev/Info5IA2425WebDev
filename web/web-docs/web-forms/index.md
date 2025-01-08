# Web forms

- [Web forms](#web-forms)
  - [W3schools - HTML Web Forms](#w3schools---html-web-forms)
  - [Mozilla - HTML Web Forms](#mozilla---html-web-forms)
  - [Mozilla - Sending data via HTML forms, with html predefined methods](#mozilla---sending-data-via-html-forms-with-html-predefined-methods)
  - [Invio di dati tramite il protocollo HTTP](#invio-di-dati-tramite-il-protocollo-http)
    - [Invio di dati mediante web form HTML tradizionali (gestiti nativamente dai browsers)](#invio-di-dati-mediante-web-form-html-tradizionali-gestiti-nativamente-dai-browsers)
      - [Caso di Form con codifica **`application/x-www-form-urlencoded`**](#caso-di-form-con-codifica-applicationx-www-form-urlencoded)
      - [Caso di Form con codifica **`multipart/form-data`**](#caso-di-form-con-codifica-multipartform-data)
      - [Caso di Form con codifica `text/plain` (solo per testing)](#caso-di-form-con-codifica-textplain-solo-per-testing)
      - [Invio di dati mediante codifiche non supportate nativamente dai web forms](#invio-di-dati-mediante-codifiche-non-supportate-nativamente-dai-web-forms)
        - [Invio Dati con codifica `application/json`](#invio-dati-con-codifica-applicationjson)
        - [Invio dati con con codifica `application/octet-stream`](#invio-dati-con-con-codifica-applicationoctet-stream)
        - [Invio Dati con codifica `application/xml`](#invio-dati-con-codifica-applicationxml)
        - [Invio Dati con codifica `multipart/related`](#invio-dati-con-codifica-multipartrelated)
    - [**Differenze le varie codifiche**](#differenze-le-varie-codifiche)
    - [**Tabella Riepilogativa delle Modalità di Invio Dati**](#tabella-riepilogativa-delle-modalità-di-invio-dati)
    - [**Codifiche in base al tipo di applicazione**](#codifiche-in-base-al-tipo-di-applicazione)
  - [Security issues in HTML forms](#security-issues-in-html-forms)
    - [Mozilla - Website security](#mozilla---website-security)
    - [Mozilla - Cross-Site Scripting (XSS)](#mozilla---cross-site-scripting-xss)
    - [Mozilla - SQL injection](#mozilla---sql-injection)
    - [W3schools - SQL Injection](#w3schools---sql-injection)
    - [Mozilla - Cross-Site Request Forgery (CSRF)](#mozilla---cross-site-request-forgery-csrf)

## [W3schools - HTML Web Forms](https://www.w3schools.com/hTml/html_forms.asp)

## [Mozilla - HTML Web Forms](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Forms)

## [Mozilla - Sending data via HTML forms, with html predefined methods](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Forms/Sending_and_retrieving_form_data)

## Invio di dati tramite il protocollo HTTP

### Invio di dati mediante web form HTML tradizionali (gestiti nativamente dai browsers)

I **web form tradizionali** supportano principalmente tre tipi di **codifiche (`enctype`)** per l'invio dei dati al server:

1. `application/x-www-form-urlencoded`** (**Default**)

   - ✅ **Codifica predefinita per i form HTML.**
   - ✅ Adatta per **dati semplici** come testo, numeri e date.
   - ✅ I dati vengono inviati nel **body** della richiesta HTTP come coppie chiave-valore separate da `&`.
   - ✅ I caratteri speciali vengono **percent-encoded** (es. lo spazio diventa `%20`).
   - ❌ **Non supporta l'upload di file.**

    > :memo: **Se non si specifica l'attributo `enctype`, il form utilizza **`application/x-www-form-urlencoded`** come impostazione predefinita.**

2. `multipart/form-data`**

   - ✅ Usata quando il form include **file upload** (`<input type="file">`).
   - ✅ Adatta per inviare **dati misti** (file, testo, numeri).
   - ✅ Ogni campo viene inviato come **parte separata** nel body, separata da un **boundary unico**.
   - ✅ Supporta il caricamento di **file binari**.
   - ❌ Più pesante rispetto ad altre codifiche.
  
   >:memo: **Obbligatoria quando si utilizza `<input type="file">.`**
  
3. `text/plain`**

   - ✅ Invia i dati come **testo semplice** nel body della richiesta HTTP.
   - ✅ Ogni coppia chiave-valore è separata da una nuova riga (`\n`).
   - ❌ I caratteri speciali **non vengono codificati** (rischio di perdita di dati o problemi di sicurezza).
   - ❌ **Non supporta l'upload di file.**
   - ❌ Parsing più complesso lato server.
  
  > :memo: **Usata raramente e adatta solo per test rapidi o scenari molto semplici.**

#### Caso di Form con codifica **`application/x-www-form-urlencoded`**

Quando un **form HTML** utilizza la codifica **`application/x-www-form-urlencoded`**, i dati vengono inviati come una stringa chiave-valore nel **body** della richiesta HTTP, con ciascuna coppia separata da un simbolo `&` e i valori codificati in URL.

   1. Esempio di Form HTML con codifica **`application/x-www-form-urlencoded`**

        ```html
        <form action="/submit" method="POST" enctype="application/x-www-form-urlencoded">
            <input type="text" name="username" value="MarioRossi">
            <input type="number" name="age" value="30">
            <input type="date" name="birthdate" value="1990-01-01">
            <input type="submit">
        </form>
        ```

   2. Richiesta HTTP completa del Browser

        ```makefile
        POST /submit HTTP/1.1
        Host: www.example.com
        User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36
        Accept: */*
        Accept-Language: it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7
        Content-Type: application/x-www-form-urlencoded
        Content-Length: 52

        username=MarioRossi&age=30&birthdate=1990-01-01
        ```

   3. Spiegazione Dettagliata
      1. Header HTTP

         - **`POST /submit HTTP/1.1`**

             - Metodo **POST** per inviare i dati al server all'endpoint `/submit`.
         - **`Host: www.example.com`**

             - Indirizzo del server a cui viene inviata la richiesta.
         - **`Content-Type: application/x-www-form-urlencoded`**

             - Specifica che i dati nel body sono inviati nel formato `application/x-www-form-urlencoded`.
         - **`Content-Length: 52`**

             - Indica la lunghezza totale del body della richiesta (in byte).

      2. Body HTTP

            ```makefile
            username=MarioRossi&age=30&birthdate=1990-01-01
            ```

   4. Punti chiave
         - **Formato Chiave-Valore:** Ogni coppia è nel formato `nomeCampo=valore`.
         - **Separatore `&`:** Le coppie chiave-valore sono separate dal simbolo `&`.
         - **Codifica URL:** Se i valori contengono spazi o caratteri speciali, vengono codificati secondo le regole **percent-encoding**.
             - Esempio: `Mario Rossi` → `Mario%20Rossi`

   5. Spiegazione dei campi inviati
      1. `username=MarioRossi` → Il campo `username` ha il valore `"MarioRossi"`.
      2. `age=30` → Il campo `age` ha il valore `"30"`.
      3. `birthdate=1990-01-01` → Il campo `birthdate` ha il valore `"1990-01-01"`.

   6. Quando usare `application/x-www-form-urlencoded`?

      - Quando i dati inviati sono **semplici** (stringhe, numeri, date).
      - Quando **non devi caricare file**.
      - Per richieste **API leggere** o situazioni in cui ogni byte conta (es. richieste API in sistemi embedded).

#### Caso di Form con codifica **`multipart/form-data`**

Quando un **form HTML** utilizza la codifica **`multipart/form-data`**, i dati del form  sono inseriti nel **body del messaggio HTTP**. L'**header** contiene solo le informazioni che indicano al server come interpretare i dati contenuti nel body. In particolare, viene utilizzato l'header:

```css
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary123456
```

- Il `boundary` è un identificatore che separa le diverse parti del contenuto all'interno del body, permettendo al server di distinguere i vari campi del form (ad esempio testo, file, ecc.).

- I dati vengono suddivisi in più parti con un `boundary` che li separa.
- Ogni parte può contenere testo o file binari.
- Adatto per **caricamento di file** o dati di form complessi.

  1. Esempio di Form HTML con codifica **`multipart/form-data`**

      ```html
      <form action="/upload" method="POST" enctype="multipart/form-data">
          <input type="text" name="username" value="MarioRossi">
          <input type="number" name="age" value="30">
          <input type="file" name="profilePicture">
          <input type="submit">
      </form>
      ```

  2. Richiesta HTTP Completa del Browser

      ```makefile
      POST /upload HTTP/1.1
      Host: www.example.com
      User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36
      Accept: */*
      Accept-Language: it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7
      Content-Type: multipart/form-data; boundary=----WebKitFormBoundary12345
      Content-Length: 356

      ------WebKitFormBoundary12345
      Content-Disposition: form-data; name="username"

      MarioRossi
      ------WebKitFormBoundary12345
      Content-Disposition: form-data; name="age"

      30
      ------WebKitFormBoundary12345
      Content-Disposition: form-data; name="profilePicture"; filename="avatar.png"
      Content-Type: image/png

      <contenuto binario del file PNG>
      ------WebKitFormBoundary12345--
      ```

  3. Spiegazione Dettagliata
      1. Header HTTP
           - **`POST /upload HTTP/1.1`**

               - Metodo **POST** per inviare dati al server all'endpoint `/upload`.
           - **`Host: www.example.com`**

               - Indirizzo del server a cui viene inviata la richiesta.
           - **`Content-Type: multipart/form-data; boundary=----WebKitFormBoundary12345`**

               - Il tipo di contenuto indica che i dati sono inviati con una **codifica multipart/form-data**.
               - `boundary=----WebKitFormBoundary12345` specifica il marcatore che separa le varie parti del body.
           - **`Content-Length: 356`**

               - Indica la lunghezza totale del body della richiesta (in byte).

      2. Body HTTP

          Il body è strutturato con sezioni separate dal `boundary` (`------WebKitFormBoundary12345`).

          **Campo 1: `username` (Testo)**

          ```css
          ------WebKitFormBoundary12345
          Content-Disposition: form-data; name="username"

          MarioRossi
          ```

          - `Content-Disposition`: Specifica che è un **campo del form** chiamato `username`.
          - **Valore:** `MarioRossi`

          **Campo 2: `age` (Numero)**

          ```css
          ------WebKitFormBoundary12345
          Content-Disposition: form-data; name="age"

          30
          ```

          - `Content-Disposition`: Campo chiamato `age`.
          - **Valore:** `30`

          **Campo 3: `profilePicture` (File)**

          ```css
          ------WebKitFormBoundary12345
          Content-Disposition: form-data; name="profilePicture"; filename="avatar.png"
          Content-Type: image/png

          <contenuto binario del file PNG>
          ```

         - `Content-Disposition`: Campo chiamato `profilePicture`, con un file caricato chiamato `avatar.png`.
         - `Content-Type`: Tipo MIME del file (`image/png`).
         - **Contenuto:** I byte del file binario sono inclusi qui.

         **Chiusura del Body**

          ```css
          ------WebKitFormBoundary12345--
          ```

         - Il doppio `--` alla fine del boundary (`------WebKitFormBoundary12345--`) segnala la **fine del body**.

  4. Punti Chiave Riassunti

     **Header:**

      - L'header `Content-Type` specifica la codifica multipart e il boundary.
      - L'header `Content-Length` indica la lunghezza del corpo della richiesta.

      **Body:**

      - Ogni campo è separato dal boundary.
      - I campi di testo hanno solo `Content-Disposition`.
      - I campi file includono anche `filename` e `Content-Type`.
      - Il body termina con il boundary finale seguito da `--`.

  5. Quando usare `multipart/form-data`?

      - Usata quando il form include **file upload** (`<input type="file">`).
      - Adatta per inviare **dati misti** (file, testo, numeri).

#### Caso di Form con codifica `text/plain` (solo per testing)

La codifica **`text/plain`** è un'opzione meno comune per l'invio di dati da un form HTML. Quando utilizzata, i dati vengono inviati come testo semplice nel **body** della richiesta HTTP, senza alcuna struttura specifica come chiave-valore formattati o separatori complessi.

1. Esempio di Form HTML con `text/plain`

   ```html
    <form action="/submit" method="POST" enctype="text/plain">
    <input type="text" name="username" value="MarioRossi">
    <input type="number" name="age" value="30">
    <input type="date" name="birthdate" value="1990-01-01">
    <input type="submit">
    </form>
    ```

    >**`enctype="text/plain"`** specifica che i dati verranno inviati come testo semplice.

2. Richiesta HTTP Completa

    ```makefile
    POST /submit HTTP/1.1
    Host: www.example.com
    User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36
    Accept: */*
    Content-Type: text/plain
    Content-Length: 51

    username=MarioRossi
    age=30
    birthdate=1990-01-01
    ```

3. Spiegazione Dettagliata

   1. Header HTTP**

      - **`POST /submit HTTP/1.1`**

          - Metodo **POST** per inviare i dati al server all'endpoint `/submit`.
      - **`Host: www.example.com`**

          - Indirizzo del server a cui viene inviata la richiesta.
      - **`Content-Type: text/plain`**

          - Indica che i dati nel body sono inviati come **testo semplice**.
      - **`Content-Length: 51`**

          - Indica la lunghezza totale dei dati inviati nel body (in byte).

4. Body HTTP

    I dati vengono inviati come testo semplice, con ogni coppia chiave-valore separata da una nuova riga:

    ```makefile
    username=MarioRossi
    age=30
    birthdate=1990-01-01
    ```

   - **Formato chiave-valore:** Simile a `application/x-www-form-urlencoded`, ma ogni coppia chiave-valore è separata da una **nuova riga** anziché da `&`.
   - **Nessuna codifica speciale:** I caratteri speciali non vengono codificati (es. spazi o simboli speciali).
   - **Non strutturato:** Il server deve interpretare manualmente i dati.

5. Quando usare `text/plain`?

   - **Debugging e test rapidi:** Utile per testare rapidamente l'invio di dati senza preoccuparsi della codifica.
   - **Form molto semplici:** In scenari estremamente basilari, dove il parsing manuale lato server non è un problema.
   - **Interfacce legacy:** Alcuni endpoint meno moderni potrebbero accettare dati solo come testo.

    :memo: **Importante**: Se il backend riceve dati `text/plain`, il parsing deve essere fatto manualmente!

    ```cs
        app.MapPost("/submit", async (HttpContext context) => {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();

            var data = body
                .Split('\n')
                .Select(line => line.Split('='))
                .ToDictionary(parts => parts[0], parts => parts[1]);

            var username = data["username"];
            var age = int.Parse(data["age"]);
            var birthdate = DateTime.Parse(data["birthdate"]);

            return Results.Ok(new { username, age, birthdate });
        });
   ```

#### Invio di dati mediante codifiche non supportate nativamente dai web forms

Quando occorre inviare dati con una codifica diversa da `application/x-www-form-urlencoded`, `multipart/form-data`, oppure `text/plain` (che è usata solo per testing), occorre impiegare meccanismi diversi da quelli predefiniti dei browser per la gestione dei form. Di seguito vengono analizzati alcuni casi di maggiore interesse nelle applicazioni.

##### Invio Dati con codifica `application/json`

Quando un **form HTML** o una richiesta API utilizza la codifica **`application/json`**, i dati vengono inviati nel **body** come una stringa JSON ben formattata.

> ⚠️ **Nota:** I form HTML tradizionali **non supportano direttamente `application/json`**. In genere, questa codifica viene utilizzata con richieste API effettuate tramite **JavaScript** (es. con `fetch` o `XMLHttpRequest`).

1. **Esempio con JavaScript (`fetch`)**

    ```html
    <form id="myForm">
        <input type="text" name="username" value="MarioRossi">
        <input type="number" name="age" value="30">
        <input type="date" name="birthdate" value="1990-01-01">
        <button type="button" id="submitBtn">Invia</button>
    </form>

    <script>
        document.getElementById('submitBtn').addEventListener('click', () => {
            const formData = {
                username: document.querySelector('input[name="username"]').value,
                age: parseInt(document.querySelector('input[name="age"]').value, 10),
                birthdate: document.querySelector('input[name="birthdate"]').value
            };

            fetch('/submit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });
        });
    </script>
    ```

2. **Richiesta HTTP Completa**

    ```makefile
    POST /submit HTTP/1.1
    Host: www.example.com
    User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36
    Accept: */*
    Accept-Language: it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7
    Content-Type: application/json
    Content-Length: 72

    {
        "username": "MarioRossi",
        "age": 30,
        "birthdate": "1990-01-01"
    }
    ```

3. **Spiegazione Dettagliata**

    **1. Header HTTP**

      - **`POST /submit HTTP/1.1`**

          - Metodo **POST** per inviare i dati al server all'endpoint `/submit`.
      - **`Host: www.example.com`**

          - Indirizzo del server a cui viene inviata la richiesta.
      - **`Content-Type: application/json`**

          - Specifica che il contenuto nel body è in formato **JSON**.
      - **`Content-Length: 72`**

          - Indica la lunghezza totale del body della richiesta (in byte).

    **2. Body HTTP**

      Il body contiene una stringa **JSON valida**:

    ```json
        {
            "username": "MarioRossi",
            "age": 30,
            "birthdate": "1990-01-01"
        }
    ```

4. **Spiegazione dei campi inviati:**

   1. `"username": "MarioRossi"` → Il campo `username` ha il valore `"MarioRossi"`.
   2. `"age": 30` → Il campo `age` ha il valore numerico `30`.
   3. `"birthdate": "1990-01-01"` → Il campo `birthdate` ha il valore `"1990-01-01"`.

5. **Differenze con `application/x-www-form-urlencoded` e `multipart/form-data`**

    | **Caratteristica** | **application/json** | **application/x-www-form-urlencoded** | **multipart/form-data** |
    | --- |  --- |  --- |  --- |
    | **Formato dei dati** | JSON | Stringa chiave-valore | Blocchi separati da boundary |
    | **Supporto file** | ❌ No | ❌ No | ✅ Sì |
    | **Struttura complessa** | ✅ Sì | ❌ No | ✅ Sì |
    | **Peso della richiesta** | ✅ Leggero | ✅ Leggero | ❌ Più pesante |
    | **Adatto per** | API REST, JSON RPC | Form web semplici | Caricamento file e form complessi |

6. **Quando usare `application/json`?**

   - Quando stai sviluppando un'**API REST**.
   - Quando devi inviare **dati strutturati complessi**.
   - Quando l'interazione client-server avviene tramite **JavaScript** (es. `fetch`).
   - Quando vuoi maggiore **leggibilità e scalabilità** dei dati inviati.

##### Invio dati con con codifica `application/octet-stream`

La codifica **`application/octet-stream`** viene utilizzata principalmente per inviare **dati binari grezzi** (file) nel **body** della richiesta HTTP.
Questa codifica è spesso usata quando un **singolo file** viene inviato al server senza ulteriori metadati o altri campi aggiuntivi.

> ⚠️ **Nota importante:** I **form HTML non supportano direttamente `application/octet-stream`**. Per questa codifica, il caricamento di un file viene generalmente effettuato tramite **JavaScript** (es. con `fetch` o `XMLHttpRequest`) o tramite un **client HTTP dedicato** (es. cURL, Postman).

1. **Esempio con JavaScript (`fetch`) - nome del file passato nella query string**

    Di seguito si riporta un esempio di invio di un file file con `application/octet-stream` tramite JavaScript, supponendo che il nome del file sia inviato direttamente nella query string:

    ```cs
    <input type="file" id="fileInput">
    <button id="uploadBtn">Upload</button>

    <script>
    document.getElementById('uploadBtn').addEventListener('click', () => {
        const fileInput = document.getElementById('fileInput');
        const file = fileInput.files[0];

        if (file) {
            const fileName = encodeURIComponent(file.name); // Codifica il nome del file per URL
            const url = `/upload?fileName=${fileName}`; // Aggiungi il nome del file come query string

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/octet-stream'
                },
                body: file
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('File upload failed');
                }
                return response.text();
            })
            .then(result => console.log('Success:', result))
            .catch(error => console.error('Error:', error));
        } else {
            console.error('No file selected!');
        }
    });
    </script>
    ```

   1. **Richiesta HTTP Completa**

        ```makefile
        POST /upload?fileName=document.pdf HTTP/1.1
        Host: www.example.com
        User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36
        Accept: */*
        Content-Type: application/octet-stream
        Content-Length: 524288

        <contenuto binario del file PDF>
        ```

   2. **Spiegazione Dettagliata**

        **1. Header HTTP**

        - **`POST /upload?fileName=document.pdf HTTP/1.1`**
            - Metodo **POST** per inviare i dati binari al server all'endpoint `/upload`.
            - Il **nome del file** (`document.pdf`) è passato come **parametro nella query string** (`?fileName=document.pdf`).
        - **`Host: www.example.com`**
            - Indirizzo del server a cui viene inviata la richiesta.
        - **`Content-Type: application/octet-stream`**
        - **`Content-Length: 524288`**
            - Indica la lunghezza totale dei dati inviati nel body (in byte).

        **2. Body HTTP**

        ```text
            <contenuto binario del file PDF>
        ```

        - I dati binari del file (`document.pdf`) vengono inclusi **direttamente nel body** senza alcun tipo di separatore o metadata aggiuntivo.
        - Non ci sono metadati aggiuntivi (come boundary o chiavi di form).
        - Il nome del file è già stato inviato tramite **query string**, quindi il server può accedere facilmente al valore di `fileName` senza dover analizzare gli header.

2. **Esempio con JavaScript (`fetch`) - nome del file passato mediante `Content-Disposition`**

    Di seguito si riporta un esempio di invio di un file file con `application/octet-stream` tramite JavaScript, supponendo che il nome del file sia inviato nell'header del messaggio HTTP, mediante il parametro `Content-Disposition`:

    ```html
    <input type="file" id="fileInput">
    <button id="uploadBtn">Upload</button>

    <script>
        document.getElementById('uploadBtn').addEventListener('click', () => {
            const fileInput = document.getElementById('fileInput');
            const file = fileInput.files[0];

            if (file) {
                fetch('/upload', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/octet-stream',
                        'Content-Disposition': `attachment; filename="${file.name}"`
                    },
                    body: file
                })
                .then(response => response.text())
                .then(result => console.log('Success:', result))
                .catch(error => console.error('Error:', error));
            } else {
                console.error('No file selected!');
            }
        });
    </script>
    ```

   1. **Richiesta HTTP Completa**

       ```makefile
       POST /upload HTTP/1.1
       Host: www.example.com
       User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36
       Accept: */*
       Content-Type: application/octet-stream
       Content-Disposition: attachment; filename="document.pdf"
       Content-Length: 524288

       <contenuto binario del file PDF>
       ```

   2. **Spiegazione Dettagliata**

       **1. Header HTTP**

       - **`POST /upload HTTP/1.1`**

           - Metodo **POST** per inviare i dati binari al server all'endpoint `/upload`.
       - **`Host: www.example.com`**

           - Indirizzo del server a cui viene inviata la richiesta.
       - **`Content-Type: application/octet-stream`**

           - Indica che i dati inviati nel body sono binari grezzi.
       - **`Content-Disposition: attachment; filename="document.pdf"`**

           - Suggerisce il **nome del file** (`document.pdf`) che viene caricato.
       - **`Content-Length: 524288`**

           - Indica la lunghezza totale dei dati inviati nel body (in byte).

       **2. Body HTTP**

       ```text
          <contenuto binario del file PDF>
       ```

         - I dati binari del file (`document.pdf`) vengono inclusi **direttamente nel body** senza alcun tipo di separatore o metadata aggiuntivo.

3. **Differenze con altre codifiche**

    | **Caratteristica** | **application/octet-stream** | **multipart/form-data** | **application/json** |
    | --- |  --- |  --- |  --- |
    | **Formato dei dati** | Dati binari grezzi | Parti separate da boundary | JSON strutturato |
    | **Supporto file** | ✅ Sì | ✅ Sì | ❌ No |
    | **Metadati inclusi** | ❌ No | ✅ Sì (nome campo, tipo) | ✅ Sì (campo JSON) |
    | **Adatto per** | Singoli file grezzi | Form con più campi | API REST |
    | **Facilità di parsing** | ❌ Difficile (solo dati grezzi) | ✅ Facile | ✅ Facile |

4. **Quando usare `application/octet-stream`?**

   - Quando si deve inviare un **singolo file binario grezzo** al server.
   - Quando non sono necessari **metadati aggiuntivi** o ulteriori campi nel form.
   - Quando l'endpoint è progettato per ricevere **esclusivamente un file binario**.
   - Tipicamente usato con **API di upload specifiche** o strumenti come **cURL**.

5. **Esempio con `cURL`**

    Se si volesse caricare lo stesso file usando `cURL`, il comando sarebbe:

    ```bash
        curl -X POST "http://localhost:5000/upload?fileName=test.pdf" \
        -H "Content-Type: application/octet-stream" \
        --data-binary "@C:\path\to\your\file.pdf"
    ```

    oppure:

    ```bash
        curl -X POST "http://www.example.com/upload"\
        -H "Content-Type: application/octet-stream"\
        -H "Content-Disposition: attachment; filename=\"document.pdf\""\
        --data-binary "@C:\path\to\your\document.pdf"
    ```

    Nel primo caso il nome del file sarebbe passato query string, mentre nel secondo caso sarebbe indicato nell'header HTTP `Content-Disposition`.

6. **Un esempio di Minimal API Endpoint per la gestione dell'upload di un file con codifica `application/octet-stream`**
   1. Supponendo che il nome del file sia passato nella query string, si potrebbe avere un codice come il seguente:

        ```cs
        using Microsoft.AspNetCore.Http.HttpResults;
        using Microsoft.AspNetCore.Mvc;
        using System.IO;

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Endpoint per ricevere un file PDF con codifica application/octet-stream
        app.MapPost("/upload", async Task<Results<Ok<string>, BadRequest<string>>> (HttpRequest request) =>
        {
            try
            {
                // Verifica che il Content-Type sia corretto
                if (!request.ContentType?.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    return TypedResults.BadRequest("Content-Type non valido. Utilizza 'application/octet-stream'.");
                }

                // Estrai il nome del file dai parametri della query (opzionale)
                var fileName = request.Query["fileName"].ToString();
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"file_{Guid.NewGuid()}.pdf"; // Nome di default
                }

                var uploadPath = Path.Combine("Uploads", fileName);
                var uploadDirectory = Path.GetDirectoryName(uploadPath);

                // Verifica se la directory esiste, altrimenti la crea
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                // Salva il file ricevuto
                using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                {
                    await request.Body.CopyToAsync(fileStream);
                }

                return TypedResults.Ok($"File salvato con successo: {uploadPath}");
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest($"Errore durante l'upload: {ex.Message}");
            }
        });

        // Avvia l'applicazione
        app.Run();
        ```

   2. Supponendo che il nome del file sia nell'header del messaggio HTTP, mediante il campo `Content-Disposition`, si potrebbe avere un codice come il seguente:

        ```cs
        using Microsoft.AspNetCore.Http.HttpResults;
        using Microsoft.AspNetCore.Mvc;
        using System.Net.Http.Headers;  // Necessario per ContentDispositionHeaderValue
        using System.IO;

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Endpoint per ricevere un file PDF con codifica application/octet-stream
        app.MapPost("/upload", async Task<Results<Ok<string>, BadRequest<string>>> (HttpRequest request) =>
        {
            // Verifica che il Content-Type sia application/octet-stream
            if (!request.ContentType?.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                return TypedResults.BadRequest("Content-Type non valido. Utilizza 'application/octet-stream'.");
            }

            // Estrai l'header 'Content-Disposition'
            if (!request.Headers.TryGetValue("Content-Disposition", out var contentDispositionHeader))
            {
                return TypedResults.BadRequest("Header 'Content-Disposition' mancante.");
            }

            // Parsing dell'header Content-Disposition
            if (!ContentDispositionHeaderValue.TryParse(contentDispositionHeader, out var contentDisposition))
            {
                return TypedResults.BadRequest("Intestazione 'Content-Disposition' non valida.");
            }

            // Estrai il nome del file
            var fileName = contentDisposition.FileNameStar ?? contentDisposition.FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                return TypedResults.BadRequest("Nome del file mancante nell'intestazione 'Content-Disposition'.");
            }

            // Definisci il percorso di salvataggio del file
            var uploadPath = Path.Combine("Uploads", fileName);
            var uploadDirectory = Path.GetDirectoryName(uploadPath);

            // Verifica se la directory esiste, altrimenti la crea
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory!);
            }

            // Salva il file ricevuto
            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                await request.Body.CopyToAsync(fileStream);
            }

            // Risposta positiva con il percorso del file salvato
            return TypedResults.Ok($"File salvato con successo: {uploadPath}");
        });

        // Avvia l'applicazione
        app.Run();
        ```

        📌 **Spiegazione di `contentDisposition.FileNameStar` e `contentDisposition.FileName`**: In ASP.NET Core, la classe `ContentDispositionHeaderValue` ha due proprietà principali per estrarre il nome del file da un header **`Content-Disposition`**:

        1. **`FileName`**:

            - È la proprietà più **tradizionale** e viene usata per ottenere il nome del file. Tuttavia, questa proprietà è **sensibile alla codifica** (ad esempio, se il nome del file contiene caratteri speciali, come spazi o caratteri non ASCII, potrebbe essere codificato in un formato speciale come `UTF-8` o `ISO-8859-1`).
        2. **`FileNameStar`**:

            - È la **versione più recente** introdotta per **gestire correttamente il nome del file** in **formati internazionali** (caratteri speciali, lettere accentate, ecc.).
            - A differenza di `FileName`, `FileNameStar` è **sempre in formato UTF-8** e non subisce alcuna codifica o escape, il che rende il nome del file **più leggibile** se il file contiene caratteri speciali.

##### Invio Dati con codifica `application/xml`

**Quando viene usata:**

- Sistemi legacy o servizi SOAP.
- Quando i dati devono essere scambiati in un formato **XML standardizzato**.

✅ **Come vengono inviati i dati:**

- I dati vengono inviati come stringa XML strutturata nel **body**.

✅ **Esempio di richiesta HTTP:**

```php
POST /api/data HTTP/1.1
Host: api.example.com
Content-Type: application/xml
Content-Length: 96

<user>
  <username>MarioRossi</username>
  <age>30</age>
  <birthdate>1990-01-01</birthdate>
</user>
```

✅ **Pro:**

- Struttura chiara e validabile con schema XSD.
- Adatto a sistemi legacy.

❌ **Contro:**

- Verboso rispetto a JSON.
- Parsing più pesante.

##### Invio Dati con codifica `multipart/related`

**Quando viene usata:**

- Quando è necessario inviare **file binari e dati strutturati insieme** in un'unica richiesta.
- Ad esempio: dati JSON + un file binario.

✅ **Come vengono inviati i dati:**

- Il body è diviso in **parti separate da boundary**.
- Ogni parte può contenere dati binari o testo strutturato.

✅ **Esempio di richiesta HTTP:**

```css
POST /upload HTTP/1.1
Host: api.example.com
Content-Type: multipart/related; boundary=abc123

--abc123
Content-Type: application/json

{
  "username": "MarioRossi",
  "description": "Documento importante"
}

--abc123
Content-Type: application/octet-stream
Content-Disposition: attachment; filename="file.pdf"

<contenuto binario del file>
--abc123--
```

✅ **Pro:**

- Permette di inviare sia dati strutturati che file in una singola richiesta.

❌ **Contro:**

- Parsing complesso lato server.
- Richiede gestione avanzata dei boundary.

### **Differenze le varie codifiche**

| **Caratteristica** | **text/plain** | **application/x-www-form-urlencoded** | **multipart/form-data** | **application/json** |
| --- |  --- |  --- |  --- |  --- |
| **Formato dei dati** | Testo semplice | Stringa chiave-valore | Blocchi separati | JSON strutturato |
| **Supporto file** | ❌ No | ❌ No | ✅ Sì | ❌ No |
| **Separatore dati** | Nuova riga (`\n`) | Simbolo `&` | Boundary | Struttura JSON |
| **Codifica speciale** | ❌ No | ✅ Percent-encoding | ✅ Boundary | ✅ JSON |
| **Parsing server-side** | ❌ Manuale | ✅ Automatico | ✅ Automatico | ✅ Automatico |
| **Adatto per** | Test rapido, debug | Form dati semplici | File upload, dati misti | API REST |

### **Tabella Riepilogativa delle Modalità di Invio Dati**

| **Codifica** | **Tipo Dati** | **Supporto File** | **Caso d'uso** | **Parsing Lato Server** |
| --- |  --- |  --- |  --- |  --- |
| `application/x-www-form-urlencoded` | Dati semplici (testo/num) | ❌ No | Web form tradizionali | ✅ Facile |
| `multipart/form-data` | Dati misti | ✅ Sì | Upload file + dati misti | ✅ Facile |
| `text/plain` | Testo semplice | ❌ No | Debugging/Test rapido | ❌ Manuale |
| `application/json` | Oggetti strutturati | ❌ No | API RESTful | ✅ Facile |
| `application/octet-stream` | File grezzi | ✅ Sì | Upload singolo file | ❌ Manuale |
| `application/xml` | Oggetti XML strutturati | ❌ No | Sistemi legacy/SOAP | ✅ Moderato |
| `multipart/related` | Dati + File | ✅ Sì | Dati strutturati + file | ❌ Complesso |

### **Codifiche in base al tipo di applicazione**

1. **Form tradizionali:** :arrow_right: `application/x-www-form-urlencoded`, `multipart/form-data`.
2. **API REST:** :arrow_right: `application/json`.
3. **File singolo grezzo:** :arrow_right: `application/octet-stream`.
4. **SOAP o XML:** :arrow_right: `application/xml`.
5. **Dati complessi (file + JSON):** :arrow_right: `multipart/related`.

## [Security issues in HTML forms](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Forms/Sending_and_retrieving_form_data#security_issues)

### [Mozilla - Website security](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Server-side/First_steps/Website_security)

### [Mozilla - Cross-Site Scripting (XSS)](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Server-side/First_steps/Website_security#cross-site_scripting_xss)

### [Mozilla - SQL injection](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Server-side/First_steps/Website_security#sql_injection)

### [W3schools - SQL Injection](https://www.w3schools.com/sql/sql_injection.asp)

### [Mozilla - Cross-Site Request Forgery (CSRF)](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Server-side/First_steps/Website_security#cross-site_request_forgery_csrf)
