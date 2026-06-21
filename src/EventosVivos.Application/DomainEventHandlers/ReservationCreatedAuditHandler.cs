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
