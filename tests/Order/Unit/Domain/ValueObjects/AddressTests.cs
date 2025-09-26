using NUnit.Framework;
using FluentAssertions;
using Order.Domain.ValueObjects;

namespace Order.Tests.Unit.Domain.ValueObjects;

/// <summary>
/// Tests pour le Value Object Address
/// </summary>
[TestFixture]
public class AddressTests
{
    [Test]
    [Description("Création d'une Address avec des valeurs valides")]
    public void Should_Create_Address_With_Valid_Values()
    {
        // Arrange & Act
        var address = Address.Create(
            "123 Main Street",
            "Paris",
            "75001",
            "France"
        );

        // Assert
        address.Street.Should().Be("123 Main Street");
        address.City.Should().Be("Paris");
        address.PostalCode.Should().Be("75001");
        address.Country.Should().Be("France");
        address.AddressLine2.Should().BeNull();
    }

    [Test]
    [Description("Création d'une Address avec ligne 2")]
    public void Should_Create_Address_With_AddressLine2()
    {
        // Arrange & Act
        var address = Address.Create(
            "123 Main Street",
            "Paris",
            "75001",
            "France",
            "Appartement 4B"
        );

        // Assert
        address.Street.Should().Be("123 Main Street");
        address.City.Should().Be("Paris");
        address.PostalCode.Should().Be("75001");
        address.Country.Should().Be("France");
        address.AddressLine2.Should().Be("Appartement 4B");
    }

    [Test]
    [Description("Formatage de l'adresse complète")]
    public void Should_Format_Full_Address()
    {
        // Arrange
        var address = Address.Create(
            "123 Main Street",
            "Paris",
            "75001",
            "France",
            "Apt 4B"
        );

        // Act
        var fullAddress = address.GetFullAddress();

        // Assert
        fullAddress.Should().Be("123 Main Street, Apt 4B, 75001 Paris, France");
    }

    [Test]
    [Description("Formatage de l'adresse sans ligne 2")]
    public void Should_Format_Address_Without_AddressLine2()
    {
        // Arrange
        var address = Address.Create(
            "123 Main Street",
            "Paris",
            "75001",
            "France"
        );

        // Act
        var fullAddress = address.GetFullAddress();

        // Assert
        fullAddress.Should().Be("123 Main Street, 75001 Paris, France");
    }

    [Test]
    [Description("Erreur avec rue null ou vide")]
    public void Should_Throw_With_Invalid_Street()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create(null!, "Paris", "75001", "France"));
        Assert.Throws<ArgumentException>(() => Address.Create("", "Paris", "75001", "France"));
        Assert.Throws<ArgumentException>(() => Address.Create(" ", "Paris", "75001", "France"));
    }

    [Test]
    [Description("Erreur avec ville null ou vide")]
    public void Should_Throw_With_Invalid_City()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", null!, "75001", "France"));
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "", "75001", "France"));
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", " ", "75001", "France"));
    }

    [Test]
    [Description("Erreur avec code postal null ou vide")]
    public void Should_Throw_With_Invalid_PostalCode()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "Paris", null!, "France"));
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "Paris", "", "France"));
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "Paris", " ", "France"));
    }

    [Test]
    [Description("Erreur avec pays null ou vide")]
    public void Should_Throw_With_Invalid_Country()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "Paris", "75001", null!));
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "Paris", "75001", ""));
        Assert.Throws<ArgumentException>(() => Address.Create("123 Main St", "Paris", "75001", " "));
    }
}