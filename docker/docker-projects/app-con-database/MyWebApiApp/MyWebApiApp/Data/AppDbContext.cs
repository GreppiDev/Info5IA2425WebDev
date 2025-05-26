using Microsoft.EntityFrameworkCore;
using MyWebApiApp.Models;

namespace MyWebApiApp.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntry> TestEntries { get; set; }

    // Opzionale: si può ulteriormente configurare il modello qui se necessario
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    //     // Esempio: modelBuilder.Entity<TestEntry>().ToTable("CustomTestEntries");
    // }
}