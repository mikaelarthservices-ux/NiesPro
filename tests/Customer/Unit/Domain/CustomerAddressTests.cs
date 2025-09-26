using NUnit.Framework;
using FluentAssertions;
using Customer.Domain.Aggregates.CustomerAggregate;

namespace Customer.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for CustomerAddress domain entity following NiesPro Enterprise standards
    /// </summary>
    [TestFixture]
    public class CustomerAddressTests
    {
        [Test]
        public void CustomerAddress_WhenCreated_ShouldHaveDefaultValues()
        {
            // Act
            var address = new CustomerAddress();

            // Assert
            address.IsDefault.Should().BeFalse();
            address.IsActive.Should().BeTrue();
            address.Type.Should().Be(string.Empty);
            address.AddressLine1.Should().Be(string.Empty);
            address.City.Should().Be(string.Empty);
            address.Country.Should().Be(string.Empty);
        }

        [Test]
        public void CustomerAddress_WithAllProperties_ShouldSetCorrectly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var addressId = Guid.NewGuid();

            // Act
            var address = new CustomerAddress
            {
                Id = addressId,
                CustomerId = customerId,
                Type = "Home",
                Label = "Primary Home",
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Apt 4B",
                City = "Springfield",
                State = "IL",
                PostalCode = "62704",
                Country = "USA",
                IsDefault = true,
                IsActive = true
            };

            // Assert
            address.Id.Should().Be(addressId);
            address.CustomerId.Should().Be(customerId);
            address.Type.Should().Be("Home");
            address.Label.Should().Be("Primary Home");
            address.AddressLine1.Should().Be("123 Main Street");
            address.AddressLine2.Should().Be("Apt 4B");
            address.City.Should().Be("Springfield");
            address.State.Should().Be("IL");
            address.PostalCode.Should().Be("62704");
            address.Country.Should().Be("USA");
            address.IsDefault.Should().BeTrue();
            address.IsActive.Should().BeTrue();
        }

        [Test]
        public void CustomerAddress_WithMinimalRequiredProperties_ShouldBeValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Act
            var address = new CustomerAddress
            {
                CustomerId = customerId,
                Type = "Work",
                AddressLine1 = "456 Business Ave",
                City = "Business City",
                Country = "USA"
            };

            // Assert
            address.CustomerId.Should().Be(customerId);
            address.Type.Should().Be("Work");
            address.AddressLine1.Should().Be("456 Business Ave");
            address.City.Should().Be("Business City");
            address.Country.Should().Be("USA");
            address.IsDefault.Should().BeFalse(); // Default value
            address.IsActive.Should().BeTrue(); // Default value
        }

        [Test]
        public void CustomerAddress_TypeVariations_ShouldAcceptDifferentTypes()
        {
            // Arrange & Act
            var homeAddress = new CustomerAddress { Type = "Home" };
            var workAddress = new CustomerAddress { Type = "Work" };
            var billingAddress = new CustomerAddress { Type = "Billing" };
            var shippingAddress = new CustomerAddress { Type = "Shipping" };

            // Assert
            homeAddress.Type.Should().Be("Home");
            workAddress.Type.Should().Be("Work");
            billingAddress.Type.Should().Be("Billing");
            shippingAddress.Type.Should().Be("Shipping");
        }
    }
}