
using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PizzaStoreV3.Data;
using PizzaStoreV3.Model;

var builder = WebApplication.CreateBuilder(args);
// 👇 si aggiunga la connection string
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=DefaultPizzas.db";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// builder.Services.AddOpenApi("internal", options => 
// {
// 	options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
// }); // Document name is internal

//adding API explorer
builder.Services.AddEndpointsApiExplorer();
//adding OpenAPI configuration with NSwag
//questa configurazione è richiesta solo nel caso in cui si voglia il supporto a Swagger tramite NSwag
builder.Services.AddOpenApiDocument(config =>
{
	config.DocumentName = "PizzaStoreAPIv3";
	config.Title = "PizzaStore v3";
	config.Version = "v1";
});

//adding services to the container
if (builder.Environment.IsDevelopment())
{
	//il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
builder.Services.AddProblemDetails();
// 👇 si aggiunga il DbContext per l'accesso al database
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));  
// 👇 si modifichi il DbContext per l'accesso al database 
builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString));
var app = builder.Build();


//app.UseHttpsRedirection();

//adding middleware for Swagger and OpenAPI
if (app.Environment.IsDevelopment())
{
	//adding middleware for OpenAPI
	app.MapOpenApi();
	//adding middleware for Swagger
	 app.UseOpenApi();
	//adding web UI for Swagger with NSwag
	app.UseSwaggerUi(config =>

	{
		config.DocumentTitle = "PizzaStore API v3";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}

//Adds a middleware to the pipeline that will catch exceptions, log them, and re-execute
//the request in an alternate pipeline. The request will not be re-executed if
//the response has already started.
// app.UseExceptionHandler(exceptionHandlerApp
// 	=> exceptionHandlerApp.Run(async context
// 		=> await Results.Problem()
// 					 .ExecuteAsync(context)));
// //Adds a StatusCodePages middleware with the specified handler that checks for responses with 
// //status codes between 400 and 599 that do not have a body.
// app.UseStatusCodePages(async statusCodeContext
// 	=> await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
// 				 .ExecuteAsync(statusCodeContext.HttpContext));

app.MapGet("/exception", () =>
{
	throw new InvalidOperationException("Sample Exception");
});


//creations of API Endpoint Routes
app.MapGet("/", () => "Hello World!");

//in questo caso non cambia nulla nel documento di OpenAPI prodotto
//perché il return type verrebbe inferito automaticamente
//app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync()).Produces<IList<Pizza>>();

//in questo caso non cambia nulla nel documento di OpenAPI prodotto
//perché il return type verrebbe inferito automaticamente
//app.MapGet("/pizzas", [ProducesResponseType<List<Pizza>>(200)] async (PizzaDb db) => await db.Pizzas.ToListAsync());

//in questo caso non cambia nulla nel documento di OpenAPI prodotto
//perché il return type verrebbe inferito automaticamente
app.MapGet("/pizzas", async (PizzaDb db) => {
	
	var pizzas =await db.Pizzas.ToListAsync();
	return TypedResults.Ok(pizzas);
	});

var pizzaGroup = app.MapGroup("pizza").WithOpenApi();
pizzaGroup.MapPost("/", async (PizzaDb db,[Description("The pizza to post") ] Pizza pizza) =>
{
	db.Pizzas.Add(pizza);
	await db.SaveChangesAsync();
	return Results.Created($"/pizza/{pizza.Id}", pizza);
});
// app.MapGet("/pizza/{id}", async Task<Results<Ok<Pizza>, NotFound>> (int id, PizzaDb db) =>
// {
// 	return await db.Pizzas.FindAsync(id)
// 			is Pizza pizza
// 				? TypedResults.Ok(pizza)
// 				: TypedResults.NotFound();
// });
app.MapGet("/pizza/{id}", async (int id, PizzaDb db) =>
	{
		if(id<0) 
		{
				return Results.BadRequest();
		}
		return	await db.Pizzas.FindAsync(id)
		is Pizza pizza
		? Results.Ok(pizza)
		: Results.NotFound();
	})
   .Produces<Pizza>(StatusCodes.Status200OK)
   .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status400BadRequest);
//pizzaGroup.MapGet("/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

pizzaGroup.MapPut("/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
	var pizza = await db.Pizzas.FindAsync(id);
	if (pizza is null) return Results.NotFound();
	pizza.Name = updatePizza.Name;
	pizza.Description = updatePizza.Description;
	await db.SaveChangesAsync();
	return Results.NoContent();
});
pizzaGroup.MapDelete("/{id}", async (PizzaDb db, int id) =>
{
	var pizza = await db.Pizzas.FindAsync(id);
	if (pizza is null)
	{
		return Results.NotFound();
	}
	db.Pizzas.Remove(pizza);
	await db.SaveChangesAsync();
	return Results.Ok();
});

app.MapGet("/extension-methods", () => "Hello world!")
  .WithSummary("This is a summary.")
  //è possibile usare CommonMark per la descrizione
  //https://commonmark.org/
  //https://www.markdownguide.org/
  //https://markdown-it.github.io/
  //Per il markdown può essere utile l'uso del nuovo operatore per le stringhe triple quote
  //https://devblogs.microsoft.com/dotnet/csharp-11-preview-updates/#raw-string-literals
  //il testo stampato è preso da:
  //https://github.com/swagger-api/swagger-ui/blob/master/README.md
  //https://raw.githubusercontent.com/swagger-api/swagger-ui/master/README.md
  
  //https://learn.microsoft.com/en-us/dotnet/api/microsoft.openapi.models.openapioperation
  //https://learn.microsoft.com/en-us/dotnet/api/microsoft.openapi.models.openapioperation.requestbody
  //The request body applicable for this operation. The requestBody is only supported in HTTP methods where
  //the HTTP 1.1 specification RFC7231 has explicitly defined semantics for request bodies.
  //In other cases where the HTTP spec is vague, requestBody SHALL be ignored by consumers.

  .WithDescription(
			"""
			# <img src="https://raw.githubusercontent.com/swagger-api/swagger.io/wordpress/images/assets/SWU-logo-clr.png" width="300">

			[![NPM version](https://badge.fury.io/js/swagger-ui.svg)](http://badge.fury.io/js/swagger-ui)
			[![Build Status](https://jenkins.swagger.io/view/OSS%20-%20JavaScript/job/oss-swagger-ui-master/badge/icon?subject=jenkins%20build)](https://jenkins.swagger.io/view/OSS%20-%20JavaScript/job/oss-swagger-ui-master/)
			[![npm audit](https://jenkins.swagger.io/buildStatus/icon?job=oss-swagger-ui-security-audit&subject=npm%20audit)](https://jenkins.swagger.io/job/oss-swagger-ui-security-audit/lastBuild/console)
			![total GitHub contributors](https://img.shields.io/github/contributors-anon/swagger-api/swagger-ui.svg)

			![monthly npm installs](https://img.shields.io/npm/dm/swagger-ui.svg?label=npm%20downloads)
			![total docker pulls](https://img.shields.io/docker/pulls/swaggerapi/swagger-ui.svg)
			![monthly packagist installs](https://img.shields.io/packagist/dm/swagger-api/swagger-ui.svg?label=packagist%20installs)
			![gzip size](https://img.shields.io/bundlephobia/minzip/swagger-ui.svg?label=gzip%20size)

			## Introduction
			[Swagger UI](https://swagger.io/tools/swagger-ui/) allows anyone — be it your development team or your end consumers — to visualize and interact with the API’s resources without having any of the implementation logic in place. It’s automatically generated from your OpenAPI (formerly known as Swagger) Specification, with the visual documentation making it easy for back end implementation and client side consumption.

			## General
			**👉🏼 Want to score an easy open-source contribution?** Check out our [Good first issue](https://github.com/swagger-api/swagger-ui/issues?q=is%3Aissue+is%3Aopen+label%3A%22Good+first+issue%22) label.

			**🕰️ Looking for the older version of Swagger UI?** Refer to the [*2.x* branch](https://github.com/swagger-api/swagger-ui/tree/2.x).
			"""
			).WithOpenApi();
//👇 i metadati summary e Description inseriti sull'Endpoint /attributes 
//sono presenti sia nella documentazione di OpenAPI che in quella di Swagger
//perché è stato usato il metodo di estensione WithOpenApi
// app.MapGet("/attributes",
//   [EndpointSummary("This is a summary.")]
// [EndpointDescription("This is a description.")]
// () => "Hello world!").WithOpenApi();

//WithTags oppure Tags permettono di inserire i tag sia in OpenAPI che in Swagger
// app.MapGet("/extension-methods", () => "Hello world!")
//   .WithTags("todos", "projects");
// app.MapGet("/attributes",[Tags("todos", "projects")]() => "Hello world!");

//👇 per assicurare coerenza tra OpenAPI e Swagger è opportuno aggiungere .WithOpenApi()
// app.MapGet("/extension-methods", () => "Hello world!").WithName("FromExtensionMethods").WithOpenApi();
// app.MapGet("/attributes",[EndpointName("FromAttributes")]() => "Hello world!").WithOpenApi();


app.MapGet("/attributes",([Description("This is a description.")] string name) => "Hello world!");
app.Run();


