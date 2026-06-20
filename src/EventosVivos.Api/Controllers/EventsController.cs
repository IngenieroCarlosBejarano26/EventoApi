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

/// <summary>
/// Endpoints de gestión de eventos: alta, consulta filtrada y métricas de ocupación.
/// </summary>
[Route("api/events")]
[Produces("application/json")]
[Tags("Events")]
public sealed class EventsController(ISender sender) : ApiControllerBase
{
    /// <summary>Crea un evento cultural asociado a un venue existente.</summary>
    /// <remarks>
    /// **Requerimiento funcional:** RF-01.
    ///
    /// **Reglas de negocio aplicadas:**
    /// - RN01: la capacidad máxima no puede superar la del venue.
    /// - RN02: no puede solaparse con otro evento activo en el mismo venue.
    /// - RN03: en fines de semana el inicio no puede ser posterior a las 22:00.
    ///
    /// **Autenticación:** requiere el header `X-API-KEY`.
    ///
    /// Tras la creación, los clientes conectados vía SignalR reciben el evento `EventCreated`.
    /// </remarks>
    /// <param name="command">Datos del evento: título, descripción, venue, capacidad, fechas, precio y tipo.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="201">Evento creado. Devuelve el DTO con identificador y entradas disponibles.</response>
    /// <response code="400">Error de validación (campos obligatorios, fechas incoherentes, reglas RN01/RN03).</response>
    /// <response code="401">API Key ausente o inválida.</response>
    /// <response code="409">Conflicto de negocio (solapamiento de venue — RN02).</response>
    [HttpPost]
    [RequireApiKey]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        Result<EventDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(result, dto => CreatedAtAction(nameof(GetOccupancyReport), new { id = dto.Id }, dto));
    }

    /// <summary>Lista eventos con filtros opcionales.</summary>
    /// <remarks>
    /// **Requerimiento funcional:** RF-02.
    ///
    /// Todos los filtros son opcionales y combinables. La búsqueda por título es parcial e
    /// insensible a mayúsculas/minúsculas.
    ///
    /// **Autenticación:** no requerida.
    ///
    /// La respuesta puede estar cacheada en memoria; se invalida ante cambios en eventos o reservas.
    /// </remarks>
    /// <param name="type">Filtra por tipo: Conferencia, Taller o Concierto.</param>
    /// <param name="startDateFrom">Inicio mínimo del rango de fechas (inclusive).</param>
    /// <param name="startDateTo">Inicio máximo del rango de fechas (inclusive).</param>
    /// <param name="venueId">Identificador del venue (1–3 en datos de referencia).</param>
    /// <param name="status">Estado del evento: Activo, Cancelado o Completado.</param>
    /// <param name="title">Búsqueda parcial por título.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="200">Listado de eventos que cumplen los filtros (puede ser vacío).</response>
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

    /// <summary>Obtiene el reporte de ocupación e ingresos de un evento.</summary>
    /// <remarks>
    /// **Requerimiento funcional:** RF-06.
    ///
    /// **Métricas incluidas:**
    /// - Entradas vendidas (confirmadas + perdidas por RN07).
    /// - Entradas disponibles restantes.
    /// - Porcentaje de ocupación sobre la capacidad máxima.
    /// - Ingresos totales (precio × entradas vendidas).
    /// - Estado actual del evento.
    ///
    /// **Autenticación:** no requerida.
    /// </remarks>
    /// <param name="id">Identificador único del evento (GUID).</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="200">Reporte de ocupación del evento.</response>
    /// <response code="404">No existe un evento con el identificador indicado.</response>
    [HttpGet("{id:guid}/occupancy-report")]
    [ProducesResponseType(typeof(OccupancyReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOccupancyReport(Guid id, CancellationToken cancellationToken)
    {
        Result<OccupancyReportResponse> result = await sender.Send(new GetOccupancyReportQuery(id), cancellationToken);
        return HandleResult(result);
    }
}
