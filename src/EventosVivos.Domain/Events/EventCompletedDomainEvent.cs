using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

/// <summary>RN06: se emite cuando un evento se marca como Completado automáticamente.</summary>
public sealed record EventCompletedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOn) : IDomainEvent;
