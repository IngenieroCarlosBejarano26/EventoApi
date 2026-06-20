using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;

namespace EventosVivos.Application.Features.Reservations.ConfirmReservation;

public sealed record ConfirmReservationCommand(Guid ReservationId) : ICommand<ReservationDto>;
