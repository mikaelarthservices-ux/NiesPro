using NUnit.Framework;
using FluentAssertions;
using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Payment.Application.Handlers;
using Payment.Application.Commands;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.Interfaces;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;
using MediatR;

namespace Payment.Tests.Unit.Application;

/// <summary>
/// Unit tests for CreatePaymentCommandHandler following NiesPro Enterprise standards
/// </summary>
[TestFixture]
public class CreatePaymentCommandHandlerEnterpriseTests
{
    private IFixture _fixture;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private Mock<IOrderService> _mockOrderService;
    private Mock<IFraudDetectionService> _mockFraudDetectionService;
    private Mock<IPaymentProcessorFactory> _mockPaymentProcessorFactory;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogsServiceClient> _mockLogsService;
    private Mock<IAuditServiceClient> _mockAuditService;
    private Mock<IMediator> _mockMediator;
    private Mock<ILogger<CreatePaymentCommandHandler>> _mockLogger;
    private CreatePaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockOrderService = new Mock<IOrderService>();
        _mockFraudDetectionService = new Mock<IFraudDetectionService>();
        _mockPaymentProcessorFactory = new Mock<IPaymentProcessorFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogsService = new Mock<ILogsServiceClient>();
        _mockAuditService = new Mock<IAuditServiceClient>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CreatePaymentCommandHandler>>();

        _handler = new CreatePaymentCommandHandler(
            _mockPaymentRepository.Object,
            _mockOrderService.Object,
            _mockFraudDetectionService.Object,
            _mockPaymentProcessorFactory.Object,
            _mockUnitOfWork.Object,
            _mockLogsService.Object,
            _mockAuditService.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldCreatePaymentSuccessfully()
    {
        // Arrange
        var command = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.Amount, 100.50m)
            .With(x => x.Currency, "EUR")
            .With(x => x.PaymentMethod, PaymentMethodType.CreditCard)
            .Create();

        var orderValidation = new OrderValidationResult { IsValid = true, Errors = new List<string>() };
        var fraudCheckResult = new FraudCheckResult { IsFraud = false, FraudScore = 0.1 };

        _mockOrderService.Setup(x => x.ValidateOrderAsync(command.OrderId))
            .ReturnsAsync(orderValidation);
        
        _mockFraudDetectionService.Setup(x => x.CheckFraudAsync(It.IsAny<FraudCheckRequest>()))
            .ReturnsAsync(fraudCheckResult);

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Payment>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockAuditService.Setup(x => x.AuditCreateAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Amount.Should().Be(command.Amount);
        result.Data.Currency.Should().Be(command.Currency);
        result.Data.Status.Should().Be(PaymentStatus.Pending);

        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Payment>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogsService.Verify(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.AtLeastOnce);
        _mockAuditService.Verify(x => x.AuditCreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithInvalidOrder_ShouldReturnFailure()
    {
        // Arrange
        var command = _fixture.Create<CreatePaymentCommand>();
        var orderValidation = new OrderValidationResult 
        { 
            IsValid = false, 
            Errors = new List<string> { "Order not found" } 
        };

        _mockOrderService.Setup(x => x.ValidateOrderAsync(command.OrderId))
            .ReturnsAsync(orderValidation);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockLogsService.Setup(x => x.LogWarningAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Order validation failed");

        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Payment>()), Times.Never);
        _mockLogsService.Verify(x => x.LogWarningAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithFraudDetected_ShouldReturnForbidden()
    {
        // Arrange
        var command = _fixture.Create<CreatePaymentCommand>();
        var orderValidation = new OrderValidationResult { IsValid = true, Errors = new List<string>() };
        var fraudCheckResult = new FraudCheckResult 
        { 
            IsFraud = true, 
            FraudScore = 0.9,
            Reasons = new List<string> { "Suspicious transaction pattern" }
        };

        _mockOrderService.Setup(x => x.ValidateOrderAsync(command.OrderId))
            .ReturnsAsync(orderValidation);
        
        _mockFraudDetectionService.Setup(x => x.CheckFraudAsync(It.IsAny<FraudCheckRequest>()))
            .ReturnsAsync(fraudCheckResult);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockLogsService.Setup(x => x.LogWarningAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Be("Payment blocked due to fraud detection");

        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Payment>()), Times.Never);
        _mockLogsService.Verify(x => x.LogWarningAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldLogAllOperations()
    {
        // Arrange
        var command = _fixture.Create<CreatePaymentCommand>();
        var orderValidation = new OrderValidationResult { IsValid = true, Errors = new List<string>() };
        var fraudCheckResult = new FraudCheckResult { IsFraud = false, FraudScore = 0.1 };

        _mockOrderService.Setup(x => x.ValidateOrderAsync(command.OrderId))
            .ReturnsAsync(orderValidation);
        
        _mockFraudDetectionService.Setup(x => x.CheckFraudAsync(It.IsAny<FraudCheckRequest>()))
            .ReturnsAsync(fraudCheckResult);

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Payment>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockAuditService.Setup(x => x.AuditCreateAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Vérification des logs enterprise
        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.Is<string>(s => s.Contains("Creating payment for order")), 
            It.IsAny<Dictionary<string, object>>()), Times.Once);

        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.Is<string>(s => s.Contains("Payment created successfully")), 
            It.IsAny<Dictionary<string, object>>()), Times.Once);

        // Vérification de l'audit trail
        _mockAuditService.Verify(x => x.AuditCreateAsync(
            It.Is<string>(s => s == command.CustomerId.ToString()),
            "Customer",
            "Payment",
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithException_ShouldLogErrorAndReturnInternalServerError()
    {
        // Arrange
        var command = _fixture.Create<CreatePaymentCommand>();
        var expectedException = new InvalidOperationException("Database connection failed");

        _mockOrderService.Setup(x => x.ValidateOrderAsync(command.OrderId))
            .ThrowsAsync(expectedException);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockLogsService.Setup(x => x.LogErrorAsync(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Internal server error while creating payment");
        result.Errors.Should().Contain(expectedException.Message);

        _mockLogsService.Verify(x => x.LogErrorAsync(
            It.Is<string>(s => s.Contains("Error creating payment for order")), 
            It.Is<Exception>(ex => ex == expectedException),
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithLargeAmount_ShouldProcessCorrectly()
    {
        // Arrange
        var command = _fixture.Build<CreatePaymentCommand>()
            .With(x => x.Amount, 99999.99m)
            .With(x => x.Currency, "EUR")
            .Create();

        var orderValidation = new OrderValidationResult { IsValid = true, Errors = new List<string>() };
        var fraudCheckResult = new FraudCheckResult { IsFraud = false, FraudScore = 0.2 };

        _mockOrderService.Setup(x => x.ValidateOrderAsync(command.OrderId))
            .ReturnsAsync(orderValidation);
        
        _mockFraudDetectionService.Setup(x => x.CheckFraudAsync(It.IsAny<FraudCheckRequest>()))
            .ReturnsAsync(fraudCheckResult);

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Payment>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockAuditService.Setup(x => x.AuditCreateAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Amount.Should().Be(99999.99m);
        result.Data.Currency.Should().Be("EUR");
        
        // Vérification que le montant élevé est correctement logué
        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.IsAny<string>(),
            It.Is<Dictionary<string, object>>(dict => 
                dict.ContainsKey("Amount") && dict["Amount"].Equals(99999.99m))), Times.AtLeastOnce);
    }
}

/// <summary>
/// Classes de support pour les tests
/// </summary>
public class OrderValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class FraudCheckResult
{
    public bool IsFraud { get; set; }
    public double FraudScore { get; set; }
    public List<string> Reasons { get; set; } = new();
}