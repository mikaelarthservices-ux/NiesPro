using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Data;
using NiesPro.Contracts.Primitives;
using Newtonsoft.Json;
using System.Text;

namespace Order.Infrastructure.EventStore;

public sealed class SqlEventStore : IEventStore
{
    private readonly OrderDbContext _context;

    public SqlEventStore(OrderDbContext context)
    {
        _context = context;
    }

    public async Task SaveEventsAsync(
        Guid aggregateId, 
        string aggregateType, 
        IEnumerable<IDomainEvent> events, 
        long expectedVersion, 
        CancellationToken cancellationToken = default)
    {
        // Vérifier la version pour concurrence optimiste
        var currentVersion = await GetCurrentVersionAsync(aggregateId, cancellationToken);
        if (currentVersion != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Concurrency conflict for aggregate {aggregateId}. Expected version: {expectedVersion}, Current version: {currentVersion}");
        }

        var storedEvents = new List<StoredEvent>();
        var version = expectedVersion;

        foreach (var domainEvent in events)
        {
            version++;
            
            var eventData = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });

            var metadata = JsonConvert.SerializeObject(new
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CausationId = domainEvent.Id.ToString(),
                Timestamp = domainEvent.OccurredAt,
                EventVersion = "1.0"
            });

            var storedEvent = new StoredEvent
            {
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventType = domainEvent.GetType().Name,
                EventData = eventData,
                Metadata = metadata,
                Version = version,
                Timestamp = domainEvent.OccurredAt
            };

            storedEvents.Add(storedEvent);
        }

        await _context.Events.AddRangeAsync(storedEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsAsync(
        Guid aggregateId, 
        long fromVersion = 0, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(
        string eventType, 
        DateTime? fromDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events.Where(e => e.EventType == eventType);
        
        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        return await query
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCurrentVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var latestEvent = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return latestEvent?.Version ?? 0;
    }

    public async Task<bool> ExistsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(e => e.AggregateId == aggregateId, cancellationToken);
    }
}