using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;

namespace EventosVivos.Application.Features.Reservations.CancelReservation;

/// <summary>RF-05: Cancelar una reserva.</summary>
public sealed record CancelReservationCommand(Guid ReservationId) : ICommand<ReservationDto>;
