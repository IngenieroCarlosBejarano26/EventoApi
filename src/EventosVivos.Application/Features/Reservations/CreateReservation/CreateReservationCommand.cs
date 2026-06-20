using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

public sealed record CreateReservationCommand(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail) : ICommand<ReservationDto>;
