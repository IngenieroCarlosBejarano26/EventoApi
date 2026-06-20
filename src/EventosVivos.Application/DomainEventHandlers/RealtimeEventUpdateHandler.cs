using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Events;
using MediatR;

namespace EventosVivos.Application.DomainEventHandlers;

public sealed class RealtimeEventUpdateHandler(
    IEventRepository events,
    IRealtimeNotifier realtime)
    : INotificationHandler<ReservationCreatedDomainEvent>,
      INotificationHandler<ReservationConfirmedDomainEvent>,
      INotificationHandler<ReservationCancelledDomainEvent>,
      INotificationHandler<EventCompletedDomainEvent>
{
    public Task Handle(ReservationCreatedDomainEvent e, CancellationToken cancellationToken) =>
        NotifyAsync(e.EventId, cancellationToken);

    public Task Handle(ReservationConfirmedDomainEvent e, CancellationToken cancellationToken) =>
        NotifyAsync(e.EventId, cancellationToken);

    public Task Handle(ReservationCancelledDomainEvent e, CancellationToken cancellationToken) =>
        NotifyAsync(e.EventId, cancellationToken);

    public Task Handle(EventCompletedDomainEvent e, CancellationToken cancellationToken) =>
        NotifyAsync(e.EventId, cancellationToken);

    private async Task NotifyAsync(Guid eventId, CancellationToken cancellationToken)
    {
        Event? @event = await events.GetByIdAsync(eventId, includeVenue: false, cancellationToken);
        if (@event is null)
            return;

        await realtime.EventUpdatedAsync(
            new EventRealtimeUpdate(
                @event.Id,
                @event.AvailableTickets,
                @event.MaxCapacity,
                @event.SoldTickets,
                @event.Status.ToString()),
            cancellationToken);
    }
}
