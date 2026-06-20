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

        logger.LogInformation("Aplicando migraciones de base de datos...");
        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Base de datos lista.");
    }
}
