using MediatR;

namespace NiesPro.Contracts.Primitives;

public interface IDomainEvent : INotification
{
    Guid Id => Guid.NewGuid();
    DateTime OccurredAt => DateTime.UtcNow;
}