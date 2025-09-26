using NUnit.Framework;
using FluentAssertions;
using Customer.Domain.Aggregates.CustomerAggregate;

namespace Customer.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for Customer domain entity following NiesPro Enterprise standards
    /// </summary>
    [TestFixture]
    public class CustomerTests
    {
        [Test]
        public void Customer_WhenCreated_ShouldHaveDefaultValues()
        {
            // Act
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer();

            // Assert
            customer.IsActive.Should().BeTrue();
            customer.IsVip.Should().BeFalse();
            customer.LoyaltyPoints.Should().Be(0);
            customer.TotalOrders.Should().Be(0);
            customer.TotalSpent.Should().Be(0);
            customer.CustomerType.Should().Be("Regular");
            customer.Addresses.Should().NotBeNull().And.BeEmpty();
            customer.Preferences.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public void FullName_ShouldConcatenateFirstAndLastName()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var fullName = customer.FullName;

            // Assert
            fullName.Should().Be("John Doe");
        }

        [Test]
        public void AddLoyaltyPoints_ShouldIncreaseLoyaltyPoints()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                LoyaltyPoints = 100
            };

            // Act
            customer.AddLoyaltyPoints(50);

            // Assert
            customer.LoyaltyPoints.Should().Be(150);
        }

        [Test]
        public void RedeemLoyaltyPoints_WithSufficientPoints_ShouldDeductPoints()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                LoyaltyPoints = 100
            };

            // Act
            customer.RedeemLoyaltyPoints(30);

            // Assert
            customer.LoyaltyPoints.Should().Be(70);
        }

        [Test]
        public void RedeemLoyaltyPoints_WithInsufficientPoints_ShouldThrowException()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                LoyaltyPoints = 50
            };

            // Act & Assert
            var action = () => customer.RedeemLoyaltyPoints(100);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Insufficient loyalty points");
        }

        [Test]
        public void RecordVisit_ShouldUpdateLastVisit()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer();
            var beforeVisit = DateTime.UtcNow;

            // Act
            customer.RecordVisit();

            // Assert
            customer.LastVisit.Should().NotBeNull()
                .And.BeOnOrAfter(beforeVisit)
                .And.BeOnOrBefore(DateTime.UtcNow);
        }

        [Test]
        public void RecordOrder_ShouldUpdateOrderCountAndSpentAndLastVisit()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                TotalOrders = 2,
                TotalSpent = 150.50m
            };
            var orderAmount = 75.25m;
            var beforeOrder = DateTime.UtcNow;

            // Act
            customer.RecordOrder(orderAmount);

            // Assert
            customer.TotalOrders.Should().Be(3);
            customer.TotalSpent.Should().Be(225.75m);
            customer.LastVisit.Should().NotBeNull()
                .And.BeOnOrAfter(beforeOrder)
                .And.BeOnOrBefore(DateTime.UtcNow);
        }

        [Test]
        public void PromoteToVip_ShouldSetVipStatusToTrue()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                IsVip = false
            };

            // Act
            customer.PromoteToVip();

            // Assert
            customer.IsVip.Should().BeTrue();
        }

        [Test]
        public void Deactivate_ShouldSetActiveStatusToFalse()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                IsActive = true
            };

            // Act
            customer.Deactivate();

            // Assert
            customer.IsActive.Should().BeFalse();
        }

        [Test]
        public void Activate_ShouldSetActiveStatusToTrue()
        {
            // Arrange
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                IsActive = false
            };

            // Act
            customer.Activate();

            // Assert
            customer.IsActive.Should().BeTrue();
        }
    }
}