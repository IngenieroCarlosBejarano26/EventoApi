using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

public sealed record ReservationCreatedDomainEvent(
    Guid ReservationId,
    Guid EventId,
    int Quantity,
    string BuyerEmail,
    DateTimeOffset OccurredOn) : IDomainEvent;
