using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Features.Venues.GetVenues;

internal sealed class GetVenuesQueryHandler(
    IVenueRepository venueRepository,
    ICacheService cache)
    : IQueryHandler<GetVenuesQuery, IReadOnlyList<VenueDto>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public async Task<Result<IReadOnlyList<VenueDto>>> Handle(GetVenuesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<VenueDto> venues = await cache.GetOrCreateAsync(
            CacheKeys.VenuesAll,
            async ct =>
            {
                IReadOnlyList<Venue> items = await venueRepository.GetAllAsync(ct);
                return (IReadOnlyList<VenueDto>)items
                    .Select(v => new VenueDto(v.Id, v.Name, v.Capacity, v.City))
                    .ToList();
            },
            CacheTtl,
            cancellationToken);

        return Result.Success(venues);
    }
}
