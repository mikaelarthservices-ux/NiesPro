using Order.Domain.ValueObjects;

namespace Order.Tests.Domain.ValueObjects;

/// <summary>
/// Tests unitaires pour le Value Object Money
/// </summary>
public sealed class MoneyTests
{
    [Fact]
    public void Money_Creation_WithValidValues_ShouldSucceed()
    {
        // Arrange & Act
        var money = new Money(100.50m, "EUR");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Money_Zero_ShouldCreateZeroAmount()
    {
        // Arrange & Act
        var zero = Money.Zero("USD");

        // Assert
        zero.Amount.Should().Be(0m);
        zero.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Add_WithSameCurrency_ShouldSucceed()
    {
        // Arrange
        var money1 = new Money(50.25m, "EUR");
        var money2 = new Money(25.75m, "EUR");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(76.00m);
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Money_Add_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(50.25m, "EUR");
        var money2 = new Money(25.75m, "USD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
    }

    [Fact]
    public void Money_Subtract_WithSameCurrency_ShouldSucceed()
    {
        // Arrange
        var money1 = new Money(100.00m, "EUR");
        var money2 = new Money(30.50m, "EUR");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(69.50m);
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Money_Multiply_ShouldSucceed()
    {
        // Arrange
        var money = new Money(25.00m, "EUR");

        // Act
        var result = money.Multiply(3);

        // Assert
        result.Amount.Should().Be(75.00m);
        result.Currency.Should().Be("EUR");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Money_Creation_WithNegativeAmount_ShouldThrowException(decimal amount)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(amount, "EUR"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Money_Creation_WithInvalidCurrency_ShouldThrowException(string currency)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, currency));
    }

    [Fact]
    public void Money_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100.50m, "EUR");
        var money2 = new Money(100.50m, "EUR");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Money_Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100.50m, "EUR");
        var money2 = new Money(100.50m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }
}