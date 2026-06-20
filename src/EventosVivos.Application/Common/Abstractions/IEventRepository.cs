using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Abstractions;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, bool includeVenue = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingActiveEventAsync(
        int venueId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Guid? excludeEventId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> GetActiveFinishedEventsAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    void Add(Event @event);
}
