namespace EventosVivos.Api.Security;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";
    public string HeaderName { get; init; } = "X-API-KEY";
    public string Value { get; init; } = string.Empty;
}
