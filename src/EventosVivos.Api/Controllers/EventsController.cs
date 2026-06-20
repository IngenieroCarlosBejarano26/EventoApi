using EventosVivos.Api.Common;
using EventosVivos.Api.Security;
using EventosVivos.Application.Features.Events.CreateEvent;
using EventosVivos.Application.Features.Events.GetEvents;
using EventosVivos.Application.Features.Events.GetOccupancyReport;
using EventosVivos.Application.Features.Events.Shared;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[Route("api/events")]
[Produces("application/json")]
public sealed class EventsController(ISender sender) : ApiControllerBase
{
    /// <summary>RF-01: Crea un evento (operación administrativa).</summary>
    [HttpPost]
    [RequireApiKey]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        Result<EventDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(result, dto => CreatedAtAction(nameof(GetOccupancyReport), new { id = dto.Id }, dto));
    }

    /// <summary>RF-02: Lista eventos con filtros opcionales.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] EventType? type,
        [FromQuery] DateTimeOffset? startDateFrom,
        [FromQuery] DateTimeOffset? startDateTo,
        [FromQuery] int? venueId,
        [FromQuery] EventStatus? status,
        [FromQuery] string? title,
        CancellationToken cancellationToken)
    {
        GetEventsQuery query = new(type, startDateFrom, startDateTo, venueId, status, title);
        Result<IReadOnlyList<EventDto>> result = await sender.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>RF-06: Reporte de ocupación de un evento.</summary>
    [HttpGet("{id:guid}/occupancy-report")]
    [ProducesResponseType(typeof(OccupancyReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOccupancyReport(Guid id, CancellationToken cancellationToken)
    {
        Result<OccupancyReportResponse> result = await sender.Send(new GetOccupancyReportQuery(id), cancellationToken);
        return HandleResult(result);
    }
}
