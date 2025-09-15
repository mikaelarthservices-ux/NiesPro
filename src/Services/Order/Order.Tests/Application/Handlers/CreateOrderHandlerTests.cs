using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Domain.Entities;
using Order.Domain.ValueObjects;
using Order.Domain.Repositories;
using Order.Infrastructure.EventStore;

namespace Order.Tests.Application.Handlers;

/// <summary>
/// Tests unitaires pour CreateOrderCommandHandler
/// </summary>
public sealed class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IEventStore> _eventStoreMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _eventStoreMock = new Mock<IEventStore>();
        var validatorMock = new Mock<FluentValidation.IValidator<CreateOrderCommand>>();
        
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _handler = new CreateOrderCommandHandler(_orderRepositoryMock.Object, _eventStoreMock.Object, validatorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var orderDto = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "john.doe@example.com",
            CustomerFirstName = "John",
            CustomerLastName = "Doe",
            CustomerPhone = "+33123456789",
            DeliveryAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "Paris", 
                PostalCode = "75001",
                Country = "France"
            },
            Items = new List<OrderItemDto>
            {
                new() 
                { 
                    ProductId = Guid.NewGuid(), 
                    ProductName = "Test Product", 
                    UnitPrice = 25.99m, 
                    Currency = "EUR", 
                    Quantity = 2 
                }
            }
        };

        var command = new CreateOrderCommand(orderDto);

        _orderRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventStoreMock
            .Setup(x => x.SaveEventsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _orderRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<Order.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), 
            Times.Once);

        _eventStoreMock.Verify(
            x => x.SaveEventsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<int>()), 
            Times.Once);
    }
}