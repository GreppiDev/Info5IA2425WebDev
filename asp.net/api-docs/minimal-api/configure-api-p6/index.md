# Anti-Forgery in ASP.NET Minimal APIs

- [Anti-Forgery in ASP.NET Minimal APIs](#anti-forgery-in-aspnet-minimal-apis)
	- [Introduzione al problema del Cross-Site Request Forgery (CSRF)](#introduzione-al-problema-del-cross-site-request-forgery-csrf)
	- [Implementazione dell'Anti-Forgery Token in ASP.NET Minimal APIs](#implementazione-dellanti-forgery-token-in-aspnet-minimal-apis)
		- [Best Practice per l'Anti-Forgery in Minimal APIs](#best-practice-per-lanti-forgery-in-minimal-apis)
	- [La sfida del Frontend separato](#la-sfida-del-frontend-separato)
		- [Flusso di lavoro in un Frontend separato con Anti-Forgery](#flusso-di-lavoro-in-un-frontend-separato-con-anti-forgery)
			- [Esempio di codice completo (Frontend Statico + Backend Minimal API Anti-Forgery)](#esempio-di-codice-completo-frontend-statico--backend-minimal-api-anti-forgery)
			- [Considerazioni importanti e best practice per Frontend separato e Anti-Forgery](#considerazioni-importanti-e-best-practice-per-frontend-separato-e-anti-forgery)
			- [Perché l'invio del solo `RequestToken` è la scelta ottimale nel caso di Frontend separato dal Backend?](#perché-linvio-del-solo-requesttoken-è-la-scelta-ottimale-nel-caso-di-frontend-separato-dal-backend)
				- [Differenze tra lo scenario con Frontend e Backend integrati e quello con Frontend e Backend separati](#differenze-tra-lo-scenario-con-frontend-e-backend-integrati-e-quello-con-frontend-e-backend-separati)
		- [Anti-Forgery Token con sessioni non abilitate sul backend](#anti-forgery-token-con-sessioni-non-abilitate-sul-backend)
			- [Test (Stateless Anti-Forgery)](#test-stateless-anti-forgery)

## Introduzione al problema del Cross-Site Request Forgery (CSRF)

Il **Cross-Site Request Forgery (CSRF)** è una vulnerabilità di sicurezza web che permette a un attaccante di indurre un utente autenticato a eseguire azioni indesiderate su un'applicazione web **a sua insaputa**.

1. **Scenario tipico di attacco CSRF:**

   1. **Utente autenticato**: Un utente (vittima) è autenticato su un'applicazione web (es. una banca online).
   2. **Sito web malevolo**: L'utente visita un sito web malevolo controllato dall'attaccante.
   3. **Form malevolo nel sito**: Il sito malevolo contiene un form nascosto che punta all'applicazione web vulnerabile.  Questo form è progettato per eseguire un'azione dannosa (es. trasferimento di denaro, cambio password).
   4. **Submit automatico**: Il sito malevolo esegue automaticamente la submit del form nascosto, mentre l'utente è autenticato nell'applicazione web.
   5. **Azione non autorizzata**: Il browser dell'utente, **inclusi i cookie di autenticazione**, invia la richiesta all'applicazione web vulnerabile. Poiché l'utente è autenticato, l'applicazione web **crede che la richiesta provenga dall'utente legittimo** e esegue l'azione dannosa.

2. **Diagramma di sequenza del flusso di dati senza Anti-Forgery**

	```mermaid
	sequenceDiagram
		participant U as Utente
		participant B as Browser
		participant M as Sito malevolo
		participant A as App Web vulnerabile

		Note over U,A: Utente già autenticato nell'App Web
		
		U->>B: Visita sito malevolo
		B->>M: Richiesta pagina
		M-->>B: Risposta con form nascosto
		
		Note over M,B: Form progettato per azione dannosa
		
		M->>B: Esegue submit automatico del form
		
		Note over B: Browser include automaticamente<br/>i cookie di autenticazione
		
		B->>A: Invia richiesta con cookie auth
		
		Note over A: App verifica autenticazione<br/>tramite cookie (validi)
		
		A-->>B: Esegue azione dannosa
		B-->>U: L'utente non è consapevole<br/>dell'azione eseguita
	```

	*Descrizione Diagramma: L'utente autenticato interagisce con un sito malevolo. Il sito malevolo contiene un form che punta all'applicazione vulnerabile. Il browser dell'utente, con i cookie di autenticazione, invia la richiesta all'applicazione vulnerabile. L'applicazione vulnerabile esegue l'azione dannosa credendo che la richiesta sia legittima.*

3. **Diagramma di sequenza del flusso di dati con Anti-Forgery**

	```mermaid
	sequenceDiagram
		participant U as Utente
		participant B as Browser
		participant A as App Web Protetta
		
		U->>B: Richiede form
		B->>A: Richiesta pagina form
		
		Note over A: Genera Anti-Forgery Token
		
		A-->>B: Risposta con HTML form +<br/>Anti-Forgery Token
		B-->>U: Mostra form
		
		U->>B: Submit form
		
		Note over B: Browser include:<br/>1. Cookie di autenticazione<br/>2. Anti-Forgery Token
		
		B->>A: Invia richiesta form
		
		Note over A: Verifica:<br/>1. Cookie di autenticazione<br/>2. Anti-Forgery Token
		
		alt Token e Cookie validi
			A-->>B: Esegue azione richiesta
			B-->>U: Mostra conferma
		else Token o Cookie non validi
			A-->>B: Rifiuta richiesta
			B-->>U: Mostra errore
		end
	```

	*Descrizione Diagramma: L'applicazione genera un Anti-Forgery Token e lo invia al client (browser) insieme alla pagina HTML del form. Il client (browser) include l'Anti-Forgery Token nel form. Quando il form viene inviato, il browser include sia i cookie di autenticazione che l'Anti-Forgery Token nella richiesta. L'applicazione verifica sia i cookie di autenticazione che l'Anti-Forgery Token. Se entrambi sono validi, l'applicazione esegue l'azione; altrimenti, la richiesta viene rifiutata.*

## Implementazione dell'Anti-Forgery Token in ASP.NET Minimal APIs

ASP.NET Core fornisce un meccanismo integrato per proteggere le applicazioni web dagli attacchi CSRF tramite **Anti-Forgery Tokens**.

1. **Lato Server: Generazione e validazione del token**

   1. **Generazione del Token**:  ASP.NET Core genera automaticamente un Anti-Forgery Token per ogni sessione utente autenticata. Questo token è univoco e legato alla sessione dell'utente.
   2. **Inclusione del Token nel Form (Server-side rendering)**: Quando generi la pagina HTML contenente il form lato server (es. con Razor Pages, MVC Views, o anche Minimal APIs che restituiscono HTML), devi includere l'Anti-Forgery Token nel form.  Questo viene fatto solitamente con un helper tag in Razor o manualmente in HTML.  **Nel contesto delle Minimal APIs che restituiscono HTML stringhe, dovrai includere il token manualmente.**
   3. **Validazione del Token (Endpoint)**:  Nell'endpoint che processa il form (tipicamente un endpoint POST), devi **validare** l'Anti-Forgery Token ricevuto dalla richiesta. Questo assicura che la richiesta provenga effettivamente dal tuo sito e non da un sito malevolo.

2. **Lato Client: Inclusione del token nel Form**

	Il client (browser) deve includere l'Anti-Forgery Token nel form quando lo invia al server.  ASP.NET Core si aspetta che l'Anti-Forgery Token sia inviato come:

	* **Campo nascosto nel form**:  Il modo più comune e compatibile.
	* **Header HTTP**:  Alternativamente, il token può essere inviato in un header HTTP personalizzato (es. per applicazioni JavaScript single-page).

**Esempio di codice completo (C\#, HTML, JavaScript) - Anti-Forgery con Form**

1. **Program.cs (C\# - ASP.NET Minimal API):**
   Il codice di questo esempio è disponibile nel progetto [MonolithicWebDemo](../../../api-samples/minimal-api/BindingDemos/AntiForgeryDemos/MonolithicWebDemo/)

	```csharp
	using Microsoft.AspNetCore.Antiforgery;
	using Microsoft.AspNetCore.Mvc;
	var builder = WebApplication.CreateBuilder(args);

	// Add services to the container.
	// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
	builder.Services.AddOpenApi();
	builder.Services.AddAntiforgery(); // Aggiunge il servizio Anti-Forgery
	var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.MapOpenApi();
	}

	app.UseHttpsRedirection();
	app.UseAntiforgery(); // Aggiunge middleware Anti-Forgery

	app.MapGet("/", (IAntiforgery antiforgery, HttpContext context) => // Inietta HttpContext come parametro
	{
		var tokens = antiforgery.GetAndStoreTokens(context);

		return Results.Content(
		$$"""
		<!DOCTYPE html>
		<html lang="it">

		<head>
		<meta charset="utf-8">
			<title>Form protetto con Anti-Forgery</title>
		</head>
		<body>
			<h1>Form protetto con Anti-Forgery</h1>
			<form id="antiforgeryForm" method="post" action="/submit-anti-forgery">
				<input type="hidden" name="{{tokens.FormFieldName}}" value="{{tokens.RequestToken}}">  <label for="messaggio">Messaggio:</label><br>
				<input type="text" name="messaggio"><br><br>
				<input type="submit" value="Invia">
			</form>

			<script>
				document.getElementById('antiforgeryForm').addEventListener('submit', function(event) {
					event.preventDefault();

					const formData = new FormData(this);

					fetch('/submit-anti-forgery', {
						method: 'POST',
						headers: {
							'Content-Type': 'application/x-www-form-urlencoded',
							'RequestVerificationToken': '{{tokens.RequestToken}}' // Alternativa: Invia token come header (invece che campo form) - RIMUOVERE se si usa il campo nascosto nel form
						},
						body: new URLSearchParams(formData).toString()
					})
					.then(response => response.text())
					.then(data => {
						alert(data);
					});
				});
			</script>
		</body>
		</html>
		""", "text/html");
	});


	app.MapPost("/submit-anti-forgery", async ([FromForm] string messaggio, IAntiforgery antiforgery, HttpContext context) =>
	{
		try
		{
			await antiforgery.ValidateRequestAsync(context);
			return Results.Ok($"Messaggio ricevuto: {messaggio} (Anti-Forgery validato)");
		}
		catch (AntiforgeryValidationException)
		{
			return Results.BadRequest("Invalid anti-forgery token");
			// Alternativa: return Results.StatusCode(403); // Forbidden
		}
	});

	app.MapGet("/anti-forgery2", (IAntiforgery antiforgery, HttpContext context) =>
	{
		var tokens = antiforgery.GetAndStoreTokens(context);

		return Results.Content(
		$$"""
		<!DOCTYPE html>
		<html lang="it">

		<head>
		<meta charset="utf-8">
			<title>Form protetto con Anti-Forgery</title>
		</head>
		<body>
			<h1>Form protetto con Anti-Forgery</h1>
			<form id="antiforgeryForm2" method="post" action="/submit-anti-forgery2">
				<input type="hidden" name="{{tokens.FormFieldName}}" value="{{tokens.RequestToken}}">
				<label for="messaggio">Messaggio:</label><br>
				<input type="text" name="messaggio"><br><br>
				<label for="name">Nome:</label><br>
				<input type="text" name="name" required/><br>
				<label for="dueDate">Data:</label><br>
				<input type="date" name="dueDate" value=""/><br>
				<label for="isCompleted">Completato:</label>
				<input type="checkbox" name="isCompleted"/><br><br>
				<input type="submit" value="Invia">
			</form>

			<script>
				document.getElementById('antiforgeryForm2').addEventListener('submit', function(event) {
				event.preventDefault();

				const formData = new FormData(this);
				const dueDateInput = document.querySelector('input[name="dueDate"]');
				
				// Rimuoviamo il campo data se è vuoto per evitare errori di binding
				//infatti se la data non viene selezionata il campo viene inviato come stringa vuota
				//e la stringa vuota non può essere convertita in un oggetto DateTime
				if (!dueDateInput.value) {
					formData.delete('dueDate');
				}
				// Aggiungiamo il campo isCompleted come booleano
				formData.set('isCompleted', document.querySelector('input[name="isCompleted"]').checked);

				fetch('/submit-anti-forgery2', {
					method: 'POST',
					headers: {
						'RequestVerificationToken': '{{tokens.RequestToken}}' // Alternativa: Invia token come header (invece che campo form) - RIMUOVERE se si usa il campo nascosto nel form
					},
					body: formData
				})
				.then(response => {
					if (!response.ok) {
						throw new Error('Network response was not ok');
					}
					return response.json();
				})
				.then(data => {
					alert(JSON.stringify(data, null, 2));
				})
				.catch(error => {
					console.error('Error:', error);
					alert('Si è verificato un errore durante l\'invio del form');
				});
			});
			</script>
		</body>
		</html>
		""", "text/html");
	});

	//in questo esempio il campo (Name = "messaggio") non è obbligatorio. 
	//Può essere utilizzato quando si vuole collegare un parametro con un nome specifico ad una variabile
	//dell'endpoint handler
	app.MapPost("/submit-anti-forgery2", async ([FromForm(Name = "messaggio")] string messaggio, [FromForm] Todo todo, IAntiforgery antiforgery, HttpContext context) =>
	{
		try
		{
			await antiforgery.ValidateRequestAsync(context);

			var response = new { todo, message = messaggio };
			return Results.Ok(response);
		}
		catch (AntiforgeryValidationException)
		{
			return Results.BadRequest("Invalid anti-forgery token");
		}
		catch (Exception ex)
		{
			return Results.BadRequest($"Error during form processing: {ex.Message}");
		}
	});


	app.Run();

	class Todo
	{
		public required string Name { get; set; }
		public bool IsCompleted { get; set; }
		public DateTime? DueDate { get; set; }
	}
	```

2. **Spiegazione:**

   1. **`builder.Services.AddAntiforgery();`**: Registra il servizio Anti-Forgery nel container DI (Dependency Injection).
   2. **`app.UseAntiforgery();`**: Abilita il middleware Anti-Forgery per l'applicazione.  Questo middleware gestisce la generazione e la validazione dei token.
   3. **Endpoint `/` (GET):**
   	  * Inietta il servizio `IAntiforgery` nell'endpoint.
   	  * `antiforgery.GetAndStoreTokens(context)`: **Genera un nuovo set di Anti-Forgery Tokens** (RequestToken e FormFieldName) e li memorizza nella sessione (o cookie).  Restituisce un oggetto `AntiforgeryTokenSet` contenente i token.
   	  * **HTML**:
   		  * **` <input type="hidden" name="{{tokens.FormFieldName}}" value="{{tokens.RequestToken}}"> `**:  **INSERISCE IL TOKEN COME CAMPO NASCOSTO NEL FORM.**  `tokens.FormFieldName` e `tokens.RequestToken` vengono interpolati nella stringa HTML, inserendo dinamicamente il nome del campo e il valore del token.
   		  * **JavaScript (Opzionale - da commentare/rimuovere se si usa il campo nascosto):**  `'RequestVerificationToken': '{{tokens.RequestToken}}'`  **Alternativa (NON usare contemporaneamente al campo nascosto):**  Invia il token come header HTTP.  In questo esempio, l'uso del campo nascosto è preferibile per la compatibilità con form standard.
   4. **Endpoint `/submit-anti-forgery` (POST):**
   	  * Inietta `IAntiforgery` nell'endpoint.
   	  * `await antiforgery.ValidateRequestAsync(context);`:  **VALIDA L'ANTI-FORGERY TOKEN** ricevuto con la richiesta. Se la validazione fallisce (token mancante, non valido, non corrispondente), verrà lanciata un'eccezione (`AntiforgeryValidationException`) e la richiesta verrà rifiutata.
   	  * Se la validazione ha successo, l'endpoint continua l'elaborazione (in questo esempio, restituisce un messaggio di successo).

3. **Per testare l'Anti-Forgery:**

   1. Creare un nuovo progetto ASP.NET Core Empty Web App.
   2. Sostituire il contenuto di `Program.cs` con il codice C\# fornito.
   3. Eseguire l'applicazione.
   4. Aprire il browser e vai all'indirizzo `https://localhost:<port>`.
   5. Inserire un messaggio nel form e clicca "Invia". Si dovrebbe vedere un alert di successo.
   6. **Provare ad attaccare (CSRF test)**:
   	  * Aprire gli strumenti di sviluppo del browser (F12) e copia l'HTML del form dalla pagina caricata (elemento `<form id="antiforgeryForm">`).
   	  * Creare un **secondo progetto** ASP.NET Core vuoto (questo simula un sito malevolo).
   	  * In `Program.cs` del **secondo progetto**, creare un endpoint GET (`/attack`) che restituisce una pagina HTML. Incolla l'HTML del form copiato nel body della risposta di questo endpoint. **RIMUOVI la riga `<input type="hidden" ...>` dal form in questo secondo progetto (simula un form malevolo senza Anti-Forgery Token)**.
   	  * Eseguire **entrambe** le applicazioni (quella originale e quella "malevola").
   	  * Aprire il browser e vai all'indirizzo dell'applicazione "malevola" (es. `https://localhost:<altro_porta>/attack`).
   	  * Inserire un messaggio nel form del sito "malevolo" e clicca "Invia".
   	  * **Si dovrebbe vedere un errore nella console del browser** (e potenzialmente un errore lato server nella prima applicazione) **perché la validazione dell'Anti-Forgery Token fallirà** (il token è mancante nel form malevolo).  Se la validazione non fallisse, saresti vulnerabile a CSRF.

### Best Practice per l'Anti-Forgery in Minimal APIs

  * **Abilitare sempre Anti-Forgery per form POST, PUT, DELETE**: Proteggere tutti gli endpoint che modificano dati (non solo quelli dei form, ma anche API che ricevono JSON, XML, ecc.) con Anti-Forgery.
  * **Validare sempre il token negli endpoint**: Non si dimentichi di chiamare `antiforgery.ValidateRequestAsync()` in tutti gli endpoint che processano form protetti.
  * **Utilizzare HTTPS**: Anti-Forgery si basa su cookie per la sessione. HTTPS è fondamentale per proteggere i cookie di sessione e prevenire attacchi man-in-the-middle.
  * **Non disabilitare Anti-Forgery globalmente senza motivo**:  La protezione Anti-Forgery è un importante meccanismo di sicurezza. Disabilitala solo se hai ragioni specifiche e sei consapevole dei rischi.
  * **Considerare CORS (Cross-Origin Resource Sharing)**: Se l'applicazione espone API utilizzate da siti web di terze parti, configura correttamente CORS per controllare quali siti possono accedere alle tue API. CORS è complementare all'Anti-Forgery e gestisce scenari diversi.

**Link Utili:**

  * [OWASP: Cross-Site Request Forgery (CSRF)](https://owasp.org/www-community/attacks/csrf)
  * [Documentazione ufficiale Microsoft: Prevenire attacchi Cross-Site Request Forgery (CSRF) in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery) (Protezione Anti-Forgery)
  
-----
 Gestire l'Anti-Forgery Token in un'architettura con frontend separato (statico HTML/CSS/JS) e backend Minimal API .NET richiede un approccio leggermente diverso rispetto a quando il frontend e il backend sono più strettamente integrati (ad esempio, con server-side rendering).

## La sfida del Frontend separato

Nel caso di frontend separato, non abbiamo più la facilità di inserire direttamente l'Anti-Forgery Token all'interno di un form HTML generato lato server, come abbiamo visto nell'esempio precedente con le Minimal API che restituivano HTML.  Il frontend statico deve **richiedere** il token al backend e poi **inviarlo** nuovamente ad ogni richiesta che necessita di protezione CSRF.

### Flusso di lavoro in un Frontend separato con Anti-Forgery

Il flusso di lavoro tipico per la gestione dell'Anti-Forgery in questo scenario è il seguente:

1. **Richiesta del Token (Frontend):**  Quando l'applicazione frontend si carica (o quando necessario), invia una richiesta **GET** speciale al backend per ottenere un Anti-Forgery Token. Questo endpoint backend è progettato unicamente per fornire il token.
2. **Risposta con Token (Backend):** Il backend Minimal API genera un Anti-Forgery Token e lo restituisce nella risposta della richiesta GET.  Solitamente, il token viene inviato come parte della risposta, ad esempio in un header HTTP personalizzato o nel corpo JSON della risposta.
3. **Memorizzazione del Token (Frontend):**  Il frontend JavaScript riceve il token e lo memorizza. Dove memorizzarlo dipende dalla durata del token e dal contesto dell'applicazione. Opzioni comuni sono:
	* **Memoria JavaScript:** La variabile JavaScript mantiene il token in memoria fino a quando la pagina non viene chiusa o ricaricata. Semplice ma il token si perde se la pagina viene ricaricata.
	* **Session Storage:** `sessionStorage` del browser. Il token persiste per la durata della sessione del browser (fino alla chiusura della finestra/tab).
	* **Local Storage:** `localStorage` del browser. Il token persiste anche dopo la chiusura del browser.  Generalmente **non è raccomandato** memorizzare token sensibili in `localStorage` per ragioni di sicurezza (rischio XSS), ma per Anti-Forgery token che hanno una validità limitata e sono meno sensibili dei token di autenticazione, potrebbe essere accettabile in alcuni contesti, se gestito con attenzione. Per semplicità, nell'esempio useremo la memoria JavaScript.
4. **Inclusione del Token nelle Richieste (Frontend):**  Ogni volta che il frontend deve inviare una richiesta al backend che necessita di protezione CSRF (tipicamente richieste `POST`, `PUT`, `DELETE` che modificano dati), deve **includere l'Anti-Forgery Token** nella richiesta.  Il modo più comune per fare ciò in un contesto API è tramite un **header HTTP personalizzato**. L'header standard che ASP.NET Core si aspetta per l'Anti-Forgery Token è `RequestVerificationToken`.
5. **Validazione del Token (Backend):** L'endpoint backend Minimal API che riceve la richiesta protetta deve **validare l'Anti-Forgery Token** presente nell'header `RequestVerificationToken`.  La validazione avviene nello stesso modo dell'esempio precedente, utilizzando `antiforgery.ValidateRequestAsync(context)`.

#### Esempio di codice completo (Frontend Statico + Backend Minimal API Anti-Forgery)

**Struttura del progetto (ipotetica):**

```text
wwwroot/  (Cartella per il frontend statico)
|-- index.html
|-- script.js
|-- style.css

backend-minimal-api/ (Cartella per il backend Minimal API .NET)
|-- Program.cs
|-- backend-minimal-api.csproj
```

1. **Backend Minimal API (.NET - `backend-minimal-api/Program.cs`)**
   Il codice di questo esempio è nel progetto [SeparatedFrontedBackend](../../../api-samples/minimal-api/BindingDemos/AntiForgeryDemos/SeparatedFrontendBackend/)

	```csharp
	using Microsoft.AspNetCore.Antiforgery;
	using Microsoft.AspNetCore.Mvc;
	var builder = WebApplication.CreateBuilder(args);

	// Add services to the container.
	// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
	builder.Services.AddOpenApi();
	builder.Services.AddAntiforgery(); // Aggiunge il servizio Anti-Forgery
	var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.MapOpenApi();
	}

	app.UseHttpsRedirection();
	app.UseDefaultFiles();
	app.UseStaticFiles();
	app.UseAntiforgery(); // Aggiunge middleware Anti-Forgery

	// Endpoint per ottenere l'Anti-Forgery Token (GET)
	app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
	{
		var tokens = antiforgery.GetAndStoreTokens(context);
		return Results.Text(tokens.RequestToken ?? ""); // Restituisce solo il RequestToken come testo
	});

	// Endpoint protetto da Anti-Forgery (POST)
	app.MapPost("/api/submitData", async ([FromForm] string messaggio, IAntiforgery antiforgery, HttpContext context) =>
	{
		try
		{

			await antiforgery.ValidateRequestAsync(context);
			return Results.Ok(new { message = $"Messaggio ricevuto: {messaggio} (Anti-Forgery validato)" });
		}

		catch (AntiforgeryValidationException)
		{
			return Results.BadRequest("Invalid anti-forgery token");
		}
		catch (Exception ex)
		{
			return Results.BadRequest($"Error during form processing: {ex.Message}");
		}

	})
	.Accepts<Dictionary<string, string>>("application/x-www-form-urlencoded"); // Specifica che accetta form URL-encoded

	app.Run();
	```

   **Spiegazione Backend:**

	  * **`/antiforgery/token` (GET Endpoint):**
	  	* Questo endpoint è dedicato a fornire l'Anti-Forgery Token.
	  	* Utilizza `IAntiforgery` per generare i token.
	  	* **Restituisce solo il `RequestToken` come testo semplice** (`Results.Text(tokens.RequestToken ?? "")`).  Potresti anche restituirlo in JSON se preferisci, ma per semplicità qui lo restituiamo come testo. **È importante restituire solo il `RequestToken` e non l'intero `AntiforgeryTokenSet` in questo scenario di frontend separato.**
	  	* **Questo endpoint *non* necessita di protezione Anti-Forgery**, poiché il suo scopo è proprio fornire il token.
	  * **`/api/submitData` (POST Endpoint):**
	  	* Questo è l'endpoint che processa i dati del form e deve essere protetto da CSRF.
	  	* Utilizza `[FromForm]` per ricevere i dati dal form (anche se in questo esempio semplice inviamo un solo campo `messaggio`).
	  	* **`await antiforgery.ValidateRequestAsync(HttpContext.Current);`**:  Valida l'Anti-Forgery Token.
	  	* Restituisce un JSON con un messaggio di successo.
	  	* **.Accepts\<Dictionary\<string, string>>("application/x-www-form-urlencoded")**: Specifica che l'endpoint si aspetta ricevere dati in formato `application/x-www-form-urlencoded` (il formato standard dei form HTML).

2. **Frontend Statico (`wwwroot/index.html`, `wwwroot/script.js`)**

	**`wwwroot/index.html`:**

	```html
	<!DOCTYPE html>
	<html lang="it">

	<head>
		<meta charset="utf-8">
		<meta name="viewport" content="width=device-width, initial-scale=1.0">
		<title>Frontend Statico con Anti-Forgery</title>
	</head>

	<body>
		<h1>Frontend Statico con Anti-Forgery</h1>
		<form id="dataForm">
			<label for="messaggio">Messaggio:</label><br>
			<input type="text" id="messaggio" name="messaggio"><br><br>
			<button type="submit">Invia Dati Protetti</button>
		</form>

		<script src="script.js"></script>
	</body>

	</html>
	```

	**`wwwroot/script.js`:**

	```javascript
	document.addEventListener("DOMContentLoaded", function () {
	let antiforgeryToken = null; // Variabile per memorizzare l'Anti-Forgery Token

	// ottiene l'Anti-Forgery Token dal backend al caricamento della pagina o del form
	fetch("/antiforgery/token")
		.then((response) => response.text())
		.then((token) => {
		antiforgeryToken = token; // Memorizza il token
		console.log("Anti-Forgery Token ottenuto:", antiforgeryToken);
		})
		.catch((error) => {
		console.error("Errore nel recupero del token Anti-Forgery:", error);
		alert(
			"Errore nel recupero del token Anti-Forgery. L'applicazione potrebbe non funzionare correttamente."
		);
		});

	const dataForm = document.getElementById("dataForm");
	dataForm.addEventListener("submit", async function (event) {
		event.preventDefault();

		const messaggioInput = document.getElementById("messaggio");
		const messaggio = messaggioInput.value;

		const formData = new FormData();
		formData.append("messaggio", messaggio); // Prepara i dati del form
	
		//Si potrebbe richiedere il token anche prima di sottomettere il form
		
		//Decidere quale opzione applicare (se richiederlo al caricamento del form oppure prima 
		//della sottomissione del form) dipende da come si gestiscono i token e le pagine del frontend
		
		//Il codice commentato sotto richiede il token prima di sottomettere il form
		//va usato in alternativa alla richiesta del token al caricamento della pagina/form
		
		// try {
		// 	// Prima richiedi il token usando await
		// 	const tokenResponse = await fetch("/antiforgery/token2");
		// 	antiforgeryToken = await tokenResponse.text();
		// } catch (error) {
		// 	console.error("Errore nel recupero del token:", error);
		// 	alert("Errore nel recupero del token. Riprova più tardi.");
		// 	return;
		// }

		// Sottomissione del form con il pattern .then() originale
		fetch("/api/submitData", {
			method: "POST",
			headers: {
				"Content-Type": "application/x-www-form-urlencoded",
				RequestVerificationToken: antiforgeryToken,
			},
			body: new URLSearchParams(formData).toString(),
		})
			.then((response) => response.json())
			.then((data) => {
				alert(data.message);
			})
			.catch((error) => {
				console.error("Errore nell'invio dei dati:", error);
				alert("Errore nell'invio dei dati. Riprova più tardi.");
			});
		});
	});
	```

	**Spiegazione Frontend:**

	* **`document.addEventListener('DOMContentLoaded', ...)`:** Assicura che lo script venga eseguito dopo che la pagina HTML è stata completamente caricata.
	* **`let antiforgeryToken = null;`:**  Dichiara una variabile per memorizzare l'Anti-Forgery Token.
	* **`fetch('/antiforgery/token')...` (Richiesta GET per il token):**
		* All'avvio della pagina, invia una richiesta GET a `/antiforgery/token` sul backend per ottenere il token.
		* Quando la risposta arriva (successo), estrae il token come testo (`response.text()`) e lo memorizza in `antiforgeryToken`.
		* Gestisce gli errori nel recupero del token.
	* **`dataForm.addEventListener('submit', ...)` (Gestione Submit Form):**
		* Intercetta l'evento di submit del form.
		* Previene il comportamento predefinito del form (`event.preventDefault()`).
		* Ottiene il valore del campo `messaggio`.
		* Crea un `FormData` object e aggiunge il campo `messaggio`.
		* **`fetch('/api/submitData', ...)` (Richiesta POST protetta):**
			* Invia una richiesta POST a `/api/submitData`.
			* **`headers: { 'RequestVerificationToken': antiforgeryToken }`**:  **INCLUDI L'ANTI-FORGERY TOKEN NELL'HEADER `RequestVerificationToken`**.
			* `'Content-Type': 'application/x-www-form-urlencoded'`: Specifica il content type corretto per i form standard.
			* `body: new URLSearchParams(formData).toString()`: Converte `FormData` in formato `application/x-www-form-urlencoded` per l'invio con `fetch`.
			* Gestisce la risposta JSON dal backend e mostra un alert con il messaggio.
			* Gestisce gli errori nell'invio dei dati.

3. **Testare l'esempio (Frontend Statico + Backend Minimal API Anti-Forgery)**

   1. **Avviare il backend Minimal API:**  Eseguire il progetto `backend-minimal-api`. Dovrebbe avviarsi su un indirizzo come `https://localhost:<porta_backend>`.
   2. **Aprire `frontend-static/index.html` nel browser:**  Aprire il file `index.html` **direttamente** nel browser (non tramite un server web, per semplicità in questo test). Poiché è un frontend statico, può essere aperto direttamente dal file system.
   3. **Interazione:**
	  	* Quando la pagina `index.html` si carica, si dovrebbe vedere un messaggio nella console del browser "Anti-Forgery Token ottenuto: ..." (se il backend è in esecuzione).
	  	* Inserire un messaggio nel campo di testo e cliccare "Invia Dati Protetti".
	  	* Dovresti vedere un alert JavaScript con la risposta JSON dal backend, confermando che il messaggio è stato ricevuto e l'Anti-Forgery validato.
   4. **Test di attacco CSRF (simulato):**
   	* Analogamente all'esempio precedente, creare un **secondo progetto frontend statico** (o modificare temporaneamente `frontend-static/index.html`).
   	* **Rimuovere completamente la parte di codice JavaScript che recupera e include l'Anti-Forgery Token** (sia la richiesta GET a `/antiforgery/token` che l'inclusione del token nell'header della richiesta POST).  In pratica, simula un sito malevolo che non conosce o non utilizza l'Anti-Forgery Token.
   	* Aprire questo **secondo frontend "malevolo"** nel browser.
   	* Provare ad inviare il form. **Si dovrebbe ricevere un errore (o una risposta di errore dal backend nella console del browser)** perché la validazione dell'Anti-Forgery Token fallirà sul backend.  Questo dimostra che la protezione CSRF sta funzionando.

#### Considerazioni importanti e best practice per Frontend separato e Anti-Forgery

* **HTTPS è Fondamentale:** Anche in questo scenario, HTTPS è **essenziale** per proteggere la comunicazione tra frontend e backend, inclusa la trasmissione dell'Anti-Forgery Token e dei cookie di sessione (se utilizzati).
* **Gestione degli Errori:**  Implementare una gestione robusta degli errori sia nel frontend che nel backend. Se il token non può essere recuperato, o la validazione fallisce, l'applicazione dovrebbe gestire correttamente la situazione e informare l'utente.
* **Durata del Token:** Considerare la durata di validità dell'Anti-Forgery Token.  ASP.NET Core genera token che sono validi per la sessione per impostazione predefinita. Se necessario, si può configurare la durata del token.  Se i token hanno una durata breve, si potrebbe aver bisogno di meccanismi di refresh del token (anche se per Anti-Forgery token, di solito non è necessario un refresh continuo come per i token di autenticazione JWT).
* **Sicurezza del Token nel Frontend:**  Sebbene `localStorage` possa essere un'opzione per persistere il token, si devono valutare attentamente i rischi di XSS (Cross-Site Scripting). In molti casi, memorizzare il token in memoria JavaScript (come nell'esempio) o in `sessionStorage` è sufficiente e più sicuro, poiché il token è legato alla sessione dell'utente e ha una validità limitata.
* **Documentazione API:**  Documentare chiaramente che le API protette si aspettano l'Anti-Forgery Token nell'header `RequestVerificationToken` e che il frontend deve prima richiedere il token all'endpoint `/antiforgery/token`.

#### Perché l'invio del solo `RequestToken` è la scelta ottimale nel caso di Frontend separato dal Backend?

È fondamentale capire perché, nello scenario di frontend separato, restituire **solo il `RequestToken`** dall'endpoint `/antiforgery/token` sia la scelta corretta, e perché inviare l'intero `AntiforgeryTokenSet` sarebbe inappropriato o addirittura problematico.

Per chiarire al meglio, ripercorriamo i concetti chiave e le differenze tra i due scenari principali di gestione dell'Anti-Forgery Token:

##### Differenze tra lo scenario con Frontend e Backend integrati e quello con Frontend e Backend separati

1. **Scenario con Frontend e Backend integrati (Server-Side Rendering, Es. Minimal API che restituisce HTML)**

	  * In questo scenario, il backend è responsabile sia della generazione della pagina HTML che della gestione degli endpoint API.
	  * Quando il backend genera la pagina HTML contenente il form, utilizza il servizio `IAntiforgery` per ottenere l'`AntiforgeryTokenSet`.
	  * L'`AntiforgeryTokenSet` contiene **due informazioni principali**:
	  	* **`RequestToken`**:  Il valore effettivo del token di protezione CSRF.
	  	* **`FormFieldName`**:  Il **nome** del campo nascosto (`<input type="hidden" name="...">`) che verrà inserito nel form HTML per contenere il `RequestToken`.

	  * Quando il backend restituisce la pagina HTML al browser, **inserisce entrambi** questi elementi nel form:
	  	* Il `RequestToken` come `value` dell'input nascosto.
	  	* Il `FormFieldName` come `name` dell'input nascosto.  Questo è cruciale perché ASP.NET Core, quando convalida l'Anti-Forgery Token in un endpoint che riceve un form, **si aspetta di trovare il token in un campo del form con un *nome* specifico**. Questo nome specifico è proprio il `FormFieldName`.

	  * **Esempio di codice HTML generato dal backend nel caso integrato:**

	  	```html
	  	<form method="post" action="/submitForm">
	  		<input type="hidden" name="__RequestVerificationToken" value="[VALORE_DEL_REQUEST_TOKEN]"> <label for="messaggio">Messaggio:</label><br>
	  		<input type="text" id="messaggio" name="messaggio"><br><br>
	  		<input type="submit" value="Invia">
	  	</form>
	  	```

	In questo esempio, `__RequestVerificationToken` è un valore tipico di `FormFieldName` (ma può essere configurato), e `[VALORE_DEL_REQUEST_TOKEN]` è il `RequestToken`.

2. **Scenario con Frontend separato (Statico HTML/JS/CSS + Backend Minimal API)**

	  * In questo scenario, il frontend (HTML, CSS, JavaScript) è completamente separato dal backend Minimal API.
	  * Il frontend è responsabile della gestione dell'interfaccia utente, delle interazioni con l'utente e dell'invio di richieste al backend.
	  * Il backend Minimal API è responsabile della logica applicativa, della gestione dei dati e della sicurezza, inclusa la protezione CSRF.
	  * **Non utilizziamo form HTML generati lato server e campi nascosti per l'Anti-Forgery Token.** Invece, adottiamo l'approccio di inviare il `RequestToken` come **header HTTP** (`RequestVerificationToken`).

	  * **Perché restituire solo il `RequestToken` dall'endpoint `/antiforgery/token`?**

	  	* **`FormFieldName` non è rilevante:**  Il `FormFieldName` è significativo **solo** quando si utilizza il meccanismo tradizionale di inserire l'Anti-Forgery Token come campo nascosto all'interno di un form HTML. Nel nostro scenario di frontend separato, **non stiamo usando campi nascosti nei form per l'Anti-Forgery Token**. Invece, inviamo il token come header HTTP.  Pertanto, il `FormFieldName` diventa superfluo e non necessario per il frontend.
	  	* **Semplicità e chiarezza:** Restituire solo il `RequestToken` rende l'API `/antiforgery/token` più semplice e focalizzata sul suo scopo: fornire il valore del token.  Il frontend si aspetta di ricevere un semplice token stringa da utilizzare nell'header. Inviare l'intero `AntiforgeryTokenSet` sarebbe eccessivo e potrebbe creare confusione nel frontend su come interpretare e utilizzare `FormFieldName` in un contesto dove non è pertinente.
	  	* **Prevenire usi impropri (anche se meno probabili):** Anche se meno probabile, se il frontend ricevesse l'intero `AntiforgeryTokenSet`, potrebbe erroneamente tentare di utilizzare `FormFieldName` in modi non previsti in un'architettura basata su header, magari cercando di manipolare i form HTML lato client in modo errato. Restituire solo il `RequestToken` riduce la superficie di possibili fraintendimenti o usi non corretti.
	  	* **Minimizzare le informazioni esposte:** In generale, è una buona pratica per le API esporre solo le informazioni strettamente necessarie per il client. In questo caso, il frontend ha bisogno solo del valore del `RequestToken` per inserirlo nell'header.  Inviare informazioni aggiuntive come `FormFieldName`, che non vengono utilizzate, non apporta alcun beneficio e potrebbe essere considerato un'esposizione non necessaria di dettagli interni.

	  * **Cosa fa il frontend con il `RequestToken` ricevuto?**

	  	* Il frontend JavaScript, dopo aver ricevuto il `RequestToken` dall'endpoint `/antiforgery/token`, lo memorizza (ad esempio, in una variabile JavaScript).
	  	* Successivamente, quando il frontend deve effettuare una richiesta protetta (POST, PUT, DELETE) al backend, **aggiunge il `RequestToken` come valore dell'header HTTP `RequestVerificationToken` nella richiesta**.

	  * **Lato Backend (Endpoint protetto - `/api/submitData` nel nostro esempio):**

	  	* Indipendentemente da come il token è stato inviato (sia come campo form nascosto nel caso integrato, sia come header nel caso separato), ASP.NET Core, quando si chiama `antiforgery.ValidateRequestAsync(context)`, si occupa di **verificare la validità del token in base a come è stato configurato il sistema Anti-Forgery**.  In genere, ASP.NET Core è configurato per cercare il token sia nel header `RequestVerificationToken` (per scenari API/frontend separati) che nel campo form con il `FormFieldName` (per scenari di server-side rendering).  Quindi, la validazione funziona correttamente in entrambi i casi, **purché il frontend invii il `RequestToken` correttamente nell'header.**

**In Sintesi:**

Nello scenario di frontend separato, il `FormFieldName` incluso nell'`AntiforgeryTokenSet` è **irrilevante** perché non stiamo utilizzando form HTML generati lato server e campi nascosti per l'Anti-Forgery Token.  Stiamo invece trasmettendo il `RequestToken` come header HTTP. Pertanto, l'endpoint `/antiforgery/token` restituisce **solo il `RequestToken`** per semplicità, chiarezza e per evitare possibili confusioni o usi impropri del `FormFieldName` in un contesto dove non è applicabile.  Il frontend ha bisogno solo del `RequestToken` per popolare l'header `RequestVerificationToken` delle richieste protette.

### Anti-Forgery Token con sessioni non abilitate sul backend

**Si può implementare il meccanismo dell'Anti-Forgery Token anche quando le sessioni non sono abilitate sul backend**.  Infatti, il sistema Anti-Forgery di ASP.NET Core è progettato per funzionare **principalmente in modo stateless**, senza dipendere dalle sessioni server-side per la memorizzazione dei token.
È importante chiarire che, sebbene le sessioni *possano* essere utilizzate per memorizzare l'Anti-Forgery Token (come abbiamo visto in precedenza con `antiforgery.GetAndStoreTokens`), **non sono un requisito indispensabile** per il funzionamento del meccanismo.

**Come funziona l'Anti-Forgery senza sessioni?**

Quando le sessioni non sono utilizzate, il sistema Anti-Forgery di ASP.NET Core si affida a un approccio **stateless e basato sui cookie**.  Ecco come funziona il flusso:

1. **Generazione del Token (Stateless):** Quando l'endpoint backend (ad esempio, `/antiforgery/token`) viene chiamato per richiedere un token, il servizio `IAntiforgery` genera un token Anti-Forgery.  Questo token **non viene memorizzato in una sessione server-side**.  Invece, il token generato contiene al suo interno tutte le informazioni necessarie per la sua validazione futura, tipicamente attraverso meccanismi di **crittografia e firma digitale**.

2. **Invio del Token al Client tramite Cookie:**  Oltre a restituire il `RequestToken` (come stringa di testo o JSON) al frontend (per l'header `RequestVerificationToken`), il backend, **in modo automatico e stateless**, imposta anche un **cookie HTTP** nella risposta. Questo cookie contiene una parte del token Anti-Forgery (spesso chiamato *cookie token* o *cookie di base*). Il nome di questo cookie è configurabile (di default, inizia con `.AspNetCore.Antiforgery`).

3. **Invio del Token con le Richieste (Frontend):** Il frontend, quando effettua una richiesta protetta (POST, PUT, DELETE) al backend, deve fare **due cose**:
	* Inviare il `RequestToken` (che ha ottenuto dall'endpoint `/antiforgery/token`) nell'**header HTTP `RequestVerificationToken`**, come abbiamo visto nell'esempio precedente.
	* Il browser **invierà automaticamente il cookie Anti-Forgery** (impostato dal backend in precedenza) insieme alla richiesta, **sempre che la richiesta sia fatta allo stesso dominio e percorso** per cui il cookie è valido.  Questo comportamento è intrinseco ai cookie HTTP.

4. **Validazione del Token (Stateless):** Quando l'endpoint backend protetto riceve la richiesta, e si chiama `antiforgery.ValidateRequestAsync(context)`, il sistema Anti-Forgery esegue la validazione **in modo stateless**.  La validazione consiste nel:
	* **Estrarre il `RequestToken` dall'header `RequestVerificationToken`**.
	* **Estrarre il cookie Anti-Forgery dalla richiesta** (se presente e valido per il dominio/percorso).
	* **Verificare che il `RequestToken` e il cookie Anti-Forgery siano *correlati* e *validi***.  La logica di correlazione e validazione si basa sulla crittografia e la firma digitale utilizzate nella generazione del token.  Poiché il token contiene tutte le informazioni necessarie, **non è necessario consultare alcuna sessione server-side**
**Vantaggi dell'approccio Stateless con Cookie:**

* **Scalabilità:**  Essere stateless rende il backend **più scalabile**, poiché non è necessario gestire la sessione per ogni utente sul server.  Questo è particolarmente importante in architetture distribuite o con un elevato numero di utenti.
* **Semplicità:**  Si evita la complessità della gestione e della persistenza delle sessioni server-side.
* **Compatibilità con architetture distribuite:**  In microservizi o architetture distribuite, la gestione delle sessioni condivise può essere complessa. L'approccio stateless con cookie elimina questa problematica.

**Esempio di codice Minimal API (senza sessioni esplicite):**

Modifichiamo leggermente l'esempio precedente per evidenziare che **non stiamo usando sessioni esplicite** e che l'Anti-Forgery continua a funzionare in modo stateless tramite cookie. In realtà, il codice precedente **già funzionava in modo stateless** di default, ma ora lo rendiamo ancora più esplicito:

**Program.cs (Backend Minimal API - Stateless Anti-Forgery):**
Il codice di questo esempio è nel progetto [SeparatedFrontedBackend](../../../api-samples/minimal-api/BindingDemos/AntiForgeryDemos/SeparatedFrontendBackend/)

```csharp
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAntiforgery(); // Aggiunge il servizio Anti-Forgery
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAntiforgery(); // Aggiunge middleware Anti-Forgery

// Endpoint per ottenere l'Anti-Forgery Token (GET) - Stateless
app.MapGet("/antiforgery/token2", (IAntiforgery antiforgery, HttpContext context) =>
{
	var tokens = antiforgery.GetTokens(context); // Utilizziamo GetTokens invece di GetAndStoreTokens
	return Results.Text(tokens.RequestToken ?? ""); // Restituisce solo il RequestToken come testo
});

// Endpoint protetto da Anti-Forgery (POST)
app.MapPost("/api/submitData", async ([FromForm] string messaggio, IAntiforgery antiforgery, HttpContext context) =>
{
	try
	{

		await antiforgery.ValidateRequestAsync(context);
		return Results.Ok(new { message = $"Messaggio ricevuto: {messaggio} (Anti-Forgery validato)" });
	}

	catch (AntiforgeryValidationException)
	{
		return Results.BadRequest("Invalid anti-forgery token");
	}
	catch (Exception ex)
	{
		return Results.BadRequest($"Error during form processing: {ex.Message}");
	}

})
.Accepts<Dictionary<string, string>>("application/x-www-form-urlencoded"); // Specifica che accetta form URL-encoded

app.Run();

```

**Modifiche rispetto all'esempio precedente:**

* Nell'endpoint `/antiforgery/token`, abbiamo sostituito `antiforgery.GetAndStoreTokens(context)` con `antiforgery.GetTokens(context)`.
	* `GetTokens()` **genera solo i token**, ma **non li memorizza nella sessione server-side**.  L'unico modo in cui i token vengono "memorizzati" è tramite l'impostazione del cookie HTTP nella risposta.
	* `GetAndStoreTokens()` (usato nell'esempio precedente) fa **sia** generare i token **che** memorizzarli nella sessione server-side (oltre a impostare il cookie).  Anche se `GetAndStoreTokens` funzionerebbe anche senza sessioni abilitate, `GetTokens` è più preciso e semanticamente corretto in un contesto stateless.

**Frontend Statico (`wwwroot/index2.html`, `wwwroot/script2.js`)**
Il codice di questo esempio è nel progetto [SeparatedFrontedBackend](../../../api-samples/minimal-api/BindingDemos/AntiForgeryDemos/SeparatedFrontendBackend/)

* **wwwroot/index2.html**

	```html
	<!DOCTYPE html>
	<html lang="it">

	<head>
		<meta charset="utf-8">
		<meta name="viewport" content="width=device-width, initial-scale=1.0">
		<title>Frontend Statico con Anti-Forgery</title>
	</head>

	<body>
		<h1>Frontend Statico con Anti-Forgery</h1>
		<form id="dataForm">
			<label for="messaggio">Messaggio:</label><br>
			<input type="text" id="messaggio" name="messaggio"><br><br>
			<button type="submit">Invia Dati Protetti</button>
		</form>

		<script src="script2.js"></script>
	</body>

	</html>
	```

* **wwwroot/script2.js**
  
  ```js
	document.addEventListener("DOMContentLoaded", function () {
	let antiforgeryToken = null; // Variabile per memorizzare l'Anti-Forgery Token

	// ottiene l'Anti-Forgery Token dal backend al caricamento della pagina o del form
	//accede all'endpoint stateless (non cambia nulla per il frontend)
	fetch("/antiforgery/token2")
		.then((response) => response.text())
		.then((token) => {
		antiforgeryToken = token; // Memorizza il token
		console.log("Anti-Forgery Token ottenuto:", antiforgeryToken);
		})
		.catch((error) => {
		console.error("Errore nel recupero del token Anti-Forgery:", error);
		alert(
			"Errore nel recupero del token Anti-Forgery. L'applicazione potrebbe non funzionare correttamente."
		);
		});

	const dataForm = document.getElementById("dataForm");
	dataForm.addEventListener("submit", async function (event) {
		event.preventDefault();

		const messaggioInput = document.getElementById("messaggio");
		const messaggio = messaggioInput.value;

		const formData = new FormData();
		formData.append("messaggio", messaggio); // Prepara i dati del form

		//Si potrebbe richiedere il token anche prima di sottomettere il form

		//Decidere quale opzione applicare (se richiederlo al caricamento del form oppure prima
		//della sottomissione del form) dipende da come si gestiscono i token e le pagine del frontend

		//Il codice commentato sotto richiede il token prima di sottomettere il form
		//va usato in alternativa alla richiesta del token al caricamento della pagina/form

		// try {
		// 	// Prima richiedi il token usando await
		// 	const tokenResponse = await fetch("/antiforgery/token2");
		// 	antiforgeryToken = await tokenResponse.text();
		// } catch (error) {
		// 	console.error("Errore nel recupero del token:", error);
		// 	alert("Errore nel recupero del token. Riprova più tardi.");
		// 	return;
		// }

		// Sottomissione del form con il pattern .then() originale
		fetch("/api/submitData", {
		method: "POST",
		headers: {
			"Content-Type": "application/x-www-form-urlencoded",
			RequestVerificationToken: antiforgeryToken,
		},
		body: new URLSearchParams(formData).toString(),
		})
		.then((response) => response.json())
		.then((data) => {
			alert(data.message);
		})
		.catch((error) => {
			console.error("Errore nell'invio dei dati:", error);
			alert("Errore nell'invio dei dati. Riprova più tardi.");
		});
		});
	});
  ```

Il codice frontend JavaScript e HTML rimane **identico** all'esempio precedente.  Il frontend non deve preoccuparsi se il backend usa sessioni o meno per l'Anti-Forgery.  Il frontend si limita a:

1. Richiedere il `RequestToken` all'endpoint `/antiforgery/token`.
2. Inviare il `RequestToken` nell'header `RequestVerificationToken` delle richieste protette.

#### Test (Stateless Anti-Forgery)

1. Eseguire il backend Minimal API modificato (`Program.cs` con `GetTokens`).
2. Aprire `wwwroot/index2.html` nel browser.
3. Interagire con il form (inserire il messaggio e inviare). Si dovrebbe vedere che l'Anti-Forgery continua a funzionare correttamente, nonostante il backend non utilizzi sessioni esplicite.
4. Provare nuovamente l'attacco CSRF simulato (creando un frontend "malevolo" senza token). Si dovrebbe verificare che la validazione fallisce e la protezione CSRF è attiva.

**Considerazioni importanti per l'Anti-Forgery Stateless con Cookie:**

* **HTTPS è *ancora più* fondamentale:**  Poiché il meccanismo stateless si basa sui cookie per la correlazione dei token, è **cruciale** utilizzare **HTTPS** per proteggere la trasmissione dei cookie e prevenire attacchi man-in-the-middle che potrebbero intercettare o manipolare i cookie Anti-Forgery. Senza sessioni server-side a rafforzare la sicurezza, la protezione offerta da HTTPS diventa ancora più importante.
* **Sicurezza dei cookie:** Assicurarsi che i cookie Anti-Forgery siano configurati con attributi di sicurezza appropriati:
	* **`HttpOnly`**:  Impedisce l'accesso ai cookie tramite JavaScript client-side, riducendo il rischio di attacchi XSS che potrebbero rubare i cookie.  (ASP.NET Core imposta `HttpOnly` di default per i cookie Anti-Forgery).
	* **`Secure`**:  Garantisce che il cookie venga trasmesso solo su connessioni HTTPS. (Si dovrebbe configurare ASP.NET Core per impostare `Secure` su `true` in ambienti di produzione HTTPS).
	* **`SameSite`**:  Aiuta a prevenire alcuni tipi di attacchi CSRF basati su cross-origin request.  Si consideri di impostare `SameSite` su `Lax` o `Strict` (a seconda delle esigenze della tua applicazione) per una protezione aggiuntiva.

**In conclusione:**

Implementare l'Anti-Forgery Token **senza sessioni server-side è non solo possibile, ma anche spesso preferibile** in termini di scalabilità e semplicità, specialmente per le Minimal API e le architetture moderne basate su frontend separati.  ASP.NET Core fornisce un sistema Anti-Forgery robusto e flessibile che supporta nativamente l'approccio stateless tramite cookie, offrendo una protezione efficace contro gli attacchi CSRF anche in assenza di sessioni server-side.  L'importante è configurare correttamente HTTPS e gli attributi di sicurezza dei cookie per massimizzare la protezione.
