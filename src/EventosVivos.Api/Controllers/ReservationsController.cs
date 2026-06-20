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

/// <summary>
/// Endpoints del ciclo de vida de reservas: creación, confirmación de pago y cancelación.
/// </summary>
[Route("api/reservations")]
[Produces("application/json")]
[Tags("Reservations")]
public sealed class ReservationsController(ISender sender) : ApiControllerBase
{
    /// <summary>Reserva entradas para un evento.</summary>
    /// <remarks>
    /// **Requerimiento funcional:** RF-03.
    ///
    /// **Reglas de negocio aplicadas:**
    /// - RN04: no se permiten reservas si faltan menos de 1 hora para el inicio.
    /// - RN05: eventos con precio &gt; $100 limitan a 10 entradas por transacción.
    /// - RF-03: si faltan menos de 24 h, máximo 5 entradas por transacción.
    /// - Anti-sobreventa: el inventario se descuenta al crear la reserva (estado PendientePago).
    ///
    /// **Idempotencia:** incluir el header `X-Idempotency-Key` (UUID recomendado). Reintentos con
    /// la misma clave devuelven la misma respuesta sin crear duplicados.
    ///
    /// **Rate limiting:** política `Reservations` (ver configuración en appsettings).
    ///
    /// **Autenticación:** no requerida.
    /// </remarks>
    /// <param name="command">Evento, cantidad, nombre y email del comprador.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="201">Reserva creada en estado PendientePago.</response>
    /// <response code="400">Validación fallida o reglas RN04/RN05/RF-03 incumplidas.</response>
    /// <response code="404">El evento indicado no existe.</response>
    /// <response code="409">Sin entradas suficientes o conflicto de concurrencia tras reintentos.</response>
    /// <response code="429">Límite de peticiones excedido (rate limiting).</response>
    [HttpPost]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.Reservations)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationCommand command,
        CancellationToken cancellationToken)
    {
        Result<ReservationDto> result = await sender.Send(command, cancellationToken);
        return HandleResult(result, dto => StatusCode(StatusCodes.Status201Created, dto));
    }

    /// <summary>Confirma el pago de una reserva pendiente.</summary>
    /// <remarks>
    /// **Requerimiento funcional:** RF-04.
    ///
    /// Transiciona la reserva de `PendientePago` a `Confirmada` y genera un código único
    /// con formato `EV-{6 dígitos}`.
    ///
    /// **Autenticación:** requiere el header `X-API-KEY`.
    ///
    /// Rechaza reservas ya confirmadas o canceladas. Emite actualización en tiempo real vía SignalR.
    /// </remarks>
    /// <param name="id">Identificador único de la reserva (GUID).</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="200">Pago confirmado. Incluye el código de reserva generado.</response>
    /// <response code="401">API Key ausente o inválida.</response>
    /// <response code="404">La reserva no existe.</response>
    /// <response code="409">Estado incompatible (ya confirmada o cancelada).</response>
    [HttpPost("{id:guid}/confirm")]
    [RequireApiKey]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        Result<ReservationDto> result = await sender.Send(new ConfirmReservationCommand(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Cancela una reserva existente.</summary>
    /// <remarks>
    /// **Requerimiento funcional:** RF-05.
    ///
    /// **Reglas de negocio aplicadas (RN07):**
    /// - Reserva pendiente o confirmada con ≥ 48 h al evento: se cancela y se liberan entradas.
    /// - Reserva confirmada con &lt; 48 h al evento: estado `Perdida`; las entradas no vuelven al inventario.
    ///
    /// **Autenticación:** no requerida.
    /// </remarks>
    /// <param name="id">Identificador único de la reserva (GUID).</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    /// <response code="200">Reserva cancelada (o marcada como Perdida según RN07).</response>
    /// <response code="404">La reserva no existe.</response>
    /// <response code="409">La reserva ya estaba cancelada o en estado incompatible.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        Result<ReservationDto> result = await sender.Send(new CancelReservationCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
