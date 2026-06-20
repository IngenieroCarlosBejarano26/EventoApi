using EventosVivos.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Traduce un Result&lt;T&gt; a la respuesta HTTP adecuada (éxito o ProblemDetails).</summary>
    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);

    protected IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess
            ? onSuccess(result.Value)
            : Problem(result.Error);

    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess
            ? NoContent()
            : Problem(result.Error);

    private ObjectResult Problem(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description,
            Type = $"https://httpstatuses.io/{statusCode}"
        };
        problemDetails.Extensions["errorCode"] = error.Code;

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
