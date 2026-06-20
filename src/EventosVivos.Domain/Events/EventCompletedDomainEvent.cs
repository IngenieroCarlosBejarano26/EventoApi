using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

public sealed record EventCompletedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOn) : IDomainEvent;
