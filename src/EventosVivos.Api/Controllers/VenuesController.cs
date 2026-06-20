using EventosVivos.Api.Common;
using EventosVivos.Application.Features.Venues.GetVenues;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

/// <summary>
/// Catálogo de venues (sedes) disponibles para asignar a eventos.
/// </summary>
[Route("api/venues")]
[Produces("application/json")]
[Tags("Venues")]
public sealed class VenuesController(ISender sender) : ApiControllerBase
{
    /// <summary>Lista todos los venues precargados en el sistema.</summary>
    /// <remarks>
    /// Devuelve el catálogo de referencia (Auditorio Central, Sala Norte, Arena Sur) con
    /// capacidad y ciudad. La respuesta está cacheada en memoria.
    ///
    /// **Autenticación:** no requerida.
    ///
    /// Útil para poblar selectores en el frontend al crear eventos (RF-01).
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="200">Listado de venues disponibles.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VenueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<VenueDto>> result = await sender.Send(new GetVenuesQuery(), cancellationToken);
        return HandleResult(result);
    }
}
