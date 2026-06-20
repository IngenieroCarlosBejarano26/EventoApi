using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

/// <summary>Se emite cuando un evento se crea correctamente (permite reflejarlo en vivo).</summary>
public sealed record EventCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOn) : IDomainEvent;
