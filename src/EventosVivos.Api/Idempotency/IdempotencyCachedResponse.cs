namespace EventosVivos.Api.Idempotency;

internal sealed record IdempotencyCachedResponse(int StatusCode, object? Body);
