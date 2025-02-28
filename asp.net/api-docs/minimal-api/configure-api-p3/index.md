# Guida introduttiva al Parameter Binding in ASP.NET Minimal APIs

- [Guida introduttiva al Parameter Binding in ASP.NET Minimal APIs](#guida-introduttiva-al-parameter-binding-in-aspnet-minimal-apis)
	- [Introduzione al Parameter Binding in ASP.NET Minimal APIs](#introduzione-al-parameter-binding-in-aspnet-minimal-apis)
	- [Binding di Form in ASP.NET Minimal APIs](#binding-di-form-in-aspnet-minimal-apis)
		- [Primo esempio (C#, HTML, CSS, JavaScript) - Dati da form con codifica `application/x-www-url-encoded`](#primo-esempio-c-html-css-javascript---dati-da-form-con-codifica-applicationx-www-url-encoded)
			- [Spiegazione del codice del primo endpoint - form con dati semplici](#spiegazione-del-codice-del-primo-endpoint---form-con-dati-semplici)
		- [Secondo esempio - invio di dati da form con codifica `multipart/form-data` e differenze rispetto alla codifica `application/x-www-form-urlencoded`](#secondo-esempio---invio-di-dati-da-form-con-codifica-multipartform-data-e-differenze-rispetto-alla-codifica-applicationx-www-form-urlencoded)
		- [Terzo esempio - invio di dati da form con codifica `application/json` e differenze rispetto alle codifiche `application/x-www-form-urlencoded` e `multipart/form-data`](#terzo-esempio---invio-di-dati-da-form-con-codifica-applicationjson-e-differenze-rispetto-alle-codifiche-applicationx-www-form-urlencoded-e-multipartform-data)
		- [Confronto tra le tre soluzioni di invio dati al server](#confronto-tra-le-tre-soluzioni-di-invio-dati-al-server)
			- [Perché nel terzo caso non si usa né `URLSearchParams` né `FormData`?](#perché-nel-terzo-caso-non-si-usa-né-urlsearchparams-né-formdata)
		- [Motivazioni principali per l'uso di JSON](#motivazioni-principali-per-luso-di-json)
		- [Quando usare ciascun metodo?](#quando-usare-ciascun-metodo)
		- [`[FromForm]` vs `[AsParameters]`: Differenze e quando usarli](#fromform-vs-asparameters-differenze-e-quando-usarli)
			- [Esempio comparativo (C#) - `[FromForm]` vs `[AsParameters]`](#esempio-comparativo-c---fromform-vs-asparameters)
			- [Best practice per la gestione dei form in Minimal APIs](#best-practice-per-la-gestione-dei-form-in-minimal-apis)
	- [Binding esplicito in ASP.NET Minimal API](#binding-esplicito-in-aspnet-minimal-api)
		- [Binding da Header con `[FromHeader]`](#binding-da-header-con-fromheader)
		- [Binding dalla Query String con `[FromQuery]`](#binding-dalla-query-string-con-fromquery)
		- [Binding dalla Route con `[FromRoute]`](#binding-dalla-route-con-fromroute)
		- [Binding dai Servizi Iniettati con `[FromServices]`](#binding-dai-servizi-iniettati-con-fromservices)
		- [Accesso Diretto a `HttpRequest`](#accesso-diretto-a-httprequest)
		- [Accesso Diretto ai Servizi Iniettati](#accesso-diretto-ai-servizi-iniettati)
	- [Meccanismi avanzati di Binding (cenni)](#meccanismi-avanzati-di-binding-cenni)
	- [Ordine nell'applicazione delle regole di Binding](#ordine-nellapplicazione-delle-regole-di-binding)
	- [Conclusioni e best practice generali sul Parameter Binding](#conclusioni-e-best-practice-generali-sul-parameter-binding)

## Introduzione al Parameter Binding in ASP.NET Minimal APIs

Il **Parameter Binding** è il processo mediante il quale ASP.NET Core trasferisce i dati dalle richieste HTTP in arrivo (come URL, header, body, form) ai parametri dei metodi degli **endpoint** nelle Minimal APIs. Questo meccanismo permette agli sviluppatori di interagire con i dati della richiesta in modo semplice e dichiarativo, senza doverli estrarre manualmente dal contesto HTTP.

In ASP.NET Minimal APIs, il binding è in gran parte **automatico** e basato sul **tipo** dei parametri degli endpoint. Il framework deduce automaticamente la sorgente dei dati in base al tipo di parametro. Ad esempio:

  * Tipi semplici (stringhe, numeri, booleani, date) vengono spesso presi dalla **route** (URL) o dalla **query string**.
  * Tipi complessi (classi, record) possono essere deserializzati dal **body** della richiesta (ad esempio, JSON o XML).
  * Dati provenienti dai **form** HTML richiedono un trattamento specifico, che vedremo in dettaglio.

**Link Utili:**

  * [Documentazione ufficiale Microsoft: Parameter binding in Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding)

-----

## Binding di Form in ASP.NET Minimal APIs

1. **Cos'è un Form HTML e come funziona**

	Un **form HTML** è un elemento fondamentale del web per raccogliere input dagli utenti. È composto da diversi controlli (campi di testo, checkbox, dropdown, bottoni, ecc.) e, quando viene *sottomesso* (submit), invia i dati inseriti al server tramite una richiesta HTTP.  I form possono utilizzare due metodi HTTP principali per l'invio dei dati:

	* **GET**: I dati del form vengono codificati e aggiunti alla **query string** dell'URL. Questo metodo è adatto per azioni "sicure" che non modificano lo stato del server (es. ricerche, visualizzazioni).
	* **POST**: I dati del form vengono inclusi nel **body** della richiesta HTTP. Questo metodo è più adatto per azioni che modificano lo stato del server (es. creazione di nuovi dati, aggiornamenti, invio di file).

	Per la gestione dei form in ASP.NET Minimal APIs, ci concentreremo principalmente sul metodo **POST**, che è il più comune per l'invio di dati strutturati e per azioni che modificano lo stato.

2. **Binding di dati semplici da Form**

	Vediamo come ASP.NET Core gestisce il binding di dati semplici provenienti da un form.

3. **Utilizzo implicito del Binding da Form per tipi semplici**

	In molti casi, per tipi semplici come stringhe e numeri, ASP.NET Core può dedurre automaticamente che i dati provengono dal form **senza necessità di attributi specifici** come `[FromForm]`, specialmente se i dati sono inviati con `Content-Type: application/x-www-form-urlencoded`.  Tuttavia, **per una maggiore chiarezza e controllo, è best practice utilizzare esplicitamente l'attributo `[FromForm]`**.

4. **Utilizzo esplicito con l'attributo `[FromForm]`**

	L'attributo `[FromForm]` indica esplicitamente ad ASP.NET Core di cercare il valore del parametro nei dati del form inclusi nel body della richiesta HTTP.  Questo è particolarmente importante quando si gestiscono form complessi o quando si vuole essere sicuri della provenienza dei dati.

### Primo esempio (C\#, HTML, CSS, JavaScript) - Dati da form con codifica `application/x-www-url-encoded`

**Program.cs (C\# - ASP.NET Minimal API):**

Si consideri il seguente esempio di codice, tratto dal progetto [SimpleForms](../../../api-samples/minimal-api/BindingDemos/SimpleFormDemos/SimpleForms/):

```csharp
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();


// Form standard 
app.MapGet("/", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form Semplice</title>
</head>
<body>
<h1>Form Semplice</h1>
<form id="simpleForm" method="post" action="/submit-form">
<label for="nome">Nome:</label><br>
<input type="text" id="nome" name="nome"><br><br>
<label for="cognome">Cognome:</label><br>
<input type="text" id="cognome" name="cognome"><br><br>
<input type="submit" value="Invia">
</form>

<script>
document.getElementById('simpleForm').addEventListener('submit', function(event) {
	event.preventDefault(); // Impedisce la submit standard
	
	const formData = new FormData(this);
	
	fetch('/submit-form', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded' // Importante specificare il content type per i form standard
		},
		body: new URLSearchParams(formData).toString() // Converte FormData in URL encoded string
	})
	.then(response => response.text())
	.then(data => {
		alert(data); // Mostra la risposta del server
	});
});
</script>
</body>
</html>
""", "text/html"));


app.MapPost("/submit-form", ([FromForm] string nome, [FromForm] string cognome) =>
{
	return $"Dati ricevuti dal form: Nome = {nome}, Cognome = {cognome}";
}).DisableAntiforgery();
//------------------------------------------

// Form con dropdown
// NOta bene:se non specificata, la codifica di un form è application/x-www-form-urlencoded
app.MapGet("/dropdown", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form con Dropdown</title>
</head>
<body>
<h1>Form con Dropdown</h1>
<form id="dropdownForm" method="post" action="/submit-dropdown">
<label for="paese">Paese:</label><br>
<select id="paese" name="paese">
	<option value="italia">Italia</option>
	<option value="francia">Francia</option>
	<option value="germania">Germania</option>
	<option value="spagna">Spagna</option>
</select><br><br>
<input type="submit" value="Invia">
</form>

<script>
document.getElementById('dropdownForm').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this);
	
	fetch('/submit-dropdown', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
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
""", "text/html"));


app.MapPost("/submit-dropdown", ([FromForm] string paese) =>
{
	return $"Paese selezionato: {paese}";
}).DisableAntiforgery();

//------------------------------------------------

//Form con checkbox
app.MapGet("/checkbox", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form con Checkbox</title>
</head>
<body>
<h1>Form con Checkbox</h1>
<form id="checkboxForm" method="post" action="/submit-checkbox">
<label>Interessi:</label><br>
<input type="checkbox" id="interesse1" name="interessi" value="programmazione"> <label for="interesse1">Programmazione</label><br>
<input type="checkbox" id="interesse2" name="interessi" value="design"> <label for="interesse2">Design</label><br>
<input type="checkbox" id="interesse3" name="interessi" value="marketing"> <label for="interesse3">Marketing</label><br><br>
<input type="submit" value="Invia">
</form>

<script>
document.getElementById('checkboxForm').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this);
	const interessiSelezionati = formData.getAll('interessi');
	
	fetch('/submit-checkbox', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: interessiSelezionati.length > 0 ? new URLSearchParams(formData).toString() : 'interessi=' 
	})
	.then(response => response.text())
	.then(data => {
		alert(data);
	});
});
</script>
</body>
</html>
""", "text/html"));


app.MapPost("/submit-checkbox", ([FromForm] CheckboxFormModel model) =>
{
	var interessi = model.Interessi ?? [];
	if (interessi.Count > 0 && interessi.Any(interesse => interesse!=""))
	{
		return $"Interessi selezionati: {string.Join(", ", interessi)}";
	}
	else
	{
		return "Nessun interesse selezionato.";
	}
}).DisableAntiforgery();

//in alternativa è possibile processare la checkbox come mostrato di seguito

// app.MapPost("/submit-checkbox", async (HttpRequest request) =>
// {
// var form = await request.ReadFormAsync();
// string[]? interessi = form["interessi"];
// if (interessi!=null && interessi.Length != 0)
// {
// return $"Interessi selezionati: {string.Join(", ", interessi)}";
// }
// else
// {
// return "Nessun interesse selezionato.";
// }
// }).DisableAntiforgery();

//altra versione del post per la checkbox
//si noti che in questo caso è possibile processare la checkbox in due modi diversi:
//con:
//const interessiSelezionati = formData.getAll('interessi');
//body: new URLSearchParams({interessi: interessiSelezionati}).toString()
//oppure come mostrato di seguito
//----------------------------------------------------

//Form con checkbox - seconda versione
app.MapGet("/checkbox2", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form con Checkbox</title>
</head>
<body>
<h1>Form con Checkbox</h1>
<form id="checkboxForm" method="post" action="/submit-checkbox2">
<label>Interessi:</label><br>
<input type="checkbox" id="interesse1" name="interessi" value="programmazione"> <label for="interesse1">Programmazione</label><br>
<input type="checkbox" id="interesse2" name="interessi" value="design"> <label for="interesse2">Design</label><br>
<input type="checkbox" id="interesse3" name="interessi" value="marketing"> <label for="interesse3">Marketing</label><br><br>
<input type="submit" value="Invia">
</form>

<script>
document.getElementById('checkboxForm').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this);
	const interessiSelezionati = formData.getAll('interessi');
	
	fetch('/submit-checkbox2', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: new URLSearchParams({interessi: interessiSelezionati}).toString()
	})
	.then(response => response.text())
	.then(data => {
		alert(data);
	});
	
});
</script>
</body>
</html>
""", "text/html"));


app.MapPost("/submit-checkbox2", ([FromForm] DatiCheckboxForm datiCheckbox) =>
{
	var interessi = datiCheckbox.Interessi;
	if (interessi != null && interessi.Length > 0 && interessi.Any(interesse => interesse != ""))
	{
		return $"Interessi selezionati: {string.Join(", ", interessi)}";
	}
	else
	{
		return "Nessun interesse selezionato.";
	}
}).DisableAntiforgery();
//-------------------------------------------------------

//Form con codifica application/x-www-form-urlencoded con diversi tipi di input
app.MapGet("/complete-url-encoded-form", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form URL-encoded Completo</title>

</head>
<body>
<h1>Form URL-encoded Completo</h1>
<form id="formUrlEncoded" method="post" action="/submit-complete-url-encoded-form">
<label for="nome">Nome:</label><br>
<input type="text" id="nome" name="nome" value="Mario"><br><br>

<label for="eta">Età:</label><br>
<input type="number" id="eta" name="eta" value="30" required><br><br>

<label for"notificheAbilitate">Abilita Notifiche:</label><br>
<input type="checkbox" id="notificheAbilitate" name="notificheAbilitate">
<br><br>

<label>Genere:</label><br>
<input type="radio" id="genereMaschile" name="genere" value="maschile" checked> <label for="genereMaschile">Maschile</label><br>
<input type="radio" id="genereFemminile" name="genere" value="femminile"> <label for="genereFemminile">Femminile</label><br><br>

<label for="paese">Paese di Origine:</label><br>
<select id="paese" name="paese">
	<option value="italia">Italia</option>
	<option value="francia">Francia</option>
	<option value="germania">Germania</option>
	<option value="spagna" selected>Spagna</option>
</select><br><br>

<label>Interessi:</label><br>
<input type="checkbox" id="interesseProgrammazione" name="interessi" value="programmazione"> <label for="interesseProgrammazione">Programmazione</label><br>
<input type="checkbox" id="interesseDesign" name="interessi" value="design" checked> <label for="interesseDesign">Design</label><br>
<input type="checkbox" id="interesseMarketing" name="interessi" value="marketing" checked> <label for="interesseMarketing">Marketing</label><br><br>

<input type="submit" value="Invia Form URL-encoded">
</form>

<script>
document.getElementById('formUrlEncoded').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this);
	const interessiSelezionati = formData.getAll('interessi');
	const notificheAbilitate = document.getElementById('notificheAbilitate').checked;
	
	fetch('/submit-complete-url-encoded-form', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/x-www-form-urlencoded'
		},
		body: new URLSearchParams({
			nome: formData.get('nome'),
			eta: formData.get('eta'),
			notificheAbilitate: notificheAbilitate, // Booleano
			genere: formData.get('genere'),
			paese: formData.get('paese'),
			interessi: interessiSelezionati // Array di stringhe
		}).toString()
	})
	.then(response => response.text())
	.then(data => {
		alert(data);
	});
});
</script>
</body>
</html>
""", "text/html"));

app.MapPost("/submit-complete-url-encoded-form", ([FromForm] DatiFormUrlEncoded dati) =>
{
	if (dati == null)
	{
		return Results.BadRequest("Dati del form non validi ricevuti.");
	}

	return Results.Ok($"Dati URL-encoded ricevuti:\n" +
	   $"Nome: {dati.Nome}\n" +
	   $"Età: {dati.Eta}\n" +
	   $"Notifiche Abilitate: {dati.NotificheAbilitate}\n" +
	   $"Genere: {dati.Genere}\n" +
	   $"Paese: {dati.Paese}\n" +
	   $"Interessi: {string.Join(", ", dati.Interessi)}");
}).DisableAntiforgery();
//-----------------------------------------------------------


app.Run();


//Data Models
public class CheckboxFormModel
{
	public List<string>? Interessi { get; set; }
}

public record DatiFormJson(string Nome, string Cognome);

public record DatiFormUrlEncoded(string Nome, int Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);

public record DatiCheckboxForm(string[] Interessi);

public record DatiFormJsonCompleto(string Nome, int? Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);
public record DatiFormMultipartCompleto
{
	public string Nome { get; set; } = "";
	public int Eta { get; set; }
	public bool NotificheAbilitate { get; set; }
	public string Genere { get; set; } = "";
	public string Paese { get; set; } = "";
	public string[] Interessi { get; set; } = Array.Empty<string>();
	public IFormFile? ImmagineProfilo { get; set; }
}

```

#### Spiegazione del codice del primo endpoint - form con dati semplici

* **`app.MapGet("/")`**: Definisce un endpoint GET alla radice (`/`) che restituisce una semplice pagina HTML contenente un form.
* Il form ha due campi di testo (`nome` e `cognome`) e utilizza il metodo POST verso l'endpoint `/submitForm`.
* Il codice JavaScript intercetta la submit del form, previene il comportamento predefinito e invia la richiesta POST tramite `fetch`.  **È fondamentale specificare `'Content-Type': 'application/x-www-form-urlencoded'`** nell'header della richiesta `fetch` per simulare una submit form standard.
* **`app.MapPost("/submitForm", ([FromForm] string nome, [FromForm] string cognome) => ...)`**: Definisce un endpoint POST `/submitForm` che accetta due parametri di tipo `string`, `nome` e `cognome`, decorati con `[FromForm]`.
* `[FromForm]` indica che ASP.NET Core deve cercare i valori di `nome` e `cognome` nei dati del form della richiesta POST.
* L'endpoint restituisce una stringa che conferma la ricezione dei dati.
* Si noti che l'endpoint che riceve i dati dal form `/submit-form` utilizza il metodo di estensione `.DisableAntiForgery()` che *"Disables anti-forgery token validation for all endpoints produced on the target IEndpointConventionBuilder."*. L'AntiForgery è un meccanismo di sicurezza per la protezione contro attacchi del tipo CSRF (trattato in queste note in generale[^1] e con riferimento alle Minimal API[^2]) è abilitata di default in ASP.NET Minimal API sugli endpoint che ricevono dati da form standard (con codifica `application/x-www-url-encoded` oppure `multipart/form-data`) anche se non si abilita esplicitamente il servizio e si configura il middleware nella pipeline di ASP.NET.

* Nell'esempio precedente, l'oggetto `FormData` viene creato con l'istruzione:

```js
const formData = new FormData(this);
```

Dove `this` si riferisce al form (`<form id="simpleForm">`), quindi `FormData` conterrà tutti i dati inseriti nei campi del form.

* **Cosa contiene `formData`?**

L'oggetto `FormData` rappresenta una collezione di coppie **chiave-valore**, dove:

- **La chiave** è il valore dell'attributo `name` di ciascun campo del form.
- **Il valore** è il valore inserito dall'utente in quel campo.

Se l'utente compila il form in questo modo:

```text
Nome: Mario
Cognome: Rossi
```

All'interno di `formData` avremo:

```text
nome -> "Mario"
cognome -> "Rossi"
```

Possiamo verificarlo con questo codice:

```js
for (let pair of formData.entries()) {
console.log(pair[0] + ": " + pair[1]);
}
```

L'output nella console sarà:

```text
nome: Mario
cognome: Rossi
```

* **Caso speciale: più campi con lo stesso nome**

Se ci fossero più input con lo stesso `name`, `FormData` manterrebbe più valori sotto la stessa chiave:

```html
<input type="checkbox" name="interessi" value="sport">
<input type="checkbox" name="interessi" value="musica">
```

Se l'utente seleziona entrambi, `FormData` conterrà:

```text
interessi -> "sport"
interessi -> "musica"
```

e potremmo recuperarli con:

```js
console.log(formData.getAll("interessi")); // ["sport", "musica"]

```

**Gli altri endpoint funzionano in maniera simile.**

### Secondo esempio - invio di dati da form con codifica `multipart/form-data` e differenze rispetto alla codifica `application/x-www-form-urlencoded`

Si considerino i seguenti endpoint, sempre del progetto [SimpleForms](../../../api-samples/minimal-api/BindingDemos/SimpleFormDemos/SimpleForms/):

```csharp
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

//precedenti endpoint con codifica application/x-www-urlencoded

//endpoint che utilizzano la codifica multipart/from-data
// Form con file upload
app.MapGet("/file-upload", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form con File Upload</title>
</head>
<body>
<h1>Form con File Upload</h1>
<form id="fileForm" method="post" action="/file-upload" enctype="multipart/form-data">
<label for="fileInput">Seleziona un file:</label><br>
<input type="file" id="fileInput" name="fileInput"><br><br>
<input type="submit" value="Upload File">
</form>

<script>
document.getElementById('fileForm').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this);
	
	fetch('/file-upload', {
		method: 'POST',
		body: formData // FormData è già in formato multipart/form-data
	})
	.then(response => response.text())
	.then(data => {
		alert(data);
	});
});
</script>
</body>
</html>
""", "text/html"));


app.MapPost("/file-upload", async (IFormFile? fileInput) =>
{
	if (fileInput != null && fileInput.Length > 0)
	{
		// Salva il file (esempio semplificato: salva in memoria)
		using var stream = new MemoryStream();
		await fileInput.CopyToAsync(stream);
		byte[] fileBytes = stream.ToArray();

		// Qui si potrebbe salvare fileBytes su disco, database, ecc.
		return $"File '{fileInput.FileName}' caricato con successo. Dimensione: {fileInput.Length} bytes.";
	}
	else
	{
		return "Nessun file caricato.";
	}
}).DisableAntiforgery();
//-----------------------------------------------------

//Form con codifica multipart/form-data comprensivo di diversi tipi di dati
app.MapGet("/multipart-complete", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form Multipart con File</title>

</head>
<body>
<h1>Form Multipart con File</h1>
<form id="formMultipart" method="post" action="/submit-multipart-complete" enctype="multipart/form-data">
<label for="nome">Nome:</label><br>
<input type="text" id="nome" name="nome" value="Mario"><br><br>

<label for="eta">Età:</label><br>
<input type="number" id="eta" name="eta" value="30" required><br><br>

<label for="notificheAbilitate">Notifiche abilitate</label><br>
<input type="checkbox" id="notificheAbilitate" name="notificheAbilitate" value="true"><br><br>

<label>Genere:</label><br>
<input type="radio" id="genereMaschile" name="genere" value="maschile" checked> <label for="genereMaschile">Maschile</label><br>
<input type="radio" id="genereFemminile" name="genere" value="femminile"> <label for="genereFemminile">Femminile</label><br><br>

<label for="paese">Paese di Origine:</label><br>
<select id="paese" name="paese">
	<option value="italia">Italia</option>
	<option value="francia">Francia</option>
	<option value="germania">Germania</option>
	<option value="spagna" selected>Spagna</option>
</select><br><br>

<label>Interessi:</label><br>
<input type="checkbox" id="interesseProgrammazione" name="interessi" value="programmazione"> <label for="interesseProgrammazione">Programmazione</label><br>
<input type="checkbox" id="interesseDesign" name="interessi" value="design" checked> <label for="interesseDesign">Design</label><br>
<input type="checkbox" id="interesseMarketing" name="interessi" value="marketing" checked> <label for="interesseMarketing">Marketing</label><br><br>

<label for="immagineProfilo">Immagine Profilo:</label><br>
<input type="file" id="immagineProfilo" name="immagineProfilo" accept="image/*"><br><br>

<input type="submit" value="Invia Form Multipart">
</form>

<script>
document.getElementById('formMultipart').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this); // FormData gestisce automaticamente multipart/form-data
	
	fetch('/submit-multipart-complete', {
		method: 'POST',
		body: formData // Invia FormData direttamente per multipart/form-data
	})
	.then(response => response.text())
	.then(data => {
		alert(data);
	})
	.catch(error => {
		console.error('Errore nella richiesta:', error);
		alert('Errore nell\'invio dei dati Multipart.');
	});
});
</script>
</body>
</html>
""", "text/html"));


app.MapPost("/submit-multipart-complete", async ([FromForm] DatiFormMultipartCompleto dati) =>
{
	string fileInfo = "Nessun file caricato.";
	if (dati.ImmagineProfilo != null && dati.ImmagineProfilo.Length > 0)
	{
		fileInfo = $"File '{dati.ImmagineProfilo.FileName}', tipo: '{dati.ImmagineProfilo.ContentType}', dimensione: {dati.ImmagineProfilo.Length} bytes.";
		// Qui si potrebbe salvare il file, ad esempio su disco o in un servizio di storage cloud.
		// Esempio semplificato: salvataggio in memoria per dimostrazione:
		using var stream = new MemoryStream();
		await dati.ImmagineProfilo.CopyToAsync(stream);
		// byte[] fileBytes = stream.ToArray();
		// ... qui si potrebbe fare qualcosa con fileBytes ...
	}

	string interessiString = dati.Interessi!=null? string.Join(", ", dati.Interessi):"";
	return Results.Ok($"Dati Multipart ricevuti:\n" +
	   $"Nome: {dati.Nome}\n" +
	   $"Età: {dati.Eta}\n" +
	   $"Notifiche Abilitate: {dati.NotificheAbilitate}\n" +
	   $"Genere: {dati.Genere}\n" +
	   $"Paese: {dati.Paese}\n" +
	   $"Interessi: {interessiString}\n" +
	   $"Immagine Profilo: {fileInfo}");
}).DisableAntiforgery();

app.Run();


//Data Models
public class CheckboxFormModel
{
	public List<string>? Interessi { get; set; }
}

public record DatiFormJson(string Nome, string Cognome);

public record DatiFormUrlEncoded(string Nome, int Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);

public record DatiCheckboxForm(string[] Interessi);

public record DatiFormJsonCompleto(string Nome, int? Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);
public record DatiFormMultipartCompleto
{
	public string Nome { get; set; } = "";
	public int Eta { get; set; }
	public bool NotificheAbilitate { get; set; }
	public string Genere { get; set; } = "";
	public string Paese { get; set; } = "";
	public string[] Interessi { get; set; } = Array.Empty<string>();
	public IFormFile? ImmagineProfilo { get; set; }
}

```

**In questo secondo esempio i form vengono sottomessi in maniera differente perché la codifica utilizzata nel form è differente.**

* **Perché nel primo esempio serve `URLSearchParams`, mentre nel secondo no?**

	1. **Primo caso** (dove viene usato `URLSearchParams`):

		- Il form viene inviato con **`application/x-www-form-urlencoded`**.
		- `FormData` viene convertito in una stringa codificata nel formato `key=value&key2=value2`, che è il formato standard per i form HTML senza file allegati.
		- Si utilizza:

			```js
			`body: new URLSearchParams(formData).toString()`
			```

		perché `fetch` non può inviare direttamente un `FormData` come `application/x-www-form-urlencoded`.

	2. **Secondo caso** (dove si usa solo `FormData` senza `URLSearchParams`):

		- Il form è definito con `enctype="multipart/form-data"`, il che significa che può gestire **file e dati binari**.
		- `FormData` viene passato direttamente nel `body` di `fetch`:

			```js
			`body: formData`
			```

		- In questo caso, **non bisogna usare `URLSearchParams`**, perché questo converte i dati in una stringa e perderebbe il supporto per gli allegati (`<input type="file">`).
		- `fetch` con un oggetto `FormData` automaticamente imposta l'**intestazione (`Content-Type: multipart/form-data; boundary=...`)**, che è necessaria per inviare file.

	3. **Riepilogo delle differenze chiave**

	| Caratteristica | Primo esempio (`URLSearchParams`) | Secondo esempio (`FormData` diretto) |
	| --- |  --- |  --- |
	| **Formato della richiesta** | `application/x-www-form-urlencoded` | `multipart/form-data` |
	| **Metodo di conversione** | `new URLSearchParams(formData).toString()` | `body: formData` diretto |
	| **Supporto per file** | ❌ (non supporta file) | ✅ (supporta file e dati binari) |
	| **Quando usarlo?** | Quando si inviano solo **testo/campi input** | Quando si devono inviare anche **file** |

	Quindi, negli endpoint del secondo esempio **non è necessario usare `URLSearchParams`**, perché `FormData` supporta direttamente la codifica multipart e mantiene il formato corretto per gli allegati.

### Terzo esempio - invio di dati da form con codifica `application/json` e differenze rispetto alle codifiche `application/x-www-form-urlencoded` e `multipart/form-data`

Si considerino i seguenti endpoint, sempre del progetto [SimpleForms](../../../api-samples/minimal-api/BindingDemos/SimpleFormDemos/SimpleForms/):

```csharp
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

//altri endpoint con le precedenti codifiche già analizzate

//Form con codifica application/json
app.MapGet("/json-data", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form JSON</title>
</head>
<body>
<h1>Form JSON</h1>
<form id="jsonForm" method="post" action="/submit-json-data">
<label for="nome">Nome:</label><br>
<input type="text" id="nome" name="nome"><br><br>
<label for="cognome">Cognome:</label><br>
<input type="text" id="cognome" name="cognome"><br><br>
<input type="submit" value="Invia come JSON">
</form>

<script>
document.getElementById('jsonForm').addEventListener('submit', function(event) {
	event.preventDefault(); // Impedisce la submit standard del form
	
	const nome = document.getElementById('nome').value;
	const cognome = document.getElementById('cognome').value;
	
	// Crea un oggetto JSON con i dati del form
	const jsonData = {
		nome: nome,
		cognome: cognome
	};
	
	fetch('/submit-json-data', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json' // Specifica che inviamo JSON
		},
		body: JSON.stringify(jsonData) // Converte l'oggetto JavaScript in stringa JSON
	})
	.then(response => response.json()) // Aspetta una risposta JSON dal server
	.then(data => {
		alert(data.message); // Mostra il messaggio di successo dal server
	})
	.catch(error => {
		console.error('Errore nella richiesta:', error);
		alert('Errore nell\'invio dei dati JSON.');
	});
});
</script>
</body>
</html>
""", "text/html"));

app.MapPost("/submit-json-data", ([FromBody] DatiFormJson dati) =>
{
	if (dati == null)
	{
		return Results.BadRequest(new { message = "Dati JSON non validi ricevuti." });
	}

	return Results.Ok(new { message = $"Dati JSON ricevuti: Nome = {dati.Nome}, Cognome = {dati.Cognome}" });
});
//----------------------------------------------------------

//Form con codifica application/json  con diversi tipi di input
app.MapGet("/json-complete", () => Results.Content(
"""
<!DOCTYPE html>
<html lang="it">

<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Form JSON Completo</title>

</head>
<body>
<h1>Form JSON Completo</h1>
<form id="formJson" method="post" action="/submit-json-complete">
<label for="nome">Nome:</label><br>
<input type="text" id="nome" name="nome" value="Mario"><br><br>

<label for="eta">Età:</label><br>
<input type="number" id="eta" name="eta" value="30" ><br><br>

<label>Abilita Notifiche:</label><br>
<label class="toggle-switch">
	<input type="checkbox" id="notificheAbilitate" name="notificheAbilitate">
	<span class="slider"></span>
</label><br><br>

<label>Genere:</label><br>
<input type="radio" id="genereMaschile" name="genere" value="maschile" checked> <label for="genereMaschile">Maschile</label><br>
<input type="radio" id="genereFemminile" name="genere" value="femminile"> <label for="genereFemminile">Femminile</label><br><br>

<label for="paese">Paese di Origine:</label><br>
<select id="paese" name="paese">
	<option value="italia">Italia</option>
	<option value="francia">Francia</option>
	<option value="germania">Germania</option>
	<option value="spagna" selected>Spagna</option>
</select><br><br>

<label>Interessi:</label><br>
<input type="checkbox" id="interesseProgrammazione" name="interessi" value="programmazione"> <label for="interesseProgrammazione">Programmazione</label><br>
<input type="checkbox" id="interesseDesign" name="interessi" value="design" checked> <label for="interesseDesign">Design</label><br>
<input type="checkbox" id="interesseMarketing" name="interessi" value="marketing" checked> <label for="interesseMarketing">Marketing</label><br><br>

<input type="submit" value="Invia Form JSON">
</form>

<script>
document.getElementById('formJson').addEventListener('submit', function(event) {
	event.preventDefault();
	
	const formData = new FormData(this);
	const interessiSelezionati = formData.getAll('interessi');
	const notificheAbilitate = document.getElementById('notificheAbilitate').checked;
	
	
	const jsonData = {
		nome: formData.get('nome'),
		eta: parseInt(formData.get('eta')), // Conversione a numero
		notificheAbilitate: notificheAbilitate, // Booleano
		genere: formData.get('genere'),
		paese: formData.get('paese'),
		interessi: interessiSelezionati // Array di stringhe
	};
	
	
	fetch('/submit-json-complete', {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(jsonData) // Invia come stringa JSON
	})
	.then(response => response.json())
	.then(data => {
		alert(data.message);
	})
	.catch(error => {
		console.error('Errore nella richiesta:', error);
		alert('Errore nell\'invio dei dati JSON.');
	});
});
</script>
</body>
</html>
""", "text/html"));

//in questo caso si potrebbe inserire anche l'annotation [FromBody] davanti a DatiFromJsonCompleto
//ma in questo caso è possibile usare il binding implicito
app.MapPost("/submit-json-complete", (DatiFormJsonCompleto dati) =>
{
	if (dati == null)
	{
		return Results.BadRequest(new { message = "Dati JSON non validi ricevuti." });
	}

	return Results.Ok(new
	{
		message = $"Dati JSON ricevuti:\n" +
		   $"Nome: {dati.Nome}\n" +
		   $"Età: {dati.Eta}\n" +
		   $"Notifiche Abilitate: {dati.NotificheAbilitate}\n" +
		   $"Genere: {dati.Genere}\n" +
		   $"Paese: {dati.Paese}\n" +
		   $"Interessi: {string.Join(", ", dati.Interessi)}"
	});
}); //in questo caso non è abilitata di default la protezione CSRF e non c'è bisogno di disabilitarla
	//----------------------------------------------------------

app.Run();


//Data Models
public class CheckboxFormModel
{
	public List<string>? Interessi { get; set; }
}

public record DatiFormJson(string Nome, string Cognome);

public record DatiFormUrlEncoded(string Nome, int Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);

public record DatiCheckboxForm(string[] Interessi);

public record DatiFormJsonCompleto(string Nome, int? Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);
public record DatiFormMultipartCompleto
{
	public string Nome { get; set; } = "";
	public int Eta { get; set; }
	public bool NotificheAbilitate { get; set; }
	public string Genere { get; set; } = "";
	public string Paese { get; set; } = "";
	public string[] Interessi { get; set; } = Array.Empty<string>();
	public IFormFile? ImmagineProfilo { get; set; }
}

```

### Confronto tra le tre soluzioni di invio dati al server

In questa sezione si sono esaminati tre modi per inviare dati ad un server. Le differenze chiave sono riportate nella tabella seguente:

| **Caratteristica**        | **Prima caso** (`URLSearchParams`) | **Secondo caso** (`FormData` diretto) | **Terzo caso** (`JSON.stringify()`) |
|---------------------------|--------------------------------------|------------------------------------------|--------------------------------------|
| **Formato della richiesta** | `application/x-www-form-urlencoded` | `multipart/form-data` | `application/json` |
| **Metodo di conversione** | `new URLSearchParams(formData).toString()` | `body: formData` diretto | `JSON.stringify({...})` |
| **Supporto per file** | ❌ No | ✅ Sì | ❌ No |
| **Quando usarlo?** | Quando si inviano solo **testo/campi input** senza file | Quando si devono inviare **file + dati di testo** | Quando si lavora con **API JSON-based** (es. REST API) |
| **Struttura dei dati** | Stringa `key=value&key2=value2` | Dati con **boundary** (supporta file) | Oggetto JSON `{ "chiave": "valore" }` |
| **Intestazione (`Content-Type`)** | `application/x-www-form-urlencoded` | `multipart/form-data` (gestito da `FormData`) | `application/json` |

#### Perché nel terzo caso non si usa né `URLSearchParams` né `FormData`?

Nel **terzo caso**, i dati vengono inviati a un endpoint che si aspetta un **JSON nel corpo della richiesta**.  
Qui, `fetch` invia i dati con:

```js
body: JSON.stringify({ chiave1: valore1, chiave2: valore2})
```

e imposta l'intestazione corretta:

```js
headers: { 'Content-Type': 'application/json' }
```

### Motivazioni principali per l'uso di JSON

1. **Le API REST moderne accettano e restituiscono JSON**, quindi inviare i dati in questo formato è più naturale.
2. **Maggiore flessibilità**: JSON supporta **oggetti annidati e array**, mentre `application/x-www-form-urlencoded` è limitato a stringhe chiave-valore.
3. **Più leggibile e strutturato**: Un JSON come:

   ```json
   {
	   "nome": "Christopher",
	   "cognome": "Nolan",
	   "nazionalità": "UK",
	   "tmdbId": 525
   }
   ```

   è più chiaro rispetto a `nome=Christopher&cognome=Nolan&nazionalità=UK&tmdbId=525`.

### Quando usare ciascun metodo?

- **`URLSearchParams`** → Quando il backend si aspetta dati come stringa URL (`application/x-www-form-urlencoded`). Tipico dei form HTML tradizionali.
- **`FormData`** → Quando bis inviare file o dati binari. Utile per upload di immagini o allegati.
- **`JSON.stringify()`** → Quando l'API accetta JSON. Ideale per comunicare con API REST moderne.

### `[FromForm]` vs `[AsParameters]`: Differenze e quando usarli

Sia `[FromForm]` che `[AsParameters]` sono utilizzati per il binding di dati provenienti dal form, ma hanno utilizzi e comportamenti differenti.

* **`[FromForm]`**:  Si applica `[FromForm]` **singolarmente ad ogni parametro** dell'endpoint che si vuole mettere in binding con i dati del form.  È il modo **più esplicito e controllato** per definire da dove provengono i dati.  È **raccomandato** per la maggior parte dei casi, specialmente quando si gestiscono form complessi o si vuole chiarezza sulla provenienza dei dati.

* **`[AsParameters]`**:  Si applica `[AsParameters]` **ad un tipo complesso** (classe o record) come parametro dell'endpoint. ASP.NET Core tenterà di mettere in binding le proprietà di questo tipo complesso dai dati del form.  `[AsParameters]` è utile per raggruppare logicamente parametri correlati provenienti dal form in un singolo oggetto, rendendo la firma dell'endpoint più pulita, **ma può rendere meno esplicito da dove provengono i dati**.

#### Esempio comparativo (C\#) - `[FromForm]` vs `[AsParameters]`

**Scenario:**  Un form con campi `Nome`, `Cognome`, `Email`.

1. **Utilizzo di `[FromForm]` (Esplicito e raccomandato):**

	```csharp
	app.MapPost("/submitDatiPersonaliFromForm",
		([FromForm] string nome, [FromForm] string cognome, [FromForm] string email) =>
	{
		return $"Dati (FromForm): Nome={nome}, Cognome={cognome}, Email={email}";
	});
	```

2. **Utilizzo di `[AsParameters]` (Raggruppamento, meno esplicito):**

	```csharp
	public record DatiPersonali(string Nome, string Cognome, string Email); // Record per raggruppare i dati

	app.MapPost("/submitDatiPersonaliAsParameters",
		([AsParameters] DatiPersonali dati) => // [AsParameters] sul record DatiPersonali
	{
		return $"Dati (AsParameters): Nome={dati.Nome}, Cognome={dati.Cognome}, Email={dati.Email}";
	});
	```

**HTML per entrambi gli esempi (identico):**

```html
<form method="post" action="/submitDatiPersonaliFromForm" id="formFromForm">
	<label for="nomeFromForm">Nome:</label><input type="text" id="nomeFromForm" name="nome"><br>
	<label for="cognomeFromForm">Cognome:</label><input type="text" id="cognomeFromForm" name="cognome"><br>
	<label for="emailFromForm">Email:</label><input type="email" id="emailFromForm" name="email"><br>
	<input type="submit" value="Invia (FromForm)">
</form>

<form method="post" action="/submitDatiPersonaliAsParameters" id="formAsParameters">
	<label for="nomeAsParameters">Nome:</label><input type="text" id="nomeAsParameters" name="nome"><br>
	<label for="cognomeAsParameters">Cognome:</label><input type="text" id="cognomeAsParameters" name="cognome"><br>
	<label for="emailAsParameters">Email:</label><input type="email" id="emailAsParameters" name="email"><br>
	<input type="submit" value="Invia (AsParameters)">
</form>
```

**Quando usare cosa:**

  * **Si usa `[FromForm]`**:
	  * Nella maggior parte dei casi, per **chiarezza** e **controllo esplicito**.
	  * Quando si vuole essere molto precisi sulla provenienza dei dati.
  * **Si usa `[AsParameters]`**:
	  * Quando si hanno **molti parametri** provenienti dal form e/o da parti diverse della richiesta e si vuole **semplificare la firma dell'endpoint**.
	  * Per raggruppare logicamente parametri in un oggetto.
	  * Quando la provenienza dei dati è ovvia dal contesto (es. un endpoint chiaramente dedicato all'elaborazione di un form specifico).

#### Best practice per la gestione dei form in Minimal APIs

  * **Utilizzare `[FromForm]` esplicitamente**: Per chiarezza e controllo, usare sempre `[FromForm]` quando si vuole collegare dati da un form, specialmente per tipi semplici.
  * **Validare i dati in ingresso**:  **Sempre** validare i dati ricevuti dal form lato server per sicurezza e integrità dei dati. Si può usare Data Annotations, FluentValidation o validazione manuale.
  * **Gestire gli errori di validazione**:  Restituire feedback appropriati all'utente in caso di errori di validazione (es. `Results.ValidationProblem()`).
  * **Proteggere i form con Anti-Forgery Token**: Implementare la protezione Anti-Forgery (si veda la sezione specifica) per prevenire attacchi CSRF, soprattutto per form che modificano dati.
  * **Utilizzare HTTPS**: Assicurare che la applicazione utilizzi HTTPS per proteggere la trasmissione dei dati del form.
  * **Considerare l'uso di View Model**: Per form complessi, si potrebbe voler creare un View Model (classi dedicate) per rappresentare i dati del form e facilitare la validazione e il binding.

**Link Utili:**

  * [Documentazione ufficiale Microsoft:  Binding dei parametri del form in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) (Sebbene focalizzata su MVC, i concetti di binding dei form sono simili anche in Minimal APIs)
  * [Documentazione ufficiale Microsoft:  Attributo FromForm](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromformattribute)

## Binding esplicito in ASP.NET Minimal API

Oltre al binding automatico e al binding dai form, ASP.NET Core Minimal APIs offrono diversi attributi e meccanismi per specificare esplicitamente la sorgente dei dati per i parametri degli endpoint. Questo binding esplicito fornisce un controllo preciso e rende il codice più chiaro e manutenibile.

### Binding da Header con `[FromHeader]`

L'attributo `[FromHeader]` indica che il valore del parametro deve essere collegato dall'**header della richiesta HTTP**.  Questo è utile quando si vogliono estrarre informazioni specifiche dagli header, come token di autorizzazione personalizzati, versioni API, user agent, etc.

**Caso d'uso:** Recuperare un token di autorizzazione personalizzato inviato in un header chiamato `X-Authorization-Token`.

**Esempio di codice (C\#) e richiesta di esempio:**

**Program.cs (C\# - ASP.NET Minimal API):**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/headerExample", ([FromHeader(Name = "X-Authorization-Token")] string authorizationToken) =>
{
	if (string.IsNullOrEmpty(authorizationToken))
	{
		return Results.BadRequest("Header 'X-Authorization-Token' mancante.");
	}
	return Results.Ok($"Valore dell'header X-Authorization-Token: {authorizationToken}");
});

app.Run();
```

**Richiesta di esempio (curl o HttpClient):**

```bash
curl -H "X-Authorization-Token: my-secret-token-123" https://localhost:<port>/headerExample
```

**Spiegazione:**

  * **`[FromHeader(Name = "X-Authorization-Token")] string authorizationToken`**:
	  * L'attributo `[FromHeader]` indica che `authorizationToken` deve essere prelevato da un header.
	  * `Name = "X-Authorization-Token"` specifica che l'header da cui prelevare il valore è `X-Authorization-Token`. Se l'attributo `Name` non viene specificato, ASP.NET Core cercherà un header con lo stesso nome del parametro (in questo caso, `AuthorizationToken`, non case-sensitive).  È best practice usare `Name` per chiarezza e per gestire differenze tra nome del parametro e nome dell'header.
  * L'endpoint `/headerExample` controlla se `authorizationToken` è vuoto e restituisce un errore BadRequest se l'header è mancante. Altrimenti, restituisce un messaggio di successo con il valore dell'header.

**Link Utili:**

  * [Documentazione ufficiale Microsoft:  Attributo FromHeader](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromheaderattribute)

-----

### Binding dalla Query String con `[FromQuery]`

L'attributo `[FromQuery]` indica che il valore del parametro deve essere prelevato dalla **query string dell'URL della richiesta HTTP**. La query string è la parte dell'URL che segue il punto interrogativo (`?`), formata da coppie chiave-valore (es. `?param1=value1&param2=value2`).  `[FromQuery]` è comunemente usato per parametri opzionali, filtri di ricerca, paginazione, ordinamento, etc.

**Caso d'uso:** Gestire parametri di ricerca e paginazione in una lista di prodotti.

**Esempio di codice (C\#) e URL di esempio:**

**Program.cs (C\# - ASP.NET Minimal API):**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/products", ([FromQuery] string? searchTerm, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1) =>
{
	string result = "Prodotti:";
	if (!string.IsNullOrEmpty(searchTerm))
	{
		result += $" Ricerca per: '{searchTerm}'";
	}
	result += $", Pagina: {pageNumber}, Dimensione pagina: {pageSize}";
	return Results.Ok(result);
});

app.Run();
```

**URL di esempio:**

```text
https://localhost:<port>/products?searchTerm=laptop&pageSize=20&pageNumber=2
```

**Spiegazione:**

  * **`[FromQuery] string? searchTerm`**:
	  * `[FromQuery]` indica che `searchTerm` deve essere prelevato dalla query string.
	  * `string?`  rende il parametro `searchTerm` **nullable**, indicando che è **opzionale** nella query string. Se `searchTerm` non è presente nella query string, il valore di `searchTerm` sarà `null`.
  * **`[FromQuery] int pageSize = 10`**:
	  * `[FromQuery]` per `pageSize`.
	  * `int pageSize = 10`:  Definisce un **valore predefinito** di `10` per `pageSize`. Se `pageSize` non è fornito nella query string, il valore di `pageSize` sarà `10`.
  * **`[FromQuery] int pageNumber = 1`**: Simile a `pageSize`, con valore predefinito `1`.
  * L'endpoint `/products` compone una stringa di risultato basata sui parametri della query string e la restituisce come risposta Ok.

**Link Utili:**

  * [Documentazione ufficiale Microsoft: Attributo FromQuery](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromqueryattribute)

-----

### Binding dalla Route con `[FromRoute]`

L'attributo `[FromRoute]` indica che il valore del parametro deve essere prelevato dai **segmenti della route URL** definiti nel pattern dell'endpoint.  Le route parameters sono parti dinamiche dell'URL, racchiuse tra parentesi graffe `{}` nel pattern di route (es. `/items/{id}`). `[FromRoute]` è tipicamente usato per identificare risorse specifiche (es. l'ID di un prodotto, l'username di un utente, etc.).

**Caso d'uso:** Recuperare l'ID di un prodotto da un URL come `/products/{id}`.

**Esempio di codice (C\#) e URL di esempio:**

**Program.cs (C\# - ASP.NET Minimal API):**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/products/{id}", ([FromRoute] int id) =>
{
	return Results.Ok($"Dettagli prodotto con ID: {id}");
});

app.Run();
```

**URL di esempio:**

```text
https://localhost:<port>/products/123
```

**Spiegazione:**

  * **`app.MapGet("/products/{id}", ...)`**: Definisce una route con il pattern `/products/{id}`. `{id}` è un **route parameter**.
  * **`[FromRoute] int id`**:
	  * `[FromRoute]` indica che `id` deve essere prelevato dal route parameter.
	  * ASP.NET Core cercherà un segmento nella route URL che corrisponda al nome `id` (in questo caso, il segmento `{id}` nel pattern `/products/{id}`).
  * L'endpoint `/products/{id}` restituisce un messaggio con l'ID del prodotto estratto dalla route.

**Link Utili:**

  * [Documentazione ufficiale Microsoft: Attributo FromRoute](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromrouteattribute)

-----

### Binding dai Servizi Iniettati con `[FromServices]`

L'attributo `[FromServices]` permette di iniettare un **servizio registrato nel container di Dependency Injection (DI)** direttamente come parametro di un endpoint Minimal API.  Questo è utile per accedere a servizi come logger, servizi di database, configurazioni, etc., all'interno degli endpoint.

**Caso d'uso:**  Utilizzare un servizio logger personalizzato per registrare eventi in un endpoint.

**Esempio di codice (C\#):**

**Program.cs (C\# - ASP.NET Minimal API):**

```csharp
// Servizio Logger Personalizzato (esempio semplice)
public interface IMyLogger
{
	void LogInformation(string message);
}

public class MyLogger : IMyLogger
{
	public void LogInformation(string message)
	{
		Console.WriteLine($"[MY LOGGER] Information: {message}");
	}
}


var builder = WebApplication.CreateBuilder(args);

// Registra il servizio personalizzato nel container DI
builder.Services.AddSingleton<IMyLogger, MyLogger>();

var app = builder.Build();

app.MapGet("/logExample", ([FromServices] IMyLogger logger) =>
{
	logger.LogInformation("Endpoint /logExample invocato.");
	return Results.Ok("Log eseguito con servizio personalizzato.");
});

app.Run();
```

**Spiegazione:**

  * **`public interface IMyLogger` e `public class MyLogger : IMyLogger`**:  Definiscono un'interfaccia e un'implementazione di un servizio logger personalizzato.
  * **`builder.Services.AddSingleton<IMyLogger, MyLogger>();`**: Registra `MyLogger` come implementazione di `IMyLogger` nel container DI con scope Singleton (una sola istanza per l'intera applicazione).
  * **`[FromServices] IMyLogger logger`**:
	  * `[FromServices]` indica che `logger` deve essere risolto dal container DI.
	  * ASP.NET Core cercherà un servizio registrato per il tipo `IMyLogger` nel container DI e lo inietterà come parametro `logger`.
  * L'endpoint `/logExample` riceve l'istanza di `IMyLogger` iniettata e la utilizza per registrare un messaggio.

**Link Utili:**

  * [Documentazione ufficiale Microsoft: Attributo FromServices](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromservicesattribute)
  * [Documentazione ufficiale Microsoft: Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) (Concetti di Dependency Injection)

-----

### Accesso Diretto a `HttpRequest`

In alcuni scenari avanzati, potresti aver bisogno di accedere direttamente all'oggetto `HttpRequest` completo per ispezionare dettagli della richiesta HTTP che non sono facilmente accessibili tramite i binding standard (es. header non standard, informazioni sulla connessione, etc.).  Puoi ottenere l'oggetto `HttpRequest` accedendo alla proprietà `HttpContext.Request` all'interno del contesto dell'endpoint.

**Caso d'uso:** Leggere un header personalizzato non gestito dagli attributi `FromHeader`.

**Esempio di codice (C\#):**

**Program.cs (C\# - ASP.NET Minimal API):**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/rawRequest", (HttpContext context) =>
{
	string? customHeaderValue = context.Request.Headers["X-Custom-Header"]; // Accesso diretto all'header

	if (string.IsNullOrEmpty(customHeaderValue))
	{
		return Results.BadRequest("Header 'X-Custom-Header' mancante.");
	}
	return Results.Ok($"Valore dell'header X-Custom-Header: {customHeaderValue}");
});

app.Run();
```

**Spiegazione:**

  * **`(HttpContext context) => ...`**:  ASP.NET Core inietta automaticamente l'oggetto `HttpContext` come parametro dell'endpoint.  `HttpContext` fornisce accesso a tutto il contesto della richiesta HTTP corrente.
  * **`context.Request.Headers["X-Custom-Header"]`**: Accede alla collezione `Headers` dell'oggetto `HttpRequest` e recupera il valore dell'header `X-Custom-Header` tramite il suo nome (case-insensitive).
  * L'endpoint `/rawRequest` verifica se l'header personalizzato è presente e restituisce la risposta appropriata.

**Considerazioni sull'accesso diretto a `HttpRequest`:**

  * L'accesso diretto a `HttpRequest` offre **massima flessibilità**, ma **riduce l'astrazione** e può rendere il codice meno dichiarativo e più soggetto a errori se non usato con attenzione.
  * **Utilizzare il binding esplicito (come `[FromHeader]`, `[FromQuery]`, `[FromRoute]`) quando possibile.** L'accesso diretto a `HttpRequest` dovrebbe essere riservato a casi d'uso specifici dove gli attributi di binding non sono sufficienti.

### Accesso Diretto ai Servizi Iniettati

Similmente all'accesso diretto a `HttpRequest`, si può anche accedere direttamente al **container di Dependency Injection (IServiceProvider)** tramite la proprietà `HttpContext.RequestServices` dell'oggetto `HttpContext`.  Questo permette di risolvere e utilizzare **qualsiasi servizio registrato nel container DI**, anche se non lo si vuole iniettare come parametro dell'endpoint tramite `[FromServices]`.

**Caso d'uso:**  Risolvere un servizio solo in determinate condizioni all'interno dell'endpoint, senza dichiararlo sempre come parametro.

**Esempio di codice (C\#):**

```csharp
public interface IAnotherService //Altro Servizio di esempio
{
	string GetData();
}

public class AnotherService : IAnotherService
{
	public string GetData() => "Data from AnotherService";
}


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IAnotherService, AnotherService>(); // Registra AnotherService

var app = builder.Build();


app.MapGet("/serviceAccess", (HttpContext context) =>
{
	var anotherService = context.RequestServices.GetService<IAnotherService>(); // Risolve IAnotherService dal container DI

	if (anotherService != null)
	{
		return Results.Ok($"Servizio risolto: {anotherService.GetData()}");
	}
	else
	{
		return Results.Problem("Servizio IAnotherService non trovato nel container DI.");
	}
});

app.Run();
```

**Spiegazione:**

  * **`public interface IAnotherService` e `public class AnotherService : IAnotherService`**: Definiscono un altro servizio di esempio.
  * **`builder.Services.AddSingleton<IAnotherService, AnotherService>();`**: Registra `AnotherService` nel container DI.
  * **`(HttpContext context) => ...`**: Inietta `HttpContext`.
  * **`context.RequestServices.GetService<IAnotherService>()`**: Utilizza `HttpContext.RequestServices` per accedere al container DI (**IServiceProvider**) associato alla richiesta corrente e risolve il servizio `IAnotherService` tramite `GetService<T>()`.
  * L'endpoint `/serviceAccess` tenta di risolvere `IAnotherService` e restituisce un messaggio appropriato a seconda se il servizio viene trovato o meno.

**Considerazioni sull'accesso diretto a `IServiceProvider`:**

  * L'accesso diretto a `IServiceProvider` offre **massima flessibilità** per la gestione dei servizi, ma **dovrebbe essere usato con parsimonia**.  **È preferibile utilizzare l'injection di servizi tramite `[FromServices]` come parametri dell'endpoint quando possibile**, in quanto rende la dipendenza dai servizi più esplicita e chiara nella firma dell'endpoint.
  * L'accesso diretto a `IServiceProvider` può essere utile in situazioni dove la risoluzione di un servizio è **condizionale** o necessaria solo in una parte specifica della logica dell'endpoint.

-----

**In conclusione:**

Il binding esplicito in ASP.NET Minimal APIs, tramite attributi come `[FromHeader]`, `[FromQuery]`, `[FromRoute]`, `[FromServices]` e l'accesso diretto a `HttpRequest` e `IServiceProvider`, fornisce un controllo granulare sulla provenienza dei dati dei parametri degli endpoint e sull'accesso ai servizi.  Utilizzare il binding esplicito (attributi) quando possibile per chiarezza e manutenibilità, riservando l'accesso diretto a `HttpRequest` e `IServiceProvider` a scenari più avanzati o specifici.

## Meccanismi avanzati di Binding (cenni)

Oltre al binding automatico e al binding da form, ASP.NET Core offre meccanismi più avanzati per scenari specifici.

**Custom Binding con `TryParse` e `BindAsync`**

Per tipi di parametri personalizzati, puoi estendere il meccanismo di binding implementando:

  * **`static bool TryParse(string value, IFormatProvider provider, out T result)`**:  Per binding **sincrono** da stringa.  ASP.NET Core cercherà un metodo `TryParse` statico nel tuo tipo personalizzato.
  * **`static ValueTask<T?> BindAsync(HttpContext context, ParameterInfo parameter)`**:  Per binding **asincrono** e più complesso, che può accedere al contesto HTTP completo (header, body, ecc.).

**Link Utili:**

  * [Documentazione ufficiale Microsoft: Custom parameter binding in Minimal APIs - TryParse](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?#tryparse)
  * [Documentazione ufficiale Microsoft: Custom parameter binding in Minimal APIs - BindAsync](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?#bindasync)

**Request Body as a `Stream` or `PipeReader`**

In scenari avanzati (es. gestione di upload di file molto grandi o streaming di dati), puoi accedere direttamente al body della richiesta come `Stream` o `PipeReader` per un controllo a basso livello e maggiore efficienza.

**Link Utili:**

  * [Documentazione ufficiale Microsoft: Access request body as a Stream or PipeReader](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?#bind-the-request-body-as-a-stream-or-pipereader)

## Ordine nell'applicazione delle regole di Binding

Regole per determinare un'origine di associazione (Binding) da un parametro:

1. Attributo esplicito definito nel parametro (attributi From\*) nell'ordine seguente:
	1. Valori del percorso: [`[FromRoute]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromrouteattribute)
	2. Stringa di query: [`[FromQuery]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromqueryattribute)
	3. Intestazione: [`[FromHeader]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromheaderattribute)
	4. Corpo: [`[FromBody]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.frombodyattribute)
	5. Modulo: [`[FromForm]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromformattribute)
	6. Servizio: [`[FromServices]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromservicesattribute)
	7. Valori dei parametri: [`[AsParameters]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.asparametersattribute)
2. Tipi speciali
	1. [`HttpContext`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext)
	2. [`HttpRequest`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest) ([`HttpContext.Request`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.request#microsoft-aspnetcore-http-httpcontext-request))
	3. [`HttpResponse`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpresponse) ([`HttpContext.Response`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.response#microsoft-aspnetcore-http-httpcontext-response))
	4. [`ClaimsPrincipal`](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal) ([`HttpContext.User`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.user#microsoft-aspnetcore-http-httpcontext-user))
	5. [`CancellationToken`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) ([`HttpContext.RequestAborted`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestaborted#microsoft-aspnetcore-http-httpcontext-requestaborted))
	6. [`IFormCollection`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformcollection) ([`HttpContext.Request.Form`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformcollection))
	7. [`IFormFileCollection`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformfilecollection) ([`HttpContext.Request.Form.Files`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformcollection.files#microsoft-aspnetcore-http-iformcollection-files))
	8. [`IFormFile`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformfile) ([`HttpContext.Request.Form.Files[paramName]`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformfilecollection.item#microsoft-aspnetcore-http-iformfilecollection-item(system-string)))
	9. [`Flusso`](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream) ([`HttpContext.Request.Body`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest.body#microsoft-aspnetcore-http-httprequest-body))
	10. [`PipeReader`](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipelines.pipereader) ([`HttpContext.Request.BodyReader`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest.bodyreader#microsoft-aspnetcore-http-httprequest-bodyreader))
3. Il tipo di parametro ha un metodo [`BindAsync`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.ibindablefromhttpcontext-1.bindasync) statico valido.
4. Il tipo di parametro è una stringa o dispone di un metodo [`TryParse`](https://learn.microsoft.com/en-us/dotnet/api/system.iparsable-1.tryparse) statico valido.
	1. Se il nome del parametro è presente nel modello di route, ad esempio , è associato dalla route.`app.Map("/todo/{id}", (int id) => {});`
	2. Associato dalla stringa di query.
5. Se il tipo di parametro è un servizio fornito dall'inserimento delle dipendenze, utilizza tale servizio come origine.
6. Il parametro proviene dal corpo.

## Conclusioni e best practice generali sul Parameter Binding

**Riepilogo delle best practice per un binding efficace e sicuro:**

  * **Essere espliciti**: Utilizzare attributi come `[FromForm]`, `[FromQuery]`, `[FromBody]`, `[FromRoute]` per dichiarare chiaramente la provenienza dei dati dei parametri.
  * **Validare sempre i dati**: Effettuare la validazione dei dati in ingresso lato server, indipendentemente dalla validazione lato client.
  * **Proteggere i form**: Implementare sempre la protezione Anti-Forgery per i form che modificano dati.
  * **Utilizzare HTTPS**:  Assicurare la sicurezza della trasmissione dei dati con HTTPS.
  * **Semplificare gli endpoint**: Utilizzare `[AsParameters]` con attenzione per raggruppare parametri logicamente correlati solo quando rende la firma dell'endpoint più chiara e gestibile.
  * **Documentare le API**:  Documentare chiaramente quali parametri si aspettano gli endpoint e da dove provengono (route, query, body, form).

**Link Utili Principali:**

  * [Documentazione ufficiale Microsoft: Parameter binding in Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding) (Pagina principale sul Parameter Binding)

[^1]: [Sicurezza nelle pagine web: CSRF e XSS](../../../../web/web-docs/security/csrf-xss/index.md)
[^2]: [Minimal API: Meccanismi di protezione contro attacchi CSRF](../configure-api-p6/index.md)
