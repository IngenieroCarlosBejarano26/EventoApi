using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Reservations.Shared;

namespace EventosVivos.Application.Features.Reservations.ConfirmReservation;

/// <summary>RF-04: Confirmar pago de una reserva.</summary>
public sealed record ConfirmReservationCommand(Guid ReservationId) : ICommand<ReservationDto>;
