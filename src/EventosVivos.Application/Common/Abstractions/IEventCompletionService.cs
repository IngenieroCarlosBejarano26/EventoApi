namespace EventosVivos.Application.Common.Abstractions;

public interface IEventCompletionService
{
    Task CompleteFinishedEventsAsync(CancellationToken cancellationToken = default);
}
