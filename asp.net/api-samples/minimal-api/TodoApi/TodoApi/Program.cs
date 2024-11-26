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
var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);


app.Run();

//metodi richiamati dagli Endpoint Routes 
//in questo caso i metodi espongono un tipo di ritorno IResult
//che è un'interfaccia che rappresenta un risultato di una richiesta HTTP
//e può essere un risultato di successo o di errore

//Con questi metodi OpenAPI può generare la documentazione automaticamente
//ma non è in grado di definire i tipi di ritorno dei metodi, perché, a differenza
//delle lambda expression, i metodi devono dichiarare il tipo di ritorno e non 
//c'è una inferenza di tipo come avviene con le lambda expression

static async Task<IResult> GetAllTodos(TodoDb db)
{
	return TypedResults.Ok(await db.Todos.ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
	return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
	return await db.Todos.FindAsync(id)
		is Todo todo
			? TypedResults.Ok(todo)
			: TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
{
	db.Todos.Add(todo);
	await db.SaveChangesAsync();

	return TypedResults.Created($"/todoitems/{todo.Id}", todo);
}

static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoDb db)
{
	var todo = await db.Todos.FindAsync(id);

	if (todo is null) return TypedResults.NotFound();

	todo.Name = inputTodo.Name;
	todo.IsComplete = inputTodo.IsComplete;

	await db.SaveChangesAsync();

	return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
	if (await db.Todos.FindAsync(id) is Todo todo)
	{
		db.Todos.Remove(todo);
		await db.SaveChangesAsync();
		return TypedResults.NoContent();
	}

	return TypedResults.NotFound();
}