using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Features.Events.GetEvents;

internal sealed class GetEventsQueryHandler(
    IEventRepository eventRepository,
    ICacheService cache)
    : IQueryHandler<GetEventsQuery, IReadOnlyList<EventDto>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    public async Task<Result<IReadOnlyList<EventDto>>> Handle(GetEventsQuery query, CancellationToken cancellationToken)
    {
        EventFilter filter = new(
            query.Type,
            query.StartDateFrom,
            query.StartDateTo,
            query.VenueId,
            query.Status,
            query.Title);

        string cacheKey = CacheKeys.EventsList(BuildFilterHash(filter));

        IReadOnlyList<EventDto> result = await cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                IReadOnlyList<Event> events = await eventRepository.ListAsync(filter, ct);
                return (IReadOnlyList<EventDto>)events.Select(e => e.ToDto()).ToList();
            },
            CacheTtl,
            cancellationToken);

        return Result.Success(result);
    }

    private static string BuildFilterHash(EventFilter filter) =>
        string.Join(
            '|',
            filter.Type?.ToString() ?? "_",
            filter.StartDateFrom?.ToString("O") ?? "_",
            filter.StartDateTo?.ToString("O") ?? "_",
            filter.VenueId?.ToString() ?? "_",
            filter.Status?.ToString() ?? "_",
            filter.TitleContains?.Trim().ToLowerInvariant() ?? "_");
}
