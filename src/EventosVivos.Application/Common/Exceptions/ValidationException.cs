namespace EventosVivos.Application.Common.Exceptions;

public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Se produjeron uno o mÃ¡s errores de validaciÃ³n.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
