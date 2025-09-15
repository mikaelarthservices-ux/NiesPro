using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Data;
using Order.Infrastructure.EventStore;
using Order.Domain.Events;
using System.Text.Json;

namespace Order.Tests.Infrastructure.EventStore;

/// <summary>
/// Tests d'intégration pour SqlEventStore
/// </summary>
public sealed class SqlEventStoreTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly SqlEventStore _eventStore;

    public SqlEventStoreTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<OrderDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var serviceProvider = services.BuildServiceProvider();
        _context = serviceProvider.GetRequiredService<OrderDbContext>();
        _eventStore = new SqlEventStore(_context);
    }

    [Fact]
    public async Task SaveEventsAsync_WithNewStream_ShouldSucceed()
    {
        // Arrange
        var streamId = $"order-{Guid.NewGuid()}";
        var events = new List<IDomainEvent>
        {
            new OrderCreatedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "john.doe@example.com",
                100.50m,
                "EUR",
                DateTime.UtcNow
            )
        };

        // Act
        await _eventStore.SaveEventsAsync(streamId, events, -1);

        // Assert
        var savedEvents = await _context.EventStore.ToListAsync();
        savedEvents.Should().HaveCount(1);

        var savedEvent = savedEvents.First();
        savedEvent.StreamId.Should().Be(streamId);
        savedEvent.EventType.Should().Be(nameof(OrderCreatedEvent));
        savedEvent.Version.Should().Be(0);
        savedEvent.EventData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SaveEventsAsync_WithExistingStream_ShouldIncrementVersion()
    {
        // Arrange
        var streamId = $"order-{Guid.NewGuid()}";
        
        var firstEvents = new List<IDomainEvent>
        {
            new OrderCreatedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "john.doe@example.com",
                100.50m,
                "EUR",
                DateTime.UtcNow
            )
        };

        var secondEvents = new List<IDomainEvent>
        {
            new OrderConfirmedEvent(
                Guid.NewGuid(),
                DateTime.UtcNow
            )
        };

        // Act
        await _eventStore.SaveEventsAsync(streamId, firstEvents, -1);
        await _eventStore.SaveEventsAsync(streamId, secondEvents, 0);

        // Assert
        var savedEvents = await _context.EventStore
            .Where(e => e.StreamId == streamId)
            .OrderBy(e => e.Version)
            .ToListAsync();

        savedEvents.Should().HaveCount(2);
        savedEvents[0].Version.Should().Be(0);
        savedEvents[1].Version.Should().Be(1);
    }

    [Fact]
    public async Task SaveEventsAsync_WithWrongExpectedVersion_ShouldThrowConcurrencyException()
    {
        // Arrange
        var streamId = $"order-{Guid.NewGuid()}";
        
        var firstEvents = new List<IDomainEvent>
        {
            new OrderCreatedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "john.doe@example.com",
                100.50m,
                "EUR",
                DateTime.UtcNow
            )
        };

        var secondEvents = new List<IDomainEvent>
        {
            new OrderConfirmedEvent(
                Guid.NewGuid(),
                DateTime.UtcNow
            )
        };

        await _eventStore.SaveEventsAsync(streamId, firstEvents, -1);

        // Act & Assert - Wrong expected version (should be 0, not -1)
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _eventStore.SaveEventsAsync(streamId, secondEvents, -1));
    }

    [Fact]
    public async Task GetEventsAsync_WithExistingStream_ShouldReturnEventsInOrder()
    {
        // Arrange
        var streamId = $"order-{Guid.NewGuid()}";
        var orderId = Guid.NewGuid();
        
        var events = new List<IDomainEvent>
        {
            new OrderCreatedEvent(
                orderId,
                Guid.NewGuid(),
                "john.doe@example.com",
                100.50m,
                "EUR",
                DateTime.UtcNow
            ),
            new OrderConfirmedEvent(
                orderId,
                DateTime.UtcNow
            )
        };

        await _eventStore.SaveEventsAsync(streamId, events, -1);

        // Act
        var retrievedEvents = await _eventStore.GetEventsAsync(streamId);

        // Assert
        retrievedEvents.Should().HaveCount(2);
        
        var eventsList = retrievedEvents.ToList();
        eventsList[0].Should().BeOfType<OrderCreatedEvent>();
        eventsList[1].Should().BeOfType<OrderConfirmedEvent>();

        var createdEvent = (OrderCreatedEvent)eventsList[0];
        createdEvent.OrderId.Should().Be(orderId);

        var confirmedEvent = (OrderConfirmedEvent)eventsList[1];
        confirmedEvent.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetEventsAsync_WithNonExistentStream_ShouldReturnEmpty()
    {
        // Arrange
        var streamId = $"non-existent-{Guid.NewGuid()}";

        // Act
        var events = await _eventStore.GetEventsAsync(streamId);

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveEventsAsync_WithMultipleEventsTypes_ShouldPreserveEventTypes()
    {
        // Arrange
        var streamId = $"order-{Guid.NewGuid()}";
        var orderId = Guid.NewGuid();
        
        var events = new List<IDomainEvent>
        {
            new OrderCreatedEvent(
                orderId,
                Guid.NewGuid(),
                "john.doe@example.com",
                100.50m,
                "EUR",
                DateTime.UtcNow
            ),
            new OrderItemAddedEvent(
                orderId,
                Guid.NewGuid(),
                "Test Product",
                2,
                DateTime.UtcNow
            ),
            new OrderConfirmedEvent(
                orderId,
                DateTime.UtcNow
            )
        };

        // Act
        await _eventStore.SaveEventsAsync(streamId, events, -1);
        var retrievedEvents = await _eventStore.GetEventsAsync(streamId);

        // Assert
        var eventsList = retrievedEvents.ToList();
        eventsList.Should().HaveCount(3);
        
        eventsList[0].Should().BeOfType<OrderCreatedEvent>();
        eventsList[1].Should().BeOfType<OrderItemAddedEvent>();
        eventsList[2].Should().BeOfType<OrderConfirmedEvent>();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}