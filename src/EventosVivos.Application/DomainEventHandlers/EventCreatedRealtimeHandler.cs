using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Events;
using MediatR;

namespace EventosVivos.Application.DomainEventHandlers;

/// <summary>Difunde en tiempo real un evento recién creado para que aparezca en vivo en los listados.</summary>
public sealed class EventCreatedRealtimeHandler(
    IEventRepository events,
    IRealtimeNotifier realtime)
    : INotificationHandler<EventCreatedDomainEvent>
{
    public async Task Handle(EventCreatedDomainEvent e, CancellationToken cancellationToken)
    {
        Event? @event = await events.GetByIdAsync(e.EventId, includeVenue: true, cancellationToken);
        if (@event is null)
            return;

        await realtime.EventCreatedAsync(@event.ToDto(), cancellationToken);
    }
}
