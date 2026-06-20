using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository(ApplicationDbContext context) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(Guid id, bool includeEvent = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Reservation> query = context.Reservations.AsQueryable();
        if (includeEvent)
            query = query.Include(r => r.Event);

        return query.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> ListByEventAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        await context.Reservations
            .AsNoTracking()
            .Where(r => r.EventId == eventId)
            .ToListAsync(cancellationToken);

    public void Add(Reservation reservation) => context.Reservations.Add(reservation);
}
