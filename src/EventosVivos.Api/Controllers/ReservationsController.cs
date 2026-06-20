using EventosVivos.Api.Common;
using EventosVivos.Api.Extensions;
using EventosVivos.Api.Idempotency;
using EventosVivos.Api.Security;
using EventosVivos.Application.Features.Reservations.CancelReservation;
using EventosVivos.Application.Features.Reservations.ConfirmReservation;
using EventosVivos.Application.Features.Reservations.CreateReservation;
using EventosVivos.Application.Features.Reservations.Shared;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventosVivos.Api.Controllers;

[Route("api/reservations")]
[Produces("application/json")]
public sealed class ReservationsController(ISender sender) : ApiControllerBase
{
    /// <summary>RF-03: Crea una reserva. Idempotente vía header X-Idempotency-Key.</summary>
    [HttpPost]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.Reservations)]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
    {
        Result<ReservationDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(result, dto => StatusCode(StatusCodes.Status201Created, dto));
    }

    /// <summary>RF-04: Confirma el pago de una reserva (operación administrativa).</summary>
    [HttpPost("{id:guid}/confirm")]
    [RequireApiKey]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        Result<ReservationDto> result = await sender.Send(new ConfirmReservationCommand(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>RF-05: Cancela una reserva.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        Result<ReservationDto> result = await sender.Send(new CancelReservationCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
