namespace EventosVivos.Application.Common.Abstractions;

/// <summary>
/// Abstrae el reloj del sistema para hacer las reglas temporales deterministas y testeables.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
