using EventosVivos.Domain.Entities;
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

public static class ReservationMappingExtensions
{
    public static ReservationDto ToDto(this Reservation r) => new(
        r.Id,
        r.EventId,
        r.Quantity,
        r.BuyerName,
        r.BuyerEmail.Value,
        r.Status,
        r.Code?.Value,
        r.CreatedAt,
        r.ConfirmedAt,
        r.CancelledAt);
}
