using Order.Domain.ValueObjects;

namespace Order.Tests.Domain.ValueObjects;

/// <summary>
/// Tests unitaires pour le Value Object Address
/// </summary>
public sealed class AddressTests
{
    [Fact]
    public void Address_Creation_WithValidValues_ShouldSucceed()
    {
        // Arrange & Act
        var address = new Address(
            "123 Main Street",
            "Apartment 4B",
            "Paris",
            "75001",
            "France"
        );

        // Assert
        address.Street.Should().Be("123 Main Street");
        address.Complement.Should().Be("Apartment 4B");
        address.City.Should().Be("Paris");
        address.PostalCode.Should().Be("75001");
        address.Country.Should().Be("France");
    }

    [Fact]
    public void Address_Creation_WithoutComplement_ShouldSucceed()
    {
        // Arrange & Act
        var address = new Address(
            "456 Oak Avenue",
            null,
            "Lyon",
            "69000",
            "France"
        );

        // Assert
        address.Street.Should().Be("456 Oak Avenue");
        address.Complement.Should().BeNull();
        address.City.Should().Be("Lyon");
        address.PostalCode.Should().Be("69000");
        address.Country.Should().Be("France");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Address_Creation_WithInvalidStreet_ShouldThrowException(string street)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Address(
            street,
            null,
            "Paris",
            "75001",
            "France"
        ));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Address_Creation_WithInvalidCity_ShouldThrowException(string city)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Address(
            "123 Main Street",
            null,
            city,
            "75001",
            "France"
        ));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Address_Creation_WithInvalidPostalCode_ShouldThrowException(string postalCode)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Address(
            "123 Main Street",
            null,
            "Paris",
            postalCode,
            "France"
        ));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Address_Creation_WithInvalidCountry_ShouldThrowException(string country)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Address(
            "123 Main Street",
            null,
            "Paris",
            "75001",
            country
        ));
    }

    [Fact]
    public void Address_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Apt 1", "Paris", "75001", "France");
        var address2 = new Address("123 Main St", "Apt 1", "Paris", "75001", "France");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void Address_Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Apt 1", "Paris", "75001", "France");
        var address2 = new Address("456 Oak Ave", "Apt 1", "Paris", "75001", "France");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Address_ToString_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address("123 Main St", "Apt 1", "Paris", "75001", "France");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Contain("123 Main St");
        result.Should().Contain("Apt 1");
        result.Should().Contain("Paris");
        result.Should().Contain("75001");
        result.Should().Contain("France");
    }
}