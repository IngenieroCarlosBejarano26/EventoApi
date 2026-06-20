using EventosVivos.Application.Common.Abstractions;
using EventosVivos.Domain.Entities;
using EventosVivos.Infrastructure.Persistence;

namespace EventosVivos.Api.BackgroundServices;

public sealed class EventCompletionBackgroundService(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<EventCompletionBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

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
        IEventRepository eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        ICacheService cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

        DateTimeOffset now = dateTimeProvider.UtcNow;
        IReadOnlyList<Event> finishedEvents = await eventRepository.GetActiveFinishedEventsAsync(now, cancellationToken);

        if (finishedEvents.Count == 0)
            return;

        foreach (Event @event in finishedEvents)
            @event.MarkAsCompletedIfFinished(now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.RemoveByPrefix(CacheKeys.EventsPrefix);
        cache.RemoveByPrefix(CacheKeys.ReportsPrefix);

        logger.LogInformation("{Count} evento(s) marcados como Completados.", finishedEvents.Count);
    }
}
