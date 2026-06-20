using EventosVivos.Application.Common.Messaging;

namespace EventosVivos.Application.Features.Events.GetOccupancyReport;

public sealed record GetOccupancyReportQuery(Guid EventId) : IQuery<OccupancyReportResponse>;
