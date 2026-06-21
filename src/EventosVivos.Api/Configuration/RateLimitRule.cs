namespace EventosVivos.Api.Configuration;

public sealed class RateLimitRule
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
}
