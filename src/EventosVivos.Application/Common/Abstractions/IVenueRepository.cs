using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Abstractions;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken = default);
}
