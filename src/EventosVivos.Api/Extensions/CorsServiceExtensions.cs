namespace EventosVivos.Api.Extensions;

/// <summary>Nombres de políticas CORS, centralizados para evitar literales mágicos repetidos.</summary>
public static class CorsPolicies
{
    public const string Frontend = "frontend";
}

/// <summary>Configuración de CORS para el frontend (incluye credenciales requeridas por SignalR).</summary>
public static class CorsServiceExtensions
{
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:4200"];

        return services.AddCors(options =>
            options.AddPolicy(CorsPolicies.Frontend, policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    // Requerido por SignalR para el transporte WebSocket con orígenes explícitos.
                    .AllowCredentials()));
    }
}
