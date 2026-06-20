using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Abstractions;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, bool includeVenue = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken cancellationToken = default);

    /// <summary>RN02: detecta si el venue tiene un evento activo con horario superpuesto.</summary>
    Task<bool> HasOverlappingActiveEventAsync(
        int venueId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Guid? excludeEventId = null,
        CancellationToken cancellationToken = default);

    /// <summary>RN06: eventos activos cuya fecha de fin ya pasó (para el background service).</summary>
    Task<IReadOnlyList<Event>> GetActiveFinishedEventsAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    void Add(Event @event);
}
