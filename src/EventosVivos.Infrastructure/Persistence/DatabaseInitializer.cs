using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Persistence;

/// <summary>Aplica migraciones pendientes al arrancar (seed de venues incluido vía HasData).</summary>
public static class DatabaseInitializer
{
    public static async Task ApplyMigrationsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        // Con un proveedor relacional (PostgreSQL) aplicamos migraciones; con uno no relacional
        // (InMemory en pruebas de integración) basta con crear el modelo + seed.
        if (context.Database.IsRelational())
        {
            logger.LogInformation("Aplicando migraciones de base de datos...");
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        logger.LogInformation("Base de datos lista.");
    }
}
