using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Features.Reservations.Shared;

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
