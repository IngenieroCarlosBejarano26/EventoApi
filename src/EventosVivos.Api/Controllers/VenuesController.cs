using EventosVivos.Api.Common;
using EventosVivos.Application.Features.Venues.GetVenues;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[Route("api/venues")]
[Produces("application/json")]
public sealed class VenuesController(ISender sender) : ApiControllerBase
{
    /// <summary>Lista los venues disponibles (cacheado).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VenueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<VenueDto>> result = await sender.Send(new GetVenuesQuery(), cancellationToken);
        return HandleResult(result);
    }
}
