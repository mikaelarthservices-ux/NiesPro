using NiesPro.Contracts.Primitives;

namespace Order.Infrastructure.EventStore;

public sealed class StoredEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public long Version { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
}

public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, string aggregateType, IEnumerable<IDomainEvent> events, long expectedVersion, CancellationToken cancellationToken = default);
    Task<IEnumerable<StoredEvent>> GetEventsAsync(Guid aggregateId, long fromVersion = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(string eventType, DateTime? fromDate = null, CancellationToken cancellationToken = default);
    Task<long> GetCurrentVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
}