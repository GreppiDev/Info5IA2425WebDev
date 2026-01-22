using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS: permette alle pagine servite da Live Server (Frontend) di chiamare il backend su un'altra porta.
// Nota: in produzione conviene restringere ulteriormente o rimuovere questa policy.
builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendDev", policy =>
		policy
			.WithOrigins(
				"http://localhost:5500",
				"http://127.0.0.1:5500",
				"https://localhost:5500",
				"https://127.0.0.1:5500")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseCors("FrontendDev");
}

// In sviluppo, evitare redirect automatici HTTP->HTTPS per non complicare le chiamate cross-origin.
if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}



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
		//body: new URLSearchParams(formData).toString()
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
	if (interessi.Count > 0 && interessi.Any(interesse => interesse != ""))
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
<input type="number" id="eta" name="eta" value="30" ><br><br>

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
			eta: formData.get('eta')||"0",//se non è presente il valore restituisce "0"
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

		// Qui potresti salvare fileBytes su disco, database, ecc.
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
	// Model binding will create a new instance if null
	string fileInfo = "Nessun file caricato.";
	if (dati.ImmagineProfilo != null && dati.ImmagineProfilo.Length > 0)
	{
		fileInfo = $"File '{dati.ImmagineProfilo.FileName}', tipo: '{dati.ImmagineProfilo.ContentType}', dimensione: {dati.ImmagineProfilo.Length} bytes.";
		// Qui potresti salvare il file, ad esempio su disco o in un servizio di storage cloud.
		// Esempio semplificato: salvataggio in memoria per dimostrazione:
		using var stream = new MemoryStream();
		await dati.ImmagineProfilo.CopyToAsync(stream);
		// byte[] fileBytes = stream.ToArray();
		// ... qui potresti fare qualcosa con fileBytes ...
	}

	string interessiString = dati.Interessi != null ? string.Join(", ", dati.Interessi) : "";
	return Results.Ok($"Dati Multipart ricevuti:\n" +
	   $"Nome: {dati.Nome}\n" +
	   $"Età: {dati.Eta}\n" +
	   $"Notifiche Abilitate: {dati.NotificheAbilitate}\n" +
	   $"Genere: {dati.Genere}\n" +
	   $"Paese: {dati.Paese}\n" +
	   $"Interessi: {interessiString}\n" +
	   $"Immagine Profilo: {fileInfo}");
}).DisableAntiforgery();


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

public record DatiFormUrlEncoded(string Nome, int? Eta, bool NotificheAbilitate, string Genere, string Paese, string[] Interessi);

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
