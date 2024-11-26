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

static async Task<Created<TodoItemDTO>> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
	var todoItem = new Todo
	{
		Name = todoItemDTO.Name,
		IsComplete = todoItemDTO.IsComplete
	};
	db.Todos.Add(todoItem);
	await db.SaveChangesAsync();
	
	todoItemDTO = new TodoItemDTO(todoItem);

	return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
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