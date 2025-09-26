using NUnit.Framework;
using FluentAssertions;
using Order.Domain.ValueObjects;

namespace Order.Tests.Unit.Domain.ValueObjects;

/// <summary>
/// Tests pour le Value Object Money
/// </summary>
[TestFixture]
public class MoneyTests
{
    [Test]
    [Description("Création d'un Money avec des valeurs valides")]
    public void Should_Create_Money_With_Valid_Values()
    {
        // Arrange & Act
        var money = Money.Create(100.50m, "EUR");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("EUR");
    }

    [Test]
    [Description("Création d'un Money avec devise par défaut")]
    public void Should_Create_Money_With_Default_Currency()
    {
        // Arrange & Act
        var money = Money.Create(50.00m);

        // Assert
        money.Amount.Should().Be(50.00m);
        money.Currency.Should().Be("EUR");
    }

    [Test]
    [Description("Addition de deux Money avec même devise")]
    public void Should_Add_Money_With_Same_Currency()
    {
        // Arrange
        var money1 = Money.Create(100.00m, "EUR");
        var money2 = Money.Create(50.50m, "EUR");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150.50m);
        result.Currency.Should().Be("EUR");
    }

    [Test]
    [Description("Erreur lors de l'addition de Money avec devises différentes")]
    public void Should_Throw_When_Adding_Different_Currencies()
    {
        // Arrange
        var money1 = Money.Create(100.00m, "EUR");
        var money2 = Money.Create(50.00m, "USD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
    }

    [Test]
    [Description("Création de Money zéro")]
    public void Should_Create_Zero_Money()
    {
        // Arrange & Act
        var zero = Money.Zero();

        // Assert
        zero.Amount.Should().Be(0m);
        zero.Currency.Should().Be("EUR");
    }

    [Test]
    [Description("Erreur avec montant négatif")]
    public void Should_Throw_With_Negative_Amount()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(-10.00m));
    }

    [Test]
    [Description("Erreur avec devise null ou vide")]
    public void Should_Throw_With_Null_Currency()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(100.00m, null!));
        Assert.Throws<ArgumentException>(() => Money.Create(100.00m, ""));
        Assert.Throws<ArgumentException>(() => Money.Create(100.00m, " "));
    }
}