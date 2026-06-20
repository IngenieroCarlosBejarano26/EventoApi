using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

public sealed record EventCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOn) : IDomainEvent;
