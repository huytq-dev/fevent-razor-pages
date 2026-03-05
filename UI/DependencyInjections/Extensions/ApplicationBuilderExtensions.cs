using Microsoft.EntityFrameworkCore;

namespace UI;

public static class ApplicationBuilderExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("?? Applying migrations...");
        Console.WriteLine($">>> [Connection string:] {db.Database.GetConnectionString()}");
        await db.Database.MigrateAsync();

        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine(">>> [[[Seeding development data]]]");
            await AppData.SeedAsync(db);
            Console.WriteLine(">>> >>> Seeding completed!");
        }
    }
}
