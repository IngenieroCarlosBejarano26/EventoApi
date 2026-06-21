using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Events;
using MediatR;

namespace EventosVivos.Application.DomainEventHandlers;

public sealed class ReservationCreatedNotificationHandler(INotificationService notifications)
    : INotificationHandler<ReservationCreatedDomainEvent>
{
    public Task Handle(ReservationCreatedDomainEvent e, CancellationToken cancellationToken) =>
        notifications.NotifyAsync(
            e.BuyerEmail,
            "Reserva recibida",
            $"Hemos registrado tu reserva de {e.Quantity} entrada(s). Está pendiente de pago.",
            cancellationToken);
}
