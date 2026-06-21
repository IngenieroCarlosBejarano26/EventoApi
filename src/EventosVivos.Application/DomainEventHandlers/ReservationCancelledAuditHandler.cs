using EventosVivos.Application.Common.Abstractions;
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
