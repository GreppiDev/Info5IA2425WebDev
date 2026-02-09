using System;
using Microsoft.EntityFrameworkCore;
using MyApi.Model;
namespace MyApi.Data;




// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas => Set<Pizza>();
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
    public DbSet<Pizza> Pizzas { get; set; } = null!;
}