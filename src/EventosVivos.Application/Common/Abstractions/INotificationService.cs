namespace EventosVivos.Application.Common.Abstractions;

public interface INotificationService
{
    Task NotifyAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}
