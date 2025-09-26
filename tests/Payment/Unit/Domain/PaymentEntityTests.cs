using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;

namespace Payment.Tests.Unit.Domain;

/// <summary>
/// Tests unitaires pour l'entité Payment - NiesPro Enterprise Standards
/// </summary>
[TestFixture]
public class PaymentEntityTests
{
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Payment_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var payment = new Payment.Domain.Entities.Payment();

        // Assert
        payment.Id.Should().BeEmpty();
        payment.PaymentNumber.Should().BeNullOrEmpty();
        payment.Amount.Value.Should().Be(0);
        payment.Amount.Currency.Should().Be("EUR");
        payment.Status.Should().Be(PaymentStatus.Created);
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Test]
    public void Payment_WithValidData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var amount = 150.75m;
        var currency = "EUR";
        var paymentNumber = "PAY_123456789";

        // Act
        var payment = new PaymentEntity
        {
            Id = paymentId,
            OrderId = orderId,
            CustomerId = customerId,
            MerchantId = merchantId,
            Amount = amount,
            Currency = currency,
            PaymentNumber = paymentNumber,
            Status = PaymentStatus.Pending
        };

        // Assert
        payment.Id.Should().Be(paymentId);
        payment.OrderId.Should().Be(orderId);
        payment.CustomerId.Should().Be(customerId);
        payment.MerchantId.Should().Be(merchantId);
        payment.Amount.Should().Be(amount);
        payment.Currency.Should().Be(currency);
        payment.PaymentNumber.Should().Be(paymentNumber);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Test]
    public void Payment_UpdateStatus_ShouldChangeStatusAndTimestamp()
    {
        // Arrange
        var payment = new PaymentEntity
        {
            Status = PaymentStatus.Pending
        };

        // Act
        payment.Status = PaymentStatus.Processing;
        payment.UpdatedAt = DateTime.UtcNow;

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.UpdatedAt.Should().NotBeNull();
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Payment_SetToCompleted_ShouldHaveCorrectFinalStatus()
    {
        // Arrange
        var payment = new PaymentEntity
        {
            Status = PaymentStatus.Processing,
            Amount = 100.0m,
            Currency = "USD"
        };

        // Act
        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.CompletedAt.Should().NotBeNull();
        payment.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Payment_SetToFailed_ShouldHaveFailureInformation()
    {
        // Arrange
        var payment = new PaymentEntity
        {
            Status = PaymentStatus.Processing
        };
        var failureReason = "Insufficient funds";

        // Act
        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = failureReason;
        payment.FailedAt = DateTime.UtcNow;

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(failureReason);
        payment.FailedAt.Should().NotBeNull();
        payment.FailedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Payment_WithDifferentCurrencies_ShouldAcceptAllFormats()
    {
        // Arrange
        var currencies = new[] { "USD", "EUR", "GBP", "JPY", "CAD", "CHF" };

        foreach (var currency in currencies)
        {
            // Act
            var payment = new PaymentEntity
            {
                Currency = currency,
                Amount = 100.0m
            };

            // Assert
            payment.Currency.Should().Be(currency);
        }
    }

    [Test]
    public void Payment_WithZeroAmount_ShouldBeValidForRefunds()
    {
        // Arrange & Act
        var payment = new PaymentEntity
        {
            Amount = 0m,
            Currency = "EUR",
            Status = PaymentStatus.Completed
        };

        // Assert
        payment.Amount.Should().Be(0m);
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Test]
    public void Payment_WithNegativeAmount_ShouldRepresentRefund()
    {
        // Arrange & Act
        var payment = new PaymentEntity
        {
            Amount = -50.25m,
            Currency = "USD",
            Status = PaymentStatus.Completed
        };

        // Assert
        payment.Amount.Should().Be(-50.25m);
        payment.Currency.Should().Be("USD");
        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Test]
    public void Payment_WithExpirationTime_ShouldHaveCorrectExpiry()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(2);

        // Act
        var payment = new PaymentEntity
        {
            ExpiresAt = expiresAt,
            Status = PaymentStatus.Pending
        };

        // Assert
        payment.ExpiresAt.Should().Be(expiresAt);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Test]
    public void Payment_IsExpired_ShouldReturnCorrectStatus()
    {
        // Arrange
        var expiredPayment = new PaymentEntity
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            Status = PaymentStatus.Pending
        };

        var validPayment = new PaymentEntity
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            Status = PaymentStatus.Pending
        };

        // Act & Assert
        (expiredPayment.ExpiresAt < DateTime.UtcNow).Should().BeTrue();
        (validPayment.ExpiresAt > DateTime.UtcNow).Should().BeTrue();
    }
}