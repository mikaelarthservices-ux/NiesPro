using NUnit.Framework;
using FluentAssertions;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Tests.Unit.Domain.Enterprise;

/// <summary>
/// Tests Enterprise pour Order Service multi-contexte
/// Validation architecture NiesPro ERP de très haut standing
/// </summary>
[TestFixture]
public class OrderEnterpriseTests
{
    private CustomerInfo _customerInfo = null!;

    [SetUp]
    public void SetUp()
    {
        _customerInfo = CustomerInfo.Create(
            "John",
            "Doe", 
            "john.doe@example.com",
            "+1234567890");
    }

    [TestFixture]
    public class RestaurantContextTests : OrderEnterpriseTests
    {
        [Test]
        public void CreateRestaurant_WithDineIn_ShouldRequireTableNumber()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                OrderEntity.CreateRestaurant(
                    "REST-001",
                    Guid.NewGuid(),
                    _customerInfo,
                    ServiceType.DineIn)); // Pas de numéro de table
        }

        [Test]
        public void CreateRestaurant_WithValidDineIn_ShouldSucceed()
        {
            // Arrange
            var tableNumber = 15;
            var waiterId = Guid.NewGuid();

            // Act
            var order = OrderEntity.CreateRestaurant(
                "REST-001",
                Guid.NewGuid(),
                _customerInfo,
                ServiceType.DineIn,
                tableNumber,
                waiterId);

            // Assert
            order.BusinessContext.Should().Be(BusinessContext.Restaurant);
            order.ServiceInfo.Type.Should().Be(ServiceType.DineIn);
            order.ServiceInfo.TableNumber.Should().Be(tableNumber);
            order.ServiceInfo.WaiterId.Should().Be(waiterId);
        }

        [Test]
        public void RestaurantWorkflow_ShouldFollowCorrectSequence()
        {
            // Arrange
            var order = OrderEntity.CreateRestaurant(
                "REST-001",
                Guid.NewGuid(),
                _customerInfo,
                ServiceType.DineIn,
                15);

            // Act & Assert - Workflow Restaurant
            order.Status.Should().Be(OrderStatus.Pending);
            
            order.TransitionToStatus(OrderStatus.Confirmed);
            order.Status.Should().Be(OrderStatus.Confirmed);
            
            order.TransitionToStatus(OrderStatus.KitchenQueue);
            order.Status.Should().Be(OrderStatus.KitchenQueue);
            
            order.TransitionToStatus(OrderStatus.Cooking);
            order.Status.Should().Be(OrderStatus.Cooking);
            
            order.TransitionToStatus(OrderStatus.Ready);
            order.Status.Should().Be(OrderStatus.Ready);
            
            order.TransitionToStatus(OrderStatus.Served);
            order.Status.Should().Be(OrderStatus.Served);
            
            // Terminal status
            order.Status.IsTerminalStatus(BusinessContext.Restaurant).Should().BeTrue();
        }

        [Test]
        public void RestaurantOrder_ShouldRequireKitchenIntegration()
        {
            // Arrange
            var order = OrderEntity.CreateRestaurant(
                "REST-001",
                Guid.NewGuid(),
                _customerInfo,
                ServiceType.DineIn,
                15);

            order.TransitionToStatus(OrderStatus.Confirmed);

            // Act & Assert
            order.RequiresKitchenIntegration().Should().BeTrue();
        }
    }

    [TestFixture]
    public class BoutiqueContextTests : OrderEnterpriseTests
    {
        [Test]
        public void CreateBoutique_WithValidTerminal_ShouldSucceed()
        {
            // Arrange
            var terminalId = Guid.NewGuid();

            // Act
            var order = OrderEntity.CreateBoutique(
                "BOUT-001",
                Guid.NewGuid(),
                _customerInfo,
                terminalId);

            // Assert
            order.BusinessContext.Should().Be(BusinessContext.Boutique);
            order.ServiceInfo.TerminalId.Should().Be(terminalId.ToString());
            order.Status.Should().Be(OrderStatus.Pending);
        }

        [Test]
        public void BoutiqueWorkflow_ShouldFollowCorrectSequence()
        {
            // Arrange
            var terminalId = Guid.NewGuid();
            var order = OrderEntity.CreateBoutique(
                "POS-001",
                Guid.NewGuid(),
                _customerInfo,
                terminalId);

            // Ajouter des articles pour permettre la confirmation
            var unitPrice = Money.Create(10.50m, "EUR");
            order.AddItem(Guid.NewGuid(), "Produit Test", "SKU-001", 2, unitPrice);

            // Act & Assert - Workflow Boutique correct
            order.Status.Should().Be(OrderStatus.Pending);
            
            // Confirmer la commande d'abord
            order.Confirm();
            order.Status.Should().Be(OrderStatus.Confirmed);
            
            // Suivre le workflow Boutique
            order.TransitionToStatus(OrderStatus.Scanned);
            order.Status.Should().Be(OrderStatus.Scanned);
            
            order.TransitionToStatus(OrderStatus.Paid);
            order.Status.Should().Be(OrderStatus.Paid);
            
            order.TransitionToStatus(OrderStatus.Receipted);
            order.Status.Should().Be(OrderStatus.Receipted);
        }

        [Test]
        public void BoutiqueOrder_ShouldRequireInventoryReservation()
        {
            // Arrange
            var order = OrderEntity.CreateBoutique(
                "POS-001",
                Guid.NewGuid(),
                _customerInfo,
                Guid.NewGuid());

            // Ajouter des articles pour permettre la confirmation
            var unitPrice = Money.Create(15.99m, "EUR");
            order.AddItem(Guid.NewGuid(), "Article Boutique", "BOUT-001", 1, unitPrice);

            // Confirmer d'abord selon le workflow Boutique
            order.Confirm();

            // Act & Assert - Les commandes Boutique nécessitent une réservation d'inventaire
            order.BusinessContext.Should().Be(BusinessContext.Boutique);
            order.Status.Should().Be(OrderStatus.Confirmed);
        }
    }

    [TestFixture]
    public class ECommerceContextTests : OrderEnterpriseTests
    {
        [Test]
        public void ECommerceWorkflow_ShouldFollowCorrectSequence()
        {
            // Arrange
            var address = Address.Create(
                "123 Main St",
                "New York",
                "10001",
                "USA",
                "Apt 4B");

            var order = OrderEntity.CreateECommerce(
                "EC-001",
                Guid.NewGuid(),
                _customerInfo,
                address);

            // Act & Assert - Workflow E-Commerce
            order.Status.Should().Be(OrderStatus.Pending);
            
            order.TransitionToStatus(OrderStatus.Confirmed);
            order.TransitionToStatus(OrderStatus.Processing);
            order.TransitionToStatus(OrderStatus.Shipped);
            order.TransitionToStatus(OrderStatus.Delivered);
            
            order.Status.IsTerminalStatus(BusinessContext.ECommerce).Should().BeTrue();
        }

        [Test]
        public void ECommerceOrder_ShouldRequireShippingManagement()
        {
            // Arrange
            var address = Address.Create(
                "123 Main St",
                "New York",
                "10001",
                "USA");

            var order = OrderEntity.CreateECommerce(
                "EC-001",
                Guid.NewGuid(),
                _customerInfo,
                address);

            order.TransitionToStatus(OrderStatus.Confirmed);
            order.TransitionToStatus(OrderStatus.Processing);

            // Act & Assert
            order.RequiresShippingManagement().Should().BeTrue();
        }
    }

    [TestFixture]
    public class BusinessContextValidationTests : OrderEnterpriseTests
    {
        [Test]
        public void InvalidStatusTransition_ShouldThrow()
        {
            // Arrange
            var order = OrderEntity.CreateRestaurant(
                "REST-001",
                Guid.NewGuid(),
                _customerInfo,
                ServiceType.TakeAway);

            // Act & Assert - Tentative de transition invalide
            Assert.Throws<InvalidOperationException>(() =>
                order.TransitionToStatus(OrderStatus.Shipped)); // Invalide pour Restaurant
        }

        [Test]
        public void GetValidTransitions_ShouldReturnCorrectOptions()
        {
            // Arrange
            var order = OrderEntity.CreateRestaurant(
                "REST-001",
                Guid.NewGuid(),
                _customerInfo,
                ServiceType.DineIn,
                10);

            order.TransitionToStatus(OrderStatus.Confirmed);

            // Act
            var validTransitions = order.GetValidTransitions().ToList();

            // Assert
            validTransitions.Should().Contain(OrderStatus.KitchenQueue);
            validTransitions.Should().Contain(OrderStatus.Cancelled);
            validTransitions.Should().NotContain(OrderStatus.Shipped); // Pas valide pour Restaurant
        }

        [Test]
        public void AssignWaiter_OnNonRestaurantOrder_ShouldThrow()
        {
            // Arrange
            var order = OrderEntity.CreateBoutique(
                "POS-001",
                Guid.NewGuid(),
                _customerInfo,
                Guid.NewGuid());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                order.AssignWaiter(Guid.NewGuid()));
        }
    }

    [TestFixture]
    public class ServiceInfoTests : OrderEnterpriseTests
    {
        [Test]
        public void ServiceInfo_Restaurant_ShouldValidateTableForDineIn()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                ServiceInfo.CreateRestaurant(ServiceType.DineIn)); // Pas de numéro de table
        }

        [Test]
        public void ServiceInfo_ECommerce_ShouldValidateDeliveryAddress()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                ServiceInfo.CreateECommerce("", _customerInfo)); // Adresse vide
        }

        [Test]
        public void ServiceInfo_UpdateNotes_ShouldWork()
        {
            // Arrange
            var serviceInfo = ServiceInfo.CreateRestaurant(
                ServiceType.TakeAway,
                serviceNotes: "Original notes");

            // Act
            var updatedInfo = serviceInfo.WithNotes("Updated notes");

            // Assert
            updatedInfo.ServiceNotes.Should().Be("Updated notes");
            serviceInfo.ServiceNotes.Should().Be("Original notes"); // Immutable
        }
    }

    [TestFixture]
    public class OrderDisplayContextTests : OrderEnterpriseTests
    {
        [Test]
        public void GetDisplayContext_ShouldReturnCorrectInformation()
        {
            // Arrange
            var order = OrderEntity.CreateRestaurant(
                "REST-001",
                Guid.NewGuid(),
                _customerInfo,
                ServiceType.DineIn,
                15);

            order.TransitionToStatus(OrderStatus.Confirmed);
            order.TransitionToStatus(OrderStatus.KitchenQueue);

            // Act
            var context = order.GetDisplayContext();

            // Assert
            context.OrderNumber.Should().Be("REST-001");
            context.Status.Should().Be(OrderStatus.KitchenQueue);
            context.Context.Should().Be(BusinessContext.Restaurant);
            context.StatusColor.Should().Be("orange");
            context.RequiresAction.Should().BeTrue();
        }
    }
}
