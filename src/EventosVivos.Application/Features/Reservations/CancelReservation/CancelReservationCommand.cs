using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

public sealed record CancelReservationCommand(Guid ReservationId) : ICommand<ReservationDto>;
