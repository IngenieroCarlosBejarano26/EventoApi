namespace EventosVivos.Application.Common.Exceptions;

/// <summary>
/// Señala un conflicto de concurrencia optimista. La infraestructura traduce la
/// DbUpdateConcurrencyException de EF Core a esta excepción agnóstica de persistencia,
/// permitiendo que la capa de aplicación reintente sin acoplarse a EF Core.
/// </summary>
public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
