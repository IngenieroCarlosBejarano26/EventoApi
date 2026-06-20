using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Features.Events.Shared;
using Microsoft.AspNetCore.SignalR;

namespace EventosVivos.Api.RealTime;

/// <summary>Adaptador de salida que implementa el puerto <see cref="IRealtimeNotifier"/> con SignalR.</summary>
public sealed class SignalRNotifier(IHubContext<EventsHub> hub) : IRealtimeNotifier
{
    public Task EventUpdatedAsync(EventRealtimeUpdate update, CancellationToken cancellationToken = default) =>
        hub.Clients.All.SendAsync("EventUpdated", update, cancellationToken);

    public Task EventCreatedAsync(EventDto @event, CancellationToken cancellationToken = default) =>
        hub.Clients.All.SendAsync("EventCreated", @event, cancellationToken);
}
