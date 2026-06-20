using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(ApplicationDbContext context) : IEventRepository
{
    public Task<Event?> GetByIdAsync(Guid id, bool includeVenue = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Event> query = context.Events.AsQueryable();
        if (includeVenue)
            query = query.Include(e => e.Venue);

        return query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Event> query = context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .AsQueryable();

        if (filter.Type is not null)
            query = query.Where(e => e.Type == filter.Type);

        if (filter.StartDateFrom is not null)
            query = query.Where(e => e.StartDate >= filter.StartDateFrom);

        if (filter.StartDateTo is not null)
            query = query.Where(e => e.StartDate <= filter.StartDateTo);

        if (filter.VenueId is not null)
            query = query.Where(e => e.VenueId == filter.VenueId);

        if (filter.Status is not null)
            query = query.Where(e => e.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.TitleContains))
        {
            // ILIKE de PostgreSQL realiza la búsqueda parcial case-insensitive (RF-02).
            string term = filter.TitleContains.Trim();
            query = query.Where(e => EF.Functions.ILike(e.Title, $"%{term}%"));
        }

        return await query.OrderBy(e => e.StartDate).ToListAsync(cancellationToken);
    }

    public Task<bool> HasOverlappingActiveEventAsync(
        int venueId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Guid? excludeEventId = null,
        CancellationToken cancellationToken = default) =>
        context.Events.AnyAsync(
            e => e.VenueId == venueId
                 && e.Status == EventStatus.Activo
                 && (excludeEventId == null || e.Id != excludeEventId)
                 && startDate < e.EndDate
                 && e.StartDate < endDate,
            cancellationToken);

    public async Task<IReadOnlyList<Event>> GetActiveFinishedEventsAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default) =>
        await context.Events
            .Where(e => e.Status == EventStatus.Activo && e.EndDate < now)
            .ToListAsync(cancellationToken);

    public void Add(Event @event) => context.Events.Add(@event);
}
