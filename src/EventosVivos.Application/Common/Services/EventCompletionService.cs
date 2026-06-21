using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Services;

internal sealed class EventCompletionService(
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cache) : IEventCompletionService
{
    public async Task CompleteFinishedEventsAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = dateTimeProvider.UtcNow;
        IReadOnlyList<Event> finishedEvents = await eventRepository.GetActiveFinishedEventsAsync(now, cancellationToken);

        if (finishedEvents.Count == 0)
            return;

        foreach (Event @event in finishedEvents)
            @event.MarkAsCompletedIfFinished(now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        cache.RemoveByPrefix(CacheKeys.EventsPrefix);
        cache.RemoveByPrefix(CacheKeys.ReportsPrefix);
    }
}
