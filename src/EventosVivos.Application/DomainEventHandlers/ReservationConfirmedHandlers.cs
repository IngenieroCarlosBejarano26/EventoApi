using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Events;
using MediatR;

namespace EventosVivos.Application.DomainEventHandlers;

public sealed class ReservationConfirmedAuditHandler(IAuditService audit)
    : INotificationHandler<ReservationConfirmedDomainEvent>
{
    public Task Handle(ReservationConfirmedDomainEvent e, CancellationToken cancellationToken) =>
        audit.RecordAsync(
            "ReservationConfirmed",
            $"Reserva {e.ReservationId} confirmada con código {e.ReservationCode}.",
            cancellationToken);
}

public sealed class ReservationConfirmedNotificationHandler(INotificationService notifications)
    : INotificationHandler<ReservationConfirmedDomainEvent>
{
    public Task Handle(ReservationConfirmedDomainEvent e, CancellationToken cancellationToken) =>
        notifications.NotifyAsync(
            e.BuyerEmail,
            "Pago confirmado",
            $"Tu reserva fue confirmada. Tu código de acceso es {e.ReservationCode}.",
            cancellationToken);
}
