namespace EventosVivos.Domain.Common;

/// <summary>
/// Raíz de agregado / entidad base. Encapsula la colección de eventos de dominio
/// para que las entidades expresen "qué ocurrió" sin conocer cómo se despachan.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
