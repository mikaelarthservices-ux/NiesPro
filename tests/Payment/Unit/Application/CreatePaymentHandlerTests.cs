using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Payment.Application.Commands;
using Payment.Application.DTOs;
using Payment.Application.Handlers;
using Payment.Application.Services;
using Payment.Domain.Interfaces;
using Payment.Domain.Enums;
using MediatR;
using AutoMapper;

namespace Payment.Tests.Unit.Application;

/// <summary>
/// Tests unitaires pour CreatePaymentHandler - NiesPro Enterprise Standards
/// </summary>
[TestFixture]
public class CreatePaymentHandlerTests
{
    private Mock<IPaymentRepository> _mockPaymentRepository = null!;
    private Mock<IOrderService> _mockOrderService = null!;
    private Mock<IFraudDetectionService> _mockFraudDetectionService = null!;
    private Mock<IPaymentProcessorFactory> _mockPaymentProcessorFactory = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IMediator> _mockMediator = null!;
    private Mock<ILogger<CreatePaymentHandler>> _mockLogger = null!;
    
    private CreatePaymentHandler _handler = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockOrderService = new Mock<IOrderService>();
        _mockFraudDetectionService = new Mock<IFraudDetectionService>();
        _mockPaymentProcessorFactory = new Mock<IPaymentProcessorFactory>();
        _mockMapper = new Mock<IMapper>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CreatePaymentHandler>>();
        
        _handler = new CreatePaymentHandler(
            _mockPaymentRepository.Object,
            _mockOrderService.Object,
            _mockFraudDetectionService.Object,
            _mockPaymentProcessorFactory.Object,
            _mockMapper.Object,
            _mockMediator.Object,
            _mockLogger.Object);
        
        _fixture = new Fixture();
    }

    [Test]
    public async Task Handle_WithValidRequest_ShouldCreatePaymentSuccessfully()
    {
        // Arrange
        var command = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.OrderId, Guid.NewGuid())
            .With(x => x.Amount, 100.50m)
            .With(x => x.Currency, "EUR")
            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.PaymentId.Should().NotBeEmpty();
        result.PaymentNumber.Should().StartWith("PAY_");
        result.Status.Should().Be(PaymentStatus.Created);
        result.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be(command.Currency);
    }

    [Test]
    public async Task Handle_WithValidRequest_ShouldLogInformation()
    {
        // Arrange
        var command = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.OrderId, Guid.NewGuid())
            .With(x => x.Amount, 75.25m)
            .With(x => x.Currency, "USD")
            .Create();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating payment for order")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_WithZeroAmount_ShouldCreatePaymentWithCorrectAmount()
    {
        // Arrange
        var command = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.OrderId, Guid.NewGuid())
            .With(x => x.Amount, 0m)
            .With(x => x.Currency, "EUR")
            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Amount.Should().Be(0m);
    }

    [Test]
    public async Task Handle_WithLargeAmount_ShouldCreatePaymentSuccessfully()
    {
        // Arrange
        var command = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.OrderId, Guid.NewGuid())
            .With(x => x.Amount, 99999.99m)
            .With(x => x.Currency, "EUR")
            .Create();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Amount.Should().Be(99999.99m);
    }

    [Test]
    public async Task Handle_WithDifferentCurrencies_ShouldPreserveCurrency()
    {
        // Arrange
        var currencies = new[] { "USD", "EUR", "GBP", "JPY" };
        
        foreach (var currency in currencies)
        {
            var command = _fixture.Build<CreatePaymentCommand>()
                .With(x => x.OrderId, Guid.NewGuid())
                .With(x => x.Amount, 100m)
                .With(x => x.Currency, currency)
                .Create();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Currency.Should().Be(currency);
        }
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var command = _fixture.Create<CreatePaymentCommand>();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.Handle(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }
}