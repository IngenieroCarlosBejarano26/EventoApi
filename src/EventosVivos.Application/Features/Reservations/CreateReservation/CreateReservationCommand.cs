using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;

namespace EventosVivos.Application.Features.Reservations.CreateReservation;

/// <summary>RF-03: Reservar entradas.</summary>
public sealed record CreateReservationCommand(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail) : ICommand<ReservationDto>;
