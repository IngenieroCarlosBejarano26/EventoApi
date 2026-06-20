using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Domain.Events;

public sealed record ReservationCancelledDomainEvent(
    Guid ReservationId,
    Guid EventId,
    int Quantity,
    bool TicketsReleased,
    ReservationStatus FinalStatus,
    DateTimeOffset OccurredOn) : IDomainEvent;
