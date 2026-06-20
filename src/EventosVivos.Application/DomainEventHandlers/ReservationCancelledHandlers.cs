using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Events;
using MediatR;

namespace EventosVivos.Application.DomainEventHandlers;

public sealed class ReservationCancelledAuditHandler(IAuditService audit)
    : INotificationHandler<ReservationCancelledDomainEvent>
{
    public Task Handle(ReservationCancelledDomainEvent e, CancellationToken cancellationToken) =>
        audit.RecordAsync(
            "ReservationCancelled",
            $"Reserva {e.ReservationId} finalizó como {e.FinalStatus}. Entradas liberadas: {e.TicketsReleased}.",
            cancellationToken);
}

public sealed class ReservationCancelledNotificationHandler(INotificationService notifications)
    : INotificationHandler<ReservationCancelledDomainEvent>
{
    public Task Handle(ReservationCancelledDomainEvent e, CancellationToken cancellationToken)
    {
        string body = e.FinalStatus == ReservationStatus.Perdida
            ? "Tu reserva fue cancelada dentro de la ventana de penalización (menos de 48h)."
            : "Tu reserva fue cancelada y las entradas fueron liberadas.";

        return notifications.NotifyAsync(string.Empty, "Reserva cancelada", body, cancellationToken);
    }
}
