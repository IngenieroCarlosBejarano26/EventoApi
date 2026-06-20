using MediatR;

namespace EventosVivos.Domain.Common;

/// <summary>
/// Marca un evento de dominio. Hereda de <see cref="INotification"/> (MediatR.Contracts)
/// para poder publicarse mediante IPublisher sin acoplar el dominio a la infraestructura
/// de mensajería. En el futuro puede mapearse a RabbitMQ / Azure Service Bus en el adaptador.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
