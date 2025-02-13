# Guida introduttiva al Parameter Binding in ASP.NET Minimal APIs

- [Guida introduttiva al Parameter Binding in ASP.NET Minimal APIs](#guida-introduttiva-al-parameter-binding-in-aspnet-minimal-apis)
	- [Introduzione al Parameter Binding in ASP.NET Minimal APIs](#introduzione-al-parameter-binding-in-aspnet-minimal-apis)
	- [Binding di Form in ASP.NET Minimal APIs](#binding-di-form-in-aspnet-minimal-apis)
		- [**Primo Esempio di codice (C#, HTML, CSS, JavaScript) - Dati semplici da Form**](#primo-esempio-di-codice-c-html-css-javascript---dati-semplici-da-form)
			- [Spiegazione del codice del primo esempio](#spiegazione-del-codice-del-primo-esempio)
		- [Secondo esempio - differenza tra codifica `application/x-www-form-urlencoded` e `multipart/form-data`](#secondo-esempio---differenza-tra-codifica-applicationx-www-form-urlencoded-e-multipartform-data)
		- [Terzo esempio - differenza tra codifica `application/x-www-form-urlencoded`, `multipart/form-data` e `application/json`](#terzo-esempio---differenza-tra-codifica-applicationx-www-form-urlencoded-multipartform-data-e-applicationjson)
			- [Confronto tra le tre soluzioni di invio dati al server](#confronto-tra-le-tre-soluzioni-di-invio-dati-al-server)
			- [**Perché nella terza pagina non si usa né `URLSearchParams` né `FormData`?**](#perché-nella-terza-pagina-non-si-usa-né-urlsearchparams-né-formdata)
			- [**Motivazioni principali per l'uso di JSON**](#motivazioni-principali-per-luso-di-json)
			- [**Quando usare ciascun metodo?**](#quando-usare-ciascun-metodo)
		- [Binding di dati strutturati da Form](#binding-di-dati-strutturati-da-form)
		- [**`[FromForm]` vs `[AsParameters]`: Differenze e quando usarli**](#fromform-vs-asparameters-differenze-e-quando-usarli)
			- [**Esempio comparativo (C#) - `[FromForm]` vs `[AsParameters]`**](#esempio-comparativo-c---fromform-vs-asparameters)
			- [Best Practice per la gestione dei Form in Minimal APIs](#best-practice-per-la-gestione-dei-form-in-minimal-apis)
	- [Meccanismi Avanzati di Binding (Panoramica)](#meccanismi-avanzati-di-binding-panoramica)
	- [Ordine nell'applicazione delle regole di Binding](#ordine-nellapplicazione-delle-regole-di-binding)
	- [Conclusioni e Best Practice Generali sul Parameter Binding](#conclusioni-e-best-practice-generali-sul-parameter-binding)

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

### **Primo Esempio di codice (C\#, HTML, CSS, JavaScript) - Dati semplici da Form**

**Program.cs (C\# - ASP.NET Minimal API):**

```csharp
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Content(
"""
<!DOCTYPE html>
<html>
<head>
	<title>Form Semplice</title>
</head>
<body>
	<h1>Form Semplice</h1>
	<form id="simpleForm" method="post" action="/submitForm">
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

			fetch('/submitForm', {
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


app.MapPost("/submitForm", ([FromForm] string nome, [FromForm] string cognome) =>
{
	return $"Dati ricevuti dal form: Nome = {nome}, Cognome = {cognome}";
});


app.Run();
```

#### Spiegazione del codice del primo esempio

* **`app.MapGet("/")`**: Definisce un endpoint GET alla radice (`/`) che restituisce una semplice pagina HTML contenente un form.
* Il form ha due campi di testo (`nome` e `cognome`) e utilizza il metodo POST verso l'endpoint `/submitForm`.
* Il codice JavaScript intercetta la submit del form, previene il comportamento predefinito e invia la richiesta POST tramite `fetch`.  **È fondamentale specificare `'Content-Type': 'application/x-www-form-urlencoded'`** nell'header della richiesta `fetch` per simulare una submit form standard.
* **`app.MapPost("/submitForm", ([FromForm] string nome, [FromForm] string cognome) => ...)`**: Definisce un endpoint POST `/submitForm` che accetta due parametri di tipo `string`, `nome` e `cognome`, decorati con `[FromForm]`.
* `[FromForm]` indica che ASP.NET Core deve cercare i valori di `nome` e `cognome` nei dati del form della richiesta POST.
* L'endpoint restituisce una stringa che conferma la ricezione dei dati.

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
		
### Secondo esempio - differenza tra codifica `application/x-www-form-urlencoded` e `multipart/form-data`

Si consideri il seguente esempio, tratto dal [progetto - FormBinding](../../../api-samples/minimal-api/BindingDemos/FormHandling/FormBinding/):

```html
<!doctype html>
<html lang="en">

<head>
	<title>Index</title>

	<!-- Required meta tags -->
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

	<!-- Bootstrap CSS -->
	<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet"
		integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
	<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"
		integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz"
		crossorigin="anonymous"></script>
	<link rel="stylesheet" href="styles.css">
</head>

<body>
	<!-- Navigation Bar -->
	<nav class="navbar navbar-expand-lg navbar-light bg-light">
		<div class="container-fluid">
			<a class="navbar-brand" href="index.html">Todo App</a>
			<button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav"
				aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
				<span class="navbar-toggler-icon"></span>
			</button>
			<div class="collapse navbar-collapse" id="navbarNav">
				<ul class="navbar-nav">
					<li class="nav-item">
						<a class="nav-link" href="index.html">Home</a>
					</li>
					<li class="nav-item">
						<a class="nav-link" href="todos.html">Get All</a>
					</li>
					<li class="nav-item">
						<a class="nav-link" href="get-todo.html">Todo Detail</a>
					</li>
				</ul>
			</div>
		</div>
	</nav>

	<div class="container mb-10">
		<div class="row justify-content-center">
			<div class="col-md-6">
				<div class="card mt-5">
					<div class="card-body">
						<!-- Existing content -->       
						<form action="ap/todos" method="post" enctype="multipart/form-data" onsubmit="submitForm(this);return false;">
							<div class="form-group">
								<label for="name">Name</label>
								<input type="text" required class="form-control" name="name" aria-describedby="emailHelp" placeholder="Enter the name">
							</div>
							<div class="form-group">
								<label for="visibility">Visibility</label>
								<select class="form-control" required name="visibility" title="Visibility">
									<option value="Public">Public</option>
									<option value="Private">Private</option>
								</select>
							</div>
							<div class="form-group mt-2">
								<label for="attachment">Attachment</label>
								<input type="file" class="form-control-file" name="attachment" title="Attachment">
							</div>
							<input type="submit" class="btn btn-primary" value="Submit">
						</form>
					</div>
				</div>
			</div>
		</div>
	</div>
	
	<script>
		async function submitForm(oFormElement) {
			const formData = new FormData(oFormElement);
			try {
				const response = await fetch(oFormElement.action, {
					method: oFormElement.method,
					body: formData
				});
				if (response.ok) {
					alert('Todo item updated successfully.\n' + 'Result: ' + response.status + ' ' +
						response.statusText);
					window.location.href = '/index.html';
				
				}else{
					alert('Todo item updated successfully.\n' + 'Result: ' + response.status + ' ' +
						response.statusText);
				}
			} catch (error) {
				console.error('Error:', error);
			}
		}
	</script>
</body>
</html>
```

**In questo secondo esempio il form viene sottomesso in maniera differente perché la codifica utilizzata nel form è differente.**

* **Perché nella prima pagina serve `URLSearchParams`, mentre nella seconda no?**

	1. **Prima pagina** (dove viene usato `URLSearchParams`):

		- Il form viene inviato con **`application/x-www-form-urlencoded`**.
		- `FormData` viene convertito in una stringa codificata nel formato `key=value&key2=value2`, che è il formato standard per i form HTML senza file allegati.
		- Si utilizza:

			```js
			`body: new URLSearchParams(formData).toString()`
			```

		perché `fetch` non può inviare direttamente un `FormData` come `application/x-www-form-urlencoded`.
	2. **Seconda pagina** (dove si usa solo `FormData` senza `URLSearchParams`):

		- Il form è definito con `enctype="multipart/form-data"`, il che significa che può gestire **file e dati binari**.
		- `FormData` viene passato direttamente nel `body` di `fetch`:

			```js
			`body: formData
			`
			```

		- In questo caso, **non bisogna usare `URLSearchParams`**, perché questo converte i dati in una stringa e perderebbe il supporto per gli allegati (`<input type="file">`).
		- `fetch` con un oggetto `FormData` automaticamente imposta l'**intestazione (`Content-Type: multipart/form-data; boundary=...`)**, che è necessaria per inviare file.

	3. **Riepilogo delle differenze chiave**

	| Caratteristica | Prima Pagina (`URLSearchParams`) | Seconda Pagina (`FormData` diretto) |
	| --- |  --- |  --- |
	| **Formato della richiesta** | `application/x-www-form-urlencoded` | `multipart/form-data` |
	| --- |  --- |  --- |
	| **Metodo di conversione** | `new URLSearchParams(formData).toString()` | `body: formData` diretto |
	| **Supporto per file** | ❌ (non supporta file) | ✅ (supporta file e dati binari) |
	| **Quando usarlo?** | Quando si inviano solo **testo/campi input** | Quando si devono inviare anche **file** |

	Quindi, nella seconda pagina **non è necessario usare `URLSearchParams`**, perché `FormData` supporta direttamente la codifica multipart e mantiene il formato corretto per gli allegati.

### Terzo esempio - differenza tra codifica `application/x-www-form-urlencoded`, `multipart/form-data` e `application/json`

Si consideri il seguente esempio, tratto dal [progetto - FilmAPI v3 - gestione registi](../../../api-samples/minimal-api/FilmAPIWeb3/FilmAPI/):

```html
<!doctype html>
<html lang="it">

<head>
	<meta charset="utf-8">
	<link rel="icon" type="image/x-icon" href="/assets/favicon.ico">
	<link rel="icon" type="image/webp" href="/assets/favicon.webp">
	<meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
	<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet"
		integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
	<link rel="stylesheet" href="/css/styles.css">
	<title>Aggiungi regista</title>
</head>

<body>
	<div id="header-container"></div>
	<main>
		<div class="container mb-10">
			<form id="registaForm">
				<div class="mb-3">
					<label for="nome" class="form-label">Nome</label>
					<input type="text" class="form-control" id="nome" name="nome" minlength="2" required>
				</div>
				<div class="mb-3">
					<label for="cognome" class="form-label">Cognome</label>
					<input type="text" class="form-control" id="cognome" name="cognome" minlength="2" required>
				</div>
				<div class="mb-3">
					<label for="nazionalità" class="form-label">Nazionalità</label>
					<input type="text" class="form-control" id="nazionalità" name="nazionalità" minlength="2" required>
				</div>
				<div class="mb-3">
					<label for="tmdbId" class="form-label">Tmdb ID (opzionale)</label>
					<input type="number" class="form-control" id="tmdbId" name="tmdbId">
				</div>

				<button type="submit" class="btn btn-primary">Aggiungi Regista</button>
			</form>
		</div>
	</main>
	<div id="footer-container"></div>
	<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"
		integrity="sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz" crossorigin="anonymous">
		</script>
	<script src="/js/template-loader.js"></script>
	<script>
		document.addEventListener('DOMContentLoaded', async function () {
			// Carica i template
			await TemplateLoader.initializeTemplates();
		});
		document.getElementById('registaForm').addEventListener('submit', function (event) {
			event.preventDefault();

			const nome = document.getElementById('nome').value;
			const cognome = document.getElementById('cognome').value;
			const nazionalità = document.getElementById('nazionalità').value;
			const tmdbId = document.getElementById('tmdbId').value;
			fetch('/api/registi', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({ nome: nome, cognome: cognome, nazionalità: nazionalità, tmdbId: tmdbId })
			})
				.then(response => response.json())
				.then(data => {
					alert('Regista aggiunto con successo!');
					document.getElementById('registaForm').reset();
				})
				.catch(error => {
					console.error('Errore:', error);
					alert('Si è verificato un errore durante l\'aggiunta del regista.');
				});
		});
	</script>
</body>

</html>
```

#### Confronto tra le tre soluzioni di invio dati al server

In questa sezione si sono esaminati tre modi per inviare dati ad un server. Le differenze chiave sono riportate nella tabella seguente:

| **Caratteristica**        | **Prima Pagina** (`URLSearchParams`) | **Seconda Pagina** (`FormData` diretto) | **Terza Pagina** (`JSON.stringify()`) |
|---------------------------|--------------------------------------|------------------------------------------|--------------------------------------|
| **Formato della richiesta** | `application/x-www-form-urlencoded` | `multipart/form-data` | `application/json` |
| **Metodo di conversione** | `new URLSearchParams(formData).toString()` | `body: formData` diretto | `JSON.stringify({...})` |
| **Supporto per file** | ❌ No | ✅ Sì | ❌ No |
| **Quando usarlo?** | Quando si inviano solo **testo/campi input** senza file | Quando si devono inviare **file + dati di testo** | Quando si lavora con **API JSON-based** (es. REST API) |
| **Struttura dei dati** | Stringa `key=value&key2=value2` | Dati con **boundary** (supporta file) | Oggetto JSON `{ "chiave": "valore" }` |
| **Intestazione (`Content-Type`)** | `application/x-www-form-urlencoded` | `multipart/form-data` (gestito da `FormData`) | `application/json` |

#### **Perché nella terza pagina non si usa né `URLSearchParams` né `FormData`?**

Nella **terza pagina**, i dati vengono inviati a un'API REST (`/api/registi`), che si aspetta un **JSON nel corpo della richiesta**.  
Qui, `fetch` invia i dati con:

```js
body: JSON.stringify({ nome: nome, cognome: cognome, nazionalità: nazionalità, tmdbId: tmdbId })
```

e imposta l'intestazione corretta:

```js
headers: { 'Content-Type': 'application/json' }
```

#### **Motivazioni principali per l'uso di JSON**

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

#### **Quando usare ciascun metodo?**

- **`URLSearchParams`** → Quando il backend si aspetta dati come stringa URL (`application/x-www-form-urlencoded`). Tipico dei form HTML tradizionali.
- **`FormData`** → Quando dobbiamo inviare file o dati binari. Utile per upload di immagini o allegati.
- **`JSON.stringify()`** → Quando l'API accetta JSON. Ideale per comunicare con API REST moderne.

### Binding di dati strutturati da Form

I form HTML possono contenere controlli più complessi per raccogliere dati strutturati. Vediamo esempi con Dropdown List, Checkbox e File Input.

1. **Dropdown List (Select)**

	**Esempio di codice (C\#, HTML, CSS, JavaScript) - Dropdown List**

	**Program.cs (C\# - ASP.NET Minimal API):**

	```csharp
	using Microsoft.AspNetCore.Mvc;

	var builder = WebApplication.CreateBuilder(args);
	var app = builder.Build();

	app.MapGet("/", () => Results.Content(
	"""
	<!DOCTYPE html>
	<html>
	<head>
		<title>Form con Dropdown</title>
	</head>
	<body>
		<h1>Form con Dropdown</h1>
		<form id="dropdownForm" method="post" action="/submitDropdown">
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

				fetch('/submitDropdown', {
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


	app.MapPost("/submitDropdown", ([FromForm] string paese) =>
	{
		return $"Paese selezionato: {paese}";
	});


	app.Run();
	```

	**Spiegazione:**

	* L'HTML include un elemento `<select>` (dropdown list) con l'attributo `name="paese"`.
	* L'endpoint `/submitDropdown` accetta un parametro `paese` di tipo `string` con l'attributo `[FromForm]`.
	* Quando l'utente seleziona un'opzione dalla dropdown e invia il form, il valore selezionato viene correttamente bound al parametro `paese` nell'endpoint.

2. **Checkbox**

	**Esempio di codice (C\#, HTML, CSS, JavaScript) - Checkbox**

	**Program.cs (C\# - ASP.NET Minimal API):**

	```csharp
	using Microsoft.AspNetCore.Mvc;

	var builder = WebApplication.CreateBuilder(args);
	var app = builder.Build();

	app.MapGet("/", () => Results.Content(
	"""
	<!DOCTYPE html>
	<html>
	<head>
		<title>Form con Checkbox</title>
	</head>
	<body>
		<h1>Form con Checkbox</h1>
		<form id="checkboxForm" method="post" action="/submitCheckbox">
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
				const interessiSelezionati = formData.getAll('interessi'); // getAll per ottenere tutti i valori delle checkbox con lo stesso nome

				fetch('/submitCheckbox', {
					method: 'POST',
					headers: {
						'Content-Type': 'application/x-www-form-urlencoded'
					},
					body: new URLSearchParams(new URLSearchParams({ interessi: interessiSelezionati })).toString() // Invia gli interessi come array
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


	app.MapPost("/submitCheckbox", ([FromForm] string[] interessi) =>
	{
		if (interessi != null && interessi.Length > 0)
		{
			return $"Interessi selezionati: {string.Join(", ", interessi)}";
		}
		else
		{
			return "Nessun interesse selezionato.";
		}
	});


	app.Run();
	```

	**Spiegazione:**

	* L'HTML include diverse `<input type="checkbox">` con lo stesso `name="interessi"`.  **È importante che le checkbox correlate abbiano lo stesso `name` per essere raggruppate.**  Ogni checkbox ha un `value` che rappresenta l'interesse.
	* L'endpoint `/submitCheckbox` accetta un parametro `interessi` di tipo `string[]` (array di stringhe) con `[FromForm]`.
	* Quando l'utente seleziona una o più checkbox e invia il form, ASP.NET Core binderà un **array di stringhe** al parametro `interessi`, contenente i valori delle checkbox selezionate.
	* Lato client, JavaScript utilizza `formData.getAll('interessi')` per ottenere **tutti** i valori delle checkbox con il nome 'interessi' e li invia al server.

3. **Input di tipo File (File Upload)**

	**Considerazioni sul binding di file**

	Il binding di file da form è leggermente diverso dagli altri tipi di dati.  Quando si inviano file tramite form, è necessario assicurarsi che:

	* Il form abbia l'attributo `enctype="multipart/form-data"`. Questo è **essenziale** per l'invio di file.
	* Lato server, il parametro dell'endpoint sia di tipo `IFormFile`.  `IFormFile` è un'interfaccia in ASP.NET Core che rappresenta un file caricato tramite HTTP.

	**Esempio di codice (C\#, HTML, CSS, JavaScript) - File Upload**

	**Program.cs (C\# - ASP.NET Minimal API):**

	```csharp
	using Microsoft.AspNetCore.Mvc;

	var builder = WebApplication.CreateBuilder(args);
	var app = builder.Build();

	app.MapGet("/", () => Results.Content(
	"""
	<!DOCTYPE html>
	<html>
	<head>
		<title>Form con File Upload</title>
	</head>
	<body>
		<h1>Form con File Upload</h1>
		<form id="fileForm" method="post" action="/uploadFile" enctype="multipart/form-data">
			<label for="fileInput">Seleziona un file:</label><br>
			<input type="file" id="fileInput" name="fileInput"><br><br>
			<input type="submit" value="Upload File">
		</form>

		<script>
			document.getElementById('fileForm').addEventListener('submit', function(event) {
				event.preventDefault();

				const formData = new FormData(this);

				fetch('/uploadFile', {
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


	app.MapPost("/uploadFile", async (IFormFile fileInput) =>
	{
		if (fileInput != null && fileInput.Length > 0)
		{
			// Salva il file (esempio semplificato: salva in memoria)
			using var stream = new MemoryStream();
			await fileInput.CopyToAsync(stream);
			byte[] fileBytes = stream.ToArray();

			// Qui potresti salvare fileBytes su disco, database, ecc.
			return $"File '{fileInput.FileName}' caricato con successo. Dimensione: {fileInput.Length} bytes.";
		}
		else
		{
			return "Nessun file caricato.";
		}
	});


	app.Run();
	```

	**Spiegazione:**

	* **HTML:**
		* L'attributo `enctype="multipart/form-data"` è **essenziale** nell'elemento `<form>`.
		* `<input type="file" id="fileInput" name="fileInput">` definisce l'input per la selezione del file.
	* **JavaScript:**
		* `FormData` viene creato dal form. **Per i file upload, `FormData` viene inviato direttamente come body della richiesta `fetch` senza ulteriori modifiche (non serve `URLSearchParams`) perché è già in formato `multipart/form-data`.**
	* **C\# (Endpoint `/uploadFile`):**
		* Il parametro `fileInput` è di tipo `IFormFile` con `[FromForm]` (anche se in questo caso `[FromForm]` è implicito per `IFormFile`).
		* L'endpoint controlla se `fileInput` non è `null` e ha una lunghezza maggiore di zero (file caricato).
		* Esempio semplificato di salvataggio del file in memoria (`MemoryStream`). In un'applicazione reale, dovresti salvare il file su disco, database, o servizio di storage.
		* Restituisce un messaggio di successo o errore.

### **`[FromForm]` vs `[AsParameters]`: Differenze e quando usarli**

Sia `[FromForm]` che `[AsParameters]` sono utilizzati per il binding di dati provenienti dal form, ma hanno utilizzi e comportamenti differenti.

* **`[FromForm]`**:  Si applica `[FromForm]` **singolarmente ad ogni parametro** dell'endpoint che si vuole mettere in binding con i dati del form.  È il modo **più esplicito e controllato** per definire da dove provengono i dati.  È **raccomandato** per la maggior parte dei casi, specialmente quando si gestiscono form complessi o si vuole chiarezza sulla provenienza dei dati.

* **`[AsParameters]`**:  Si applica `[AsParameters]` **ad un tipo complesso** (classe o record) come parametro dell'endpoint. ASP.NET Core tenterà di mettere in binding le proprietà di questo tipo complesso dai dati del form.  `[AsParameters]` è utile per raggruppare logicamente parametri correlati provenienti dal form in un singolo oggetto, rendendo la firma dell'endpoint più pulita, **ma può rendere meno esplicito da dove provengono i dati**.

#### **Esempio comparativo (C\#) - `[FromForm]` vs `[AsParameters]`**

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
	  * Quando si hanno solo pochi parametri da collegare dal form.
	  * Quando vuoi essere molto preciso sulla provenienza dei dati.
  * **Si usa `[AsParameters]`**:
	  * Quando si hanno **molti parametri correlati** provenienti dal form e si vuole **semplificare la firma dell'endpoint**.
	  * Per raggruppare logicamente parametri in un oggetto.
	  * Quando la provenienza dei dati è ovvia dal contesto (es. un endpoint chiaramente dedicato all'elaborazione di un form specifico).

#### Best Practice per la gestione dei Form in Minimal APIs

  * **Utilizzare `[FromForm]` esplicitamente**: Per chiarezza e controllo, usa sempre `[FromForm]` quando si vuole collegare dati da un form, specialmente per tipi semplici.
  * **Validare i dati in ingresso**:  **Sempre** validare i dati ricevuti dal form lato server per sicurezza e integrità dei dati. Puoi usare Data Annotations, FluentValidation o validazione manuale.
  * **Gestire gli errori di validazione**:  Restituire feedback appropriati all'utente in caso di errori di validazione (es. `Results.ValidationProblem()`).
  * **Proteggere i form con Anti-Forgery Token**: Implementare la protezione Anti-Forgery (si veda la sezione successiva) per prevenire attacchi CSRF, soprattutto per form che modificano dati.
  * **Utilizza HTTPS**: Assicurare che la tua applicazione utilizzi HTTPS per proteggere la trasmissione dei dati del form.
  * **Considerare l'uso di View Model**: Per form complessi, si potrebbe voler creare un View Model (classi dedicate) per rappresentare i dati del form e facilitare la validazione e il binding.

**Link Utili:**

  * [Documentazione ufficiale Microsoft:  Binding dei parametri del form in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) (Sebbene focalizzata su MVC, i concetti di binding dei form sono simili anche in Minimal APIs)
  * [Documentazione ufficiale Microsoft:  Attributo FromForm](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.fromformattribute)

## Meccanismi Avanzati di Binding (Panoramica)

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

-----

## Conclusioni e Best Practice Generali sul Parameter Binding

**Riepilogo delle best practice per un binding efficace e sicuro:**

  * **Essere espliciti**: Utilizzare attributi come `[FromForm]`, `[FromQuery]`, `[FromBody]`, `[FromRoute]` per dichiarare chiaramente la provenienza dei dati dei parametri.
  * **Validare sempre i dati**: Effettuare la validazione dei dati in ingresso lato server, indipendentemente dalla validazione lato client.
  * **Proteggere i form**: Implementare sempre la protezione Anti-Forgery per i form che modificano dati.
  * **Utilizzare HTTPS**:  Assicurare la sicurezza della trasmissione dei dati con HTTPS.
  * **Semplificare gli endpoint**: Utilizzare `[AsParameters]` con attenzione per raggruppare parametri logicamente correlati solo quando rende la firma dell'endpoint più chiara e gestibile.
  * **Documentare le API**:  Documentare chiaramente quali parametri si aspettano gli endpoint e da dove provengono (route, query, body, form).

**Link Utili Principali:**

  * [Documentazione ufficiale Microsoft: Parameter binding in Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding) (Pagina principale sul Parameter Binding)
