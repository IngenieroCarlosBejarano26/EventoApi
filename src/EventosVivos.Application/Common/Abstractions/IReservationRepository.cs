using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Common.Abstractions;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, bool includeEvent = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> ListByEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    void Add(Reservation reservation);
}
