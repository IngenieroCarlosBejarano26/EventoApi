using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Events.GetOccupancyReport;

public sealed record OccupancyReportResponse(
    Guid EventId,
    string EventTitle,
    int MaxCapacity,
    int SoldTickets,
    int AvailableTickets,
    decimal OccupancyPercentage,
    decimal TotalRevenue,
    EventStatus Status);
