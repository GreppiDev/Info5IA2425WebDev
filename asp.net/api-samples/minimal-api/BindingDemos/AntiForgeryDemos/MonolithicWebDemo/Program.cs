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
	<meta name="viewport" content="width=device-width, initial-scale=1.0">
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

