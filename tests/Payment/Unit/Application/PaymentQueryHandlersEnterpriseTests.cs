using NUnit.Framework;
using FluentAssertions;
using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Payment.Application.Handlers;
using Payment.Application.Queries;
using Payment.Application.DTOs;
using Payment.Domain.Entities;
using Payment.Domain.Interfaces;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Payment.Tests.Unit.Application;

/// <summary>
/// Unit tests for GetPaymentByIdQueryHandler following NiesPro Enterprise standards
/// </summary>
[TestFixture]
public class GetPaymentByIdQueryHandlerEnterpriseTests
{
    private IFixture _fixture;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private Mock<ITransactionRepository> _mockTransactionRepository;
    private Mock<ILogsServiceClient> _mockLogsService;
    private Mock<ILogger<GetPaymentByIdQueryHandler>> _mockLogger;
    private GetPaymentByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockLogsService = new Mock<ILogsServiceClient>();
        _mockLogger = new Mock<ILogger<GetPaymentByIdQueryHandler>>();

        _handler = new GetPaymentByIdQueryHandler(
            _mockPaymentRepository.Object,
            _mockTransactionRepository.Object,
            _mockLogsService.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WithExistingPayment_ShouldReturnPaymentDetails()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);
        
        var payment = new Domain.Entities.Payment
        {
            Id = paymentId,
            PaymentNumber = "PAY-20241026-1234",
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            MerchantId = Guid.NewGuid(),
            Amount = new Money(150.00m, "EUR"),
            Status = PaymentStatus.Completed,
            PaymentMethod = PaymentMethodType.CreditCard,
            Description = "Test payment",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-1),
            Metadata = new Dictionary<string, object> { { "test", "value" } }
        };

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(paymentId);
        result.Data.PaymentNumber.Should().Be("PAY-20241026-1234");
        result.Data.Amount.Should().Be(150.00m);
        result.Data.Currency.Should().Be("EUR");
        result.Data.Status.Should().Be(PaymentStatus.Completed);
        result.Data.PaymentMethod.Should().Be(PaymentMethodType.CreditCard);
        result.Data.Transactions.Should().BeNull(); // Par défaut, pas de transactions incluses

        _mockPaymentRepository.Verify(x => x.GetByIdAsync(paymentId), Times.Once);
        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.Is<string>(s => s.Contains("Retrieving payment")), 
            It.IsAny<Dictionary<string, object>>()), Times.Once);
        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.Is<string>(s => s.Contains("Payment retrieved successfully")), 
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentPayment_ShouldReturnNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync((Domain.Entities.Payment?)null);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockLogsService.Setup(x => x.LogWarningAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Payment not found");

        _mockLogsService.Verify(x => x.LogWarningAsync(
            It.Is<string>(s => s.Contains("Payment not found")), 
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithIncludeTransactions_ShouldIncludeTransactionData()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId) 
        { 
            IncludeTransactions = true 
        };
        
        var payment = _fixture.Build<Domain.Entities.Payment>()
            .With(x => x.Id, paymentId)
            .With(x => x.Amount, new Money(100.00m, "EUR"))
            .Create();

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                Type = TransactionType.Payment,
                Status = TransactionStatus.Success,
                Amount = new Money(100.00m, "EUR"),
                ExternalReference = "TXN-123456",
                ProcessedAt = DateTime.UtcNow
            }
        };

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _mockTransactionRepository.Setup(x => x.GetByPaymentIdAsync(paymentId))
            .ReturnsAsync(transactions);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Transactions.Should().NotBeNull();
        result.Data.Transactions!.Should().HaveCount(1);
        result.Data.Transactions.First().Type.Should().Be(TransactionType.Payment);
        result.Data.Transactions.First().Status.Should().Be(TransactionStatus.Success);
        result.Data.Transactions.First().ExternalReference.Should().Be("TXN-123456");

        _mockTransactionRepository.Verify(x => x.GetByPaymentIdAsync(paymentId), Times.Once);
    }

    [Test]
    public async Task Handle_WithIncludeRefunds_ShouldIncludeRefundData()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId) 
        { 
            IncludeRefunds = true 
        };
        
        var payment = _fixture.Build<Domain.Entities.Payment>()
            .With(x => x.Id, paymentId)
            .With(x => x.Amount, new Money(100.00m, "EUR"))
            .Create();

        var refunds = new List<PaymentRefund>
        {
            new PaymentRefund
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                Amount = new Money(50.00m, "EUR"),
                Reason = "Customer request",
                Status = RefundStatus.Completed,
                RefundDate = DateTime.UtcNow
            }
        };

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.GetRefundsByPaymentIdAsync(paymentId))
            .ReturnsAsync(refunds);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Refunds.Should().NotBeNull();
        result.Data.Refunds!.Should().HaveCount(1);
        result.Data.Refunds.First().Amount.Should().Be(50.00m);
        result.Data.Refunds.First().Reason.Should().Be("Customer request");
        result.Data.Refunds.First().Status.Should().Be(RefundStatus.Completed);

        _mockPaymentRepository.Verify(x => x.GetRefundsByPaymentIdAsync(paymentId), Times.Once);
    }

    [Test]
    public async Task Handle_WithException_ShouldLogErrorAndReturnInternalServerError()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);
        var expectedException = new InvalidOperationException("Database connection failed");

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId))
            .ThrowsAsync(expectedException);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        _mockLogsService.Setup(x => x.LogErrorAsync(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Internal server error while retrieving payment");
        result.Errors.Should().Contain(expectedException.Message);

        _mockLogsService.Verify(x => x.LogErrorAsync(
            It.Is<string>(s => s.Contains("Error retrieving payment")), 
            It.Is<Exception>(ex => ex == expectedException),
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldLogQueryDetailsWithCorrectMetadata()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId)
        {
            IncludeTransactions = true,
            IncludeRefunds = true
        };
        
        var payment = _fixture.Build<Domain.Entities.Payment>()
            .With(x => x.Id, paymentId)
            .With(x => x.Amount, new Money(100.00m, "EUR"))
            .Create();

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Vérification des métadonnées de log
        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.IsAny<string>(),
            It.Is<Dictionary<string, object>>(dict => 
                dict.ContainsKey("QueryId") &&
                dict.ContainsKey("PaymentId") &&
                dict.ContainsKey("IncludeTransactions") &&
                dict.ContainsKey("IncludeRefunds") &&
                dict["PaymentId"].Equals(paymentId) &&
                dict["IncludeTransactions"].Equals(true) &&
                dict["IncludeRefunds"].Equals(true))), Times.AtLeastOnce);
    }
}

/// <summary>
/// Unit tests for GetPaymentsByCustomerQueryHandler following NiesPro Enterprise standards
/// </summary>
[TestFixture]
public class GetPaymentsByCustomerQueryHandlerEnterpriseTests
{
    private IFixture _fixture;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private Mock<ILogsServiceClient> _mockLogsService;
    private Mock<ILogger<GetPaymentsByCustomerQueryHandler>> _mockLogger;
    private GetPaymentsByCustomerQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockLogsService = new Mock<ILogsServiceClient>();
        _mockLogger = new Mock<ILogger<GetPaymentsByCustomerQueryHandler>>();

        _handler = new GetPaymentsByCustomerQueryHandler(
            _mockPaymentRepository.Object,
            _mockLogsService.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WithValidCustomer_ShouldReturnPagedResults()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetPaymentsByCustomerQuery(customerId)
        {
            Page = 1,
            PageSize = 10
        };

        var payments = new List<Domain.Entities.Payment>
        {
            _fixture.Build<Domain.Entities.Payment>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Amount, new Money(100.00m, "EUR"))
                .Create(),
            _fixture.Build<Domain.Entities.Payment>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Amount, new Money(200.00m, "EUR"))
                .Create()
        };

        _mockPaymentRepository.Setup(x => x.GetByCustomerIdAsync(
            customerId, 1, 10, null, null, null))
            .ReturnsAsync(payments);

        _mockPaymentRepository.Setup(x => x.GetCountByCustomerIdAsync(
            customerId, null, null, null))
            .ReturnsAsync(25);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(25);
        result.Data.Page.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalPages.Should().Be(3);
        result.Data.HasNext.Should().BeTrue();
        result.Data.HasPrevious.Should().BeFalse();

        _mockPaymentRepository.Verify(x => x.GetByCustomerIdAsync(
            customerId, 1, 10, null, null, null), Times.Once);
        _mockPaymentRepository.Verify(x => x.GetCountByCustomerIdAsync(
            customerId, null, null, null), Times.Once);
    }

    [Test]
    public async Task Handle_WithInvalidPageSize_ShouldUseDefaultValues()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetPaymentsByCustomerQuery(customerId)
        {
            Page = 0,      // Invalid
            PageSize = 150 // Too large
        };

        _mockPaymentRepository.Setup(x => x.GetByCustomerIdAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PaymentStatus?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<Domain.Entities.Payment>());

        _mockPaymentRepository.Setup(x => x.GetCountByCustomerIdAsync(
            It.IsAny<Guid>(), It.IsAny<PaymentStatus?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Vérification des valeurs corrigées
        _mockPaymentRepository.Verify(x => x.GetByCustomerIdAsync(
            customerId, 1, 20, null, null, null), Times.Once); // Page=1, PageSize=20 (defaults)
    }

    [Test]
    public async Task Handle_WithStatusFilter_ShouldApplyFilter()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetPaymentsByCustomerQuery(customerId)
        {
            Status = PaymentStatus.Completed,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        _mockPaymentRepository.Setup(x => x.GetByCustomerIdAsync(
            customerId, 1, 20, PaymentStatus.Completed, 
            It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Domain.Entities.Payment>());

        _mockPaymentRepository.Setup(x => x.GetCountByCustomerIdAsync(
            customerId, PaymentStatus.Completed, 
            It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        _mockLogsService.Setup(x => x.LogInformationAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPaymentRepository.Verify(x => x.GetByCustomerIdAsync(
            customerId, 1, 20, PaymentStatus.Completed, 
            It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);

        // Vérification du logging avec filtres
        _mockLogsService.Verify(x => x.LogInformationAsync(
            It.IsAny<string>(),
            It.Is<Dictionary<string, object>>(dict => 
                dict.ContainsKey("Status") &&
                dict["Status"].ToString() == PaymentStatus.Completed.ToString())), Times.AtLeastOnce);
    }
}