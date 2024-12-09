// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();

using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Annotations;
using TodoApi;
//creation of Web application builder
var builder = WebApplication.CreateBuilder(args);
//adding services to the container
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//adding API explorer
builder.Services.AddEndpointsApiExplorer();
// adding OpenAPI configuration
// builder.Services.AddOpenApiDocument(config =>
// {
// 	config.DocumentName = "TodoAPI";
// 	config.Title = "TodoAPI v1";
// 	config.Version = "v1";
// });
builder.Services.AddOpenApiDocument(options =>
{
	options.PostProcess = document =>
	{
		document.Info = new OpenApiInfo
		{
			Version = "v1",
			Title = "ToDo API",
			Description = "An ASP.NET Core Web API for managing ToDo items",
			TermsOfService = "https://example.com/terms",
			Contact = new OpenApiContact
			{
				Name = "Example Contact",
				Url = "https://example.com/contact"
			},
			License = new OpenApiLicense
			{
				Name = "Example License",
				Url = "https://example.com/license"
			}
		};
	};
});
//creation of Web application
var app = builder.Build();

//adding middleware for Swagger and OpenAPI
if (app.Environment.IsDevelopment())
{
	//adding middleware for OpenAPI
	app.MapOpenApi();
	//adding middleware for Swagger
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
//un esempio di utilizzo di MapGet con attributi
//ProducesResponseType specifica il tipo di risposta, il codice di stato e il tipo di contenuto
//ProducesResponseType richiede using Microsoft.AspNetCore.Mvc
//Description specifica la descrizione dell'endpoint
//Description richiede using System.ComponentModel
app.MapGet("/", [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/plain"), Description("Una semplice Get")] () => "Hello World!").WithName("HelloWorld");
var todoItems = app.MapGroup("/todoitems");
todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);


app.Run();

//metodi richiamati dagli Endpoint Routes 

static async Task<Ok<TodoItemDTO[]>> GetAllTodos(TodoDb db)
{
	return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

static async Task<Ok<List<TodoItemDTO>>> GetCompleteTodos(TodoDb db)
{
	return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<Results<Ok<TodoItemDTO>, NotFound>> GetTodo(int id, TodoDb db)
{
	return await db.Todos.FindAsync(id)
		is Todo todo
			? TypedResults.Ok(new TodoItemDTO(todo))
			: TypedResults.NotFound();
}

//SwaggerResponse richiede using NSwag.Annotations
[SwaggerResponse(StatusCodes.Status201Created, typeof(TodoItemDTO), Description = "Returns the object created ...")]
[ProducesResponseType(typeof(TodoItemDTO), StatusCodes.Status201Created), Description("Create the specified object ...")]
static async Task<Created<TodoItemDTO>> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
	var todoItem = new Todo
	{
		Name = todoItemDTO.Name,
		IsComplete = todoItemDTO.IsComplete,
		Secret = "Secret data"
	};
	db.Todos.Add(todoItem);
	await db.SaveChangesAsync();
	//l'Id viene stabilito dal database
	todoItemDTO = new TodoItemDTO(todoItem);

	return TypedResults.Created($"/todoitems/{todoItemDTO.Id}", todoItemDTO);
}

static async Task<Results<NotFound, NoContent>> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
	var todo = await db.Todos.FindAsync(id);

	if (todo is null) return TypedResults.NotFound();

	todo.Name = todoItemDTO.Name;
	todo.IsComplete = todoItemDTO.IsComplete;

	await db.SaveChangesAsync();

	return TypedResults.NoContent();
}
//SwaggerResponse richiede using NSwag.Annotations
[SwaggerResponse(StatusCodes.Status204NoContent, typeof(void), Description = "Object has been deleted ...")]
[SwaggerResponse(StatusCodes.Status404NotFound, typeof(void), Description = "Object with specified Id was not found ...")]
static async Task<Results<NoContent,NotFound>> DeleteTodo(int id, TodoDb db)
{
	if (await db.Todos.FindAsync(id) is Todo todo)
	{
		db.Todos.Remove(todo);
		await db.SaveChangesAsync();
		return TypedResults.NoContent();
	}
	return TypedResults.NotFound();
}