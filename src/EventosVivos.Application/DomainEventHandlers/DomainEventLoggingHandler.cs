using EventosVivos.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.DomainEventHandlers;

public sealed class DomainEventLoggingHandler<TDomainEvent>(
    ILogger<DomainEventLoggingHandler<TDomainEvent>> logger)
    : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Evento de dominio publicado: {DomainEvent} a las {OccurredOn:o}",
            typeof(TDomainEvent).Name, notification.OccurredOn);
        return Task.CompletedTask;
    }
}
