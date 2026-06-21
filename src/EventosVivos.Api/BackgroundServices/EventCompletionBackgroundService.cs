using EventosVivos.Application.Common.Abstractions;

namespace EventosVivos.Api.BackgroundServices;

public sealed class EventCompletionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<EventCompletionBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(Interval);

        do
        {
            try
            {
                await CompleteFinishedEventsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error al completar eventos finalizados.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CompleteFinishedEventsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IEventCompletionService completionService =
            scope.ServiceProvider.GetRequiredService<IEventCompletionService>();

        await completionService.CompleteFinishedEventsAsync(cancellationToken);
    }
}
