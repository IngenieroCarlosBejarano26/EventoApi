namespace EventosVivos.Application.Common.Exceptions;

/// <summary>
/// Excepción de validación de entrada (no de negocio). El Global Exception Middleware
/// la traduce a un 400 con ValidationProblemDetails y el diccionario de errores por campo.
/// </summary>
public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Se produjeron uno o más errores de validación.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
