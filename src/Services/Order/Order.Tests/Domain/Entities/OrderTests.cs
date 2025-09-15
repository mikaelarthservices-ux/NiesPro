using Order.Domain.Entities;
using Order.Domain.ValueObjects;
using Order.Domain.Enums;
using Order.Domain.Events;

namespace Order.Tests.Domain.Entities;

/// <summary>
/// Tests unitaires pour l'entité Order (Aggregate Root)
/// </summary>
public sealed class OrderTests
{
    private readonly CustomerInfo _customerInfo;
    private readonly Address _deliveryAddress;

    public OrderTests()
    {
        _customerInfo = new CustomerInfo(
            Guid.NewGuid(),
            "john.doe@example.com",
            "John",
            "Doe",
            "+33123456789"
        );

        _deliveryAddress = new Address(
            "123 Main Street",
            "Apt 4B",
            "Paris",
            "75001",
            "France"
        );
    }

    [Fact]
    public void Order_Creation_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        // Assert
        order.Id.Should().NotBeNull();
        order.Customer.Should().Be(_customerInfo);
        order.DeliveryAddress.Should().Be(_deliveryAddress);
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(Money.Zero("EUR"));
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_Creation_ShouldRaise_OrderCreatedEvent()
    {
        // Arrange & Act
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        // Assert
        var domainEvents = order.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<OrderCreatedEvent>();

        var orderCreatedEvent = (OrderCreatedEvent)domainEvents.First();
        orderCreatedEvent.OrderId.Should().Be(order.Id.Value);
        orderCreatedEvent.CustomerId.Should().Be(_customerInfo.Id);
        orderCreatedEvent.CustomerEmail.Should().Be(_customerInfo.Email);
    }

    [Fact]
    public void Order_AddItem_WithValidData_ShouldSucceed()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        var productId = Guid.NewGuid();
        var unitPrice = new Money(25.99m, "EUR");

        // Act
        order.AddItem(productId, "Test Product", unitPrice, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        
        var item = order.Items.First();
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Test Product");
        item.UnitPrice.Should().Be(unitPrice);
        item.Quantity.Should().Be(2);
        item.TotalPrice.Should().Be(new Money(51.98m, "EUR"));
        
        order.TotalAmount.Should().Be(new Money(51.98m, "EUR"));
    }

    [Fact]
    public void Order_AddItem_ShouldRaise_OrderItemAddedEvent()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );
        
        order.ClearDomainEvents(); // Clear creation event

        var productId = Guid.NewGuid();
        var unitPrice = new Money(25.99m, "EUR");

        // Act
        order.AddItem(productId, "Test Product", unitPrice, 2);

        // Assert
        var domainEvents = order.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<OrderItemAddedEvent>();

        var itemAddedEvent = (OrderItemAddedEvent)domainEvents.First();
        itemAddedEvent.OrderId.Should().Be(order.Id.Value);
        itemAddedEvent.ProductId.Should().Be(productId);
        itemAddedEvent.Quantity.Should().Be(2);
    }

    [Fact]
    public void Order_ConfirmOrder_WithValidState_ShouldSucceed()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        order.AddItem(Guid.NewGuid(), "Test Product", new Money(25.99m, "EUR"), 1);
        order.ClearDomainEvents(); // Clear previous events

        // Act
        order.ConfirmOrder();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var domainEvents = order.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<OrderConfirmedEvent>();
    }

    [Fact]
    public void Order_ConfirmOrder_WithoutItems_ShouldThrowException()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ConfirmOrder());
    }

    [Fact]
    public void Order_ConfirmOrder_WhenAlreadyConfirmed_ShouldThrowException()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        order.AddItem(Guid.NewGuid(), "Test Product", new Money(25.99m, "EUR"), 1);
        order.ConfirmOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ConfirmOrder());
    }

    [Fact]
    public void Order_CancelOrder_WithValidState_ShouldSucceed()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        order.AddItem(Guid.NewGuid(), "Test Product", new Money(25.99m, "EUR"), 1);
        order.ClearDomainEvents(); // Clear previous events

        // Act
        var reason = "Customer requested cancellation";
        order.CancelOrder(reason);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var domainEvents = order.GetDomainEvents();
        domainEvents.Should().HaveCount(1);
        domainEvents.First().Should().BeOfType<OrderCancelledEvent>();

        var cancelledEvent = (OrderCancelledEvent)domainEvents.First();
        cancelledEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void Order_CancelOrder_WhenShipped_ShouldThrowException()
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        order.AddItem(Guid.NewGuid(), "Test Product", new Money(25.99m, "EUR"), 1);
        order.ConfirmOrder();
        order.ShipOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            order.CancelOrder("Cannot cancel shipped order"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Order_AddItem_WithInvalidQuantity_ShouldThrowException(int quantity)
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            order.AddItem(Guid.NewGuid(), "Test Product", new Money(25.99m, "EUR"), quantity));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Order_AddItem_WithInvalidProductName_ShouldThrowException(string productName)
    {
        // Arrange
        var order = new Order.Domain.Entities.Order(
            OrderId.Create(),
            _customerInfo,
            _deliveryAddress
        );

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            order.AddItem(Guid.NewGuid(), productName, new Money(25.99m, "EUR"), 1));
    }
}