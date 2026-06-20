using EventosVivos.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = EventosVivos.Application.Common.Exceptions.ValidationException;

namespace EventosVivos.Api.Middleware;

/// <summary>
/// Convierte cualquier excepción no controlada en una respuesta ProblemDetails estandarizada.
/// </summary>
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validación fallida: {Errors}", string.Join("; ", ex.Errors.Keys));
            await WriteValidationProblemAsync(context, ex);
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(ex, "Conflicto de concurrencia.");
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "ConcurrencyConflict", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción no controlada.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "InternalServerError",
                "Ocurrió un error inesperado.");
        }
    }

    private static Task WriteValidationProblemAsync(HttpContext context, ValidationException ex)
    {
        ValidationProblemDetails problem = new(ex.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Error de validación",
            Type = "https://httpstatuses.io/400"
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        ProblemDetails problem = new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.io/{statusCode}"
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problem);
    }
}
