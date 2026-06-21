using EventosVivos.Application.Features.Events.Shared;

namespace EventosVivos.Application.Common.Abstractions;

public interface IRealtimeNotifier
{
    Task EventUpdatedAsync(EventRealtimeUpdate update, CancellationToken cancellationToken = default);

    Task EventCreatedAsync(EventDto @event, CancellationToken cancellationToken = default);
}
