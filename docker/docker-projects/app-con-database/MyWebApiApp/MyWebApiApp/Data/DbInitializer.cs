using Microsoft.EntityFrameworkCore;
using MyWebApiApp.Models;

namespace MyWebApiApp.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        // Applica le migrazioni al database
        await context.Database.MigrateAsync();

        // Aggiungi dati di esempio se la tabella è vuota
        if (!await context.TestEntries.AnyAsync())
        {
            var sampleEntry = new TestEntry
            {
                Message = $"Primo record di esempio - {DateTime.UtcNow}"
            };

            context.TestEntries.Add(sampleEntry);
            await context.SaveChangesAsync();
        }
    }
}
