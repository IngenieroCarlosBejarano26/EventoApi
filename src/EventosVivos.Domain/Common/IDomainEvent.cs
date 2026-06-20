using MediatR;

namespace EventosVivos.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
