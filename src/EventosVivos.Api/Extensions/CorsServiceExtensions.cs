namespace EventosVivos.Api.Extensions;

public static class CorsPolicies
{
    public const string Frontend = "frontend";
}

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
                    .AllowCredentials()));
    }
}
