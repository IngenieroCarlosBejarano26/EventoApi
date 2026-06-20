namespace EventosVivos.Api.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitRule Global { get; init; } = new() { PermitLimit = 120, WindowSeconds = 60 };

    public RateLimitRule Reservations { get; init; } = new() { PermitLimit = 10, WindowSeconds = 60 };
}

public sealed class RateLimitRule
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
}
