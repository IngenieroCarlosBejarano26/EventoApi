using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Reservations.Shared;

public sealed record ReservationDto(
    Guid Id,
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    ReservationStatus Status,
    string? Code,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CancelledAt);
