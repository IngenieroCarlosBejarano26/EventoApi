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
