using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

public sealed record ReservationConfirmedDomainEvent(
    Guid ReservationId,
    Guid EventId,
    string ReservationCode,
    string BuyerEmail,
    DateTimeOffset OccurredOn) : IDomainEvent;
