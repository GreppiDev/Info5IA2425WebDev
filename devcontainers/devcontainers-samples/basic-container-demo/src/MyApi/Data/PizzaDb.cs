using Microsoft.EntityFrameworkCore;
using MyApi.Model;
namespace MyApi.Data;




// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas => Set<Pizza>();
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
public class PizzaDb(DbContextOptions options) : DbContext(options)
{
    public DbSet<Pizza> Pizzas { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pizza>().HasData(
                new Pizza { Id = 1, Name = "Montemagno", Description = "Pizza shaped like a great mountain" },
                new Pizza { Id = 2, Name = "The Galloway", Description = "Pizza shaped like a submarine, silent but deadly" },
                new Pizza { Id = 3, Name = "The Noring", Description = "Pizza shaped like a Viking helmet, where's the mead" }
            );
    }

}