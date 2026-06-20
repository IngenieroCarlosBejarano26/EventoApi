using System.Diagnostics;
using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.Common.Behaviors;

/// <summary>
/// Logging estructurado de cada request: inicio, resultado (éxito/fallo) y duración.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        logger.LogInformation("Procesando {RequestName}", requestName);

        Stopwatch stopwatch = Stopwatch.StartNew();
        TResponse response = await next();
        stopwatch.Stop();

        if (response.IsSuccess)
        {
            logger.LogInformation(
                "{RequestName} completado con éxito en {ElapsedMs} ms",
                requestName, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogWarning(
                "{RequestName} falló con error {ErrorCode}: {ErrorDescription} ({ElapsedMs} ms)",
                requestName, response.Error.Code, response.Error.Description, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
