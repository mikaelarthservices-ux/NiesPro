using MediatR;
using NiesPro.Contracts.Primitives;

namespace Customer.Application.Interfaces
{
    /// <summary>
    /// Simple event bus interface for publishing domain events
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publish a domain event
        /// </summary>
        Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent;

        /// <summary>
        /// Publish multiple domain events
        /// </summary>
        Task PublishAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : IDomainEvent;
    }

    /// <summary>
    /// Simple in-memory event bus implementation using MediatR
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly IMediator _mediator;

        public EventBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        public async Task PublishAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}