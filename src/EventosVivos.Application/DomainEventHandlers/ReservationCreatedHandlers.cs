using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Events;
using MediatR;

namespace EventosVivos.Application.DomainEventHandlers;

public sealed class ReservationCreatedAuditHandler(IAuditService audit)
    : INotificationHandler<ReservationCreatedDomainEvent>
{
    public Task Handle(ReservationCreatedDomainEvent e, CancellationToken cancellationToken) =>
        audit.RecordAsync(
            "ReservationCreated",
            $"Reserva {e.ReservationId} creada para el evento {e.EventId} ({e.Quantity} entradas).",
            cancellationToken);
}

public sealed class ReservationCreatedNotificationHandler(INotificationService notifications)
    : INotificationHandler<ReservationCreatedDomainEvent>
{
    public Task Handle(ReservationCreatedDomainEvent e, CancellationToken cancellationToken) =>
        notifications.NotifyAsync(
            e.BuyerEmail,
            "Reserva recibida",
            $"Hemos registrado tu reserva de {e.Quantity} entrada(s). EstÃ¡ pendiente de pago.",
            cancellationToken);
}
