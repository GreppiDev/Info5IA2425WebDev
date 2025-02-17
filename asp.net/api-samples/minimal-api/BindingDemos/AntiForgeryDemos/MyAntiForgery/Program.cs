using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();

builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();


app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) =>
{
	var token = antiforgery.GetAndStoreTokens(context);
	return Results.Content(MyHtml.GenerateForm2("/todo", token), "text/html");
});

// Don't pass a token, fails
app.MapGet("/SkipToken", (HttpContext context, IAntiforgery antiforgery) =>
{
	var token = antiforgery.GetAndStoreTokens(context);
	return Results.Content(MyHtml.GenerateForm2("/todo",token, false ), "text/html");
});

// Post to /todo2. DisableAntiforgery on that endpoint so no token needed.
app.MapGet("/DisableAntiforgery", (HttpContext context, IAntiforgery antiforgery) =>
{
	var token = antiforgery.GetAndStoreTokens(context);
	return Results.Content(MyHtml.GenerateForm2("/todo2", token, false), "text/html");
});

app.MapPost("/todo", ([FromForm] Todo todo) => {
	return Results.Ok(todo);
	});

app.MapPost("/todo2", ([FromForm] Todo todo) => Results.Ok(todo))
												.DisableAntiforgery();
app.Run();

class Todo
{
	public required string Name { get; set; }
	public bool IsCompleted { get; set; }
	public DateTime? DueDate { get; set; }
}

public static class MyHtml
{
	// <snippet_html>
	public static string GenerateForm(string action, 
		AntiforgeryTokenSet token, bool UseToken=true)
	{
		string tokenInput = "";
		if (UseToken)
		{
			tokenInput = $@"<input name=""{token.FormFieldName}""
							 type=""hidden"" value=""{token.RequestToken}"" />";
		}

		return $@"
		<html><body>
			<form action=""{action}"" method=""POST"" enctype=""multipart/form-data"">
				{tokenInput}
				<input type=""text"" name=""name"" />
				<input type=""date"" name=""dueDate"" />
				<input type=""checkbox"" name=""isCompleted"" value=""false"" onchange=""this.value = this.checked"" />
				<input type=""submit"" />
			</form>
		</body></html>
	";
	}

	public static string GenerateForm2(string action,
	AntiforgeryTokenSet token, bool UseToken = true)
	{
		string tokenInput = "";
		if (UseToken)
		{
			tokenInput = $@"<input name=""{token.FormFieldName}""
						 type=""hidden"" value=""{token.RequestToken}"" />";
		}

		return

	$$"""
	<!DOCTYPE html>
	<html lang="it">

	<head>
	<meta charset="utf-8">
		<title>Form protetto con Anti-Forgery</title>
	</head>
	<body>
		<form id="todoForm" action="{{action}}" method="POST" enctype="multipart/form-data" onsubmit="return handleSubmit(event)">
			{{tokenInput}}
			<input type="text" name="name" required />
			<input type="date" name="dueDate" id="dueDate" />
			<input type="checkbox" name="isCompleted" value="false" onchange="this.value = this.checked" />
			<input type="submit" />
		</form>

		<script>
		async function handleSubmit(event) {
				event.preventDefault();
			const form = event.target;
			const formData = new FormData(form);
			
			// Check if dueDate is empty and remove it from FormData if it is
			//il problema è che nel caso in cui dueDate non sia stato impostato nel form, il client invierà una stringa vuota
			//che non può essere convertita in una data. Per evitare questo problema, possiamo rimuovere il campo dueDate dal FormData se dueDate è vuoto.
			//In questo modo, il model binder nel server non cercherà di convertire una stringa vuota in un oggetto DateTime, ma lo lascerà come null.
			const dueDate = formData.get('dueDate');
			if (!dueDate) {
				formData.delete('dueDate');
			}

			try {
				const response = await fetch(form.action, {
					method: 'POST',
					body: formData
				});
				
				if (response.ok) {
					const result = await response.json();
					alert('Success: ' + JSON.stringify(result));
				} else {
					alert('Error submitting form');
				}
			} catch (error) {
				console.error('Error:', error);
				alert('Error submitting form');
			}
		}
		</script>
	</body></html>
	""";
	}
}

