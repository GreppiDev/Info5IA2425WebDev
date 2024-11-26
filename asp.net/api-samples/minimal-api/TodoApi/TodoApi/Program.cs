// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoApi;
//creation of Web application builder
var builder = WebApplication.CreateBuilder(args);
//adding services to the container
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
//adding API explorer
builder.Services.AddEndpointsApiExplorer();
//adding OpenAPI configuration
builder.Services.AddOpenApiDocument(config => 
{
	config.DocumentName = "TodoAPI";
	config.Title = "TodoAPI v1";
	config.Version = "v1";
});
//creation of Web application
var app = builder.Build();

//adding middleware for Swagger
if (app.Environment.IsDevelopment())
{
	app.UseOpenApi();
	app.UseSwaggerUi(config => 
	
	{
		config.DocumentTitle = "TodoAPI";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}


//creations of API routes
app.MapGet("/", () => "Hello World!");
//uso di MapGroup per raggruppare le API
var todoItems = app.MapGroup("/todoitems");
//get all todoitems
todoItems.MapGet("/", async (TodoDb db) => TypedResults.Ok( await db.Todos.ToListAsync()));

//get all completed todoitems
//si noti che le lambda expressions permettono di inferire il tipo di ritorno
//ad esempio in questo caso il tipo di ritorno è List<Todo>
// la lambda expression è async Task<Ok<List<Todo>>> (TodoDb db) => TypedResults.Ok( await db.Todos.Where(x => x.IsComplete).ToListAsync())
todoItems.MapGet("/complete", async (TodoDb db) => TypedResults.Ok( await db.Todos.Where(x => x.IsComplete).ToListAsync()));
// todoItems.MapGet("/complete", async (TodoDb db) => {

// 	 var results = await db.Todos.Where(x => x.IsComplete).ToListAsync();
// 	 return Results.Ok(results);

// 	});

//get specific todoitem
//nel caso in cui si utilizzi TypedResults al posto di Results è possibile specificare il tipo di ritorno
//ma bisogna stare attenti a come si gestiscono i tipi di ritorno
//nel caso in cui si utilizzi Results è possibile specificare il tipo di ritorno in modo più semplice
//nel caso in cui si utilizzi TypedResults è necessario specificare il tipo di ritorno in modo più esplicito
// ad esempio in questo caso si specifica che il tipo di ritorno è Results<Ok<Todo>, NotFound>
//ma sarebbe anche possibile specificare il tipo di ritorno come IResult
//In cosa sta la differenza?
//TypedResults.Ok(todo) ritorna un oggetto di tipo Ok<Todo>
//Results.Ok(todo) ritorna un oggetto di tipo Ok
//quindi se si utilizza TypedResults è necessario specificare il tipo di ritorno in modo più esplicito.

//La differenza sta anche nell'utilizzo di OpenAPI, infatti se si utilizza TypedResults è possibile specificare il tipo di ritorno
//in modo più esplicito e quindi OpenAPI può generare una documentazione più precisa

todoItems.MapGet("/{id}", async Task<Results<Ok<Todo>, NotFound>> (TodoDb db, int id) =>
{
	var todo = await db.Todos.FindAsync(id);
	return todo is not null ? TypedResults.Ok(todo) : TypedResults.NotFound();
});
// {
// 	var todo = await db.Todos.FindAsync(id);
// 	if (todo == null)
// 	{
// 		return Results.NotFound();
// 	}
// 	return Results.Ok(todo);
// });

//insert specific todoitem
//here we use Dependency Injection 
todoItems.MapPost("/", async (TodoDb db, Todo todo) =>

{
	db.Todos.Add(todo);
	await db.SaveChangesAsync();
	return Results.Created($"/todoitems/{todo.Id}", todo);
});

//update specific todoitem
todoItems.MapPut("/{id}", async (TodoDb db, int id, Todo todo) =>

{
	//check if the id in the URL matches the id in the body
	if (id != todo.Id)
	{
		return Results.BadRequest("Id mismatch");
	}
	var existing = await db.Todos.FindAsync(id);
	//check if the todoitem exists
	if (existing == null)
	{
		return Results.NotFound();
	}
	//update the todoitem
	existing.Name = todo.Name;
	existing.IsComplete = todo.IsComplete;
	await db.SaveChangesAsync();
	//return a 204 No Content response
	return Results.NoContent();
});

//delete specific todoitem
todoItems.MapDelete("/{id}", async (TodoDb db, int id) =>

{
	// //verify if the todoitem exists
	// var todo = await db.Todos.FindAsync(id);
	// if (todo == null)
	// {
	// 	return Results.NotFound();
	// }
	// //remove the todoitem
	// db.Todos.Remove(todo);
	// await db.SaveChangesAsync();
	// //return a 204 No Content response
	// return Results.NoContent();

	if (await db.Todos.FindAsync(id) is Todo todo)
	{
		db.Todos.Remove(todo);
		await db.SaveChangesAsync();
		return Results.NoContent();
	}
	return Results.NotFound();
});

app.Run();