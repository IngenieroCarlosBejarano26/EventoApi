using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class VenueRepository(ApplicationDbContext context) : IVenueRepository
{
    public Task<Venue?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Venues.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Venues.AsNoTracking().OrderBy(v => v.Name).ToListAsync(cancellationToken);
}
