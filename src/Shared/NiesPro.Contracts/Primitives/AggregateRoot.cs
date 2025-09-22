namespace NiesPro.Contracts.Primitives;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() : base() { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}