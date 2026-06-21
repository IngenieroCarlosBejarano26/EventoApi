using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Application.Common.Messaging;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Errors;

namespace EventosVivos.Application.Features.Events.GetOccupancyReport;

internal sealed class GetOccupancyReportQueryHandler(
    IEventRepository eventRepository,
    IReservationRepository reservationRepository,
    IEventCompletionService eventCompletionService,
    ICacheService cache)
    : IQueryHandler<GetOccupancyReportQuery, OccupancyReportResponse>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1);

    public async Task<Result<OccupancyReportResponse>> Handle(
        GetOccupancyReportQuery query,
        CancellationToken cancellationToken)
    {
        await eventCompletionService.CompleteFinishedEventsAsync(cancellationToken);

        Event? @event = await eventRepository.GetByIdAsync(query.EventId, includeVenue: false, cancellationToken);
        if (@event is null)
            return DomainErrors.Event.NotFound(query.EventId);

        OccupancyReportResponse report = await cache.GetOrCreateAsync(
            CacheKeys.OccupancyReport(query.EventId),
            async ct =>
            {
                IReadOnlyList<Reservation> reservations = await reservationRepository.ListByEventAsync(query.EventId, ct);

                int soldTickets = reservations
                    .Where(r => r.Status is ReservationStatus.Confirmada or ReservationStatus.Perdida)
                    .Sum(r => r.Quantity);

                decimal occupancy = @event.MaxCapacity == 0
                    ? 0m
                    : Math.Round((decimal)soldTickets / @event.MaxCapacity * 100m, 2);

                return new OccupancyReportResponse(
                    @event.Id,
                    @event.Title,
                    @event.MaxCapacity,
                    soldTickets,
                    @event.AvailableTickets,
                    occupancy,
                    soldTickets * @event.Price,
                    @event.Status);
            },
            CacheTtl,
            cancellationToken);

        return Result.Success(report);
    }
}
