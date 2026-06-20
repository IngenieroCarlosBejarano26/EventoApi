using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Features.Events.GetOccupancyReport;

/// <summary>RF-06: Reporte de ocupación por evento.</summary>
public sealed record OccupancyReportResponse(
    Guid EventId,
    string EventTitle,
    int MaxCapacity,
    int SoldTickets,
    int AvailableTickets,
    decimal OccupancyPercentage,
    decimal TotalRevenue,
    EventStatus Status);
