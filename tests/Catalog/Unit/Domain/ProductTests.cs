using AutoFixture;
using AutoFixture.NUnit3;
using Catalog.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Catalog.Tests.Unit.Domain
{
    [TestFixture]
    public class ProductTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Product_Creation_ShouldSetBasicProperties()
        {
            // Arrange
            var name = "Test Product";
            var sku = "TEST001";
            var price = 99.99m;
            var categoryId = Guid.NewGuid();

            // Act
            var product = new Product
            {
                Name = name,
                SKU = sku,
                Price = price,
                CategoryId = categoryId
            };

            // Assert
            product.Name.Should().Be(name);
            product.SKU.Should().Be(sku);
            product.Price.Should().Be(price);
            product.CategoryId.Should().Be(categoryId);
            product.IsActive.Should().BeTrue(); // Default value
            product.IsFeatured.Should().BeFalse(); // Default value
            product.TrackQuantity.Should().BeTrue(); // Default value
            product.Quantity.Should().Be(0); // Default value
        }

        [Test]
        public void IsInStock_WhenTrackQuantityFalse_ShouldReturnTrue()
        {
            // Arrange
            var product = new Product
            {
                TrackQuantity = false,
                Quantity = 0
            };

            // Act & Assert
            product.IsInStock.Should().BeTrue();
        }

        [Test]
        public void IsInStock_WhenTrackQuantityTrueAndQuantityZero_ShouldReturnFalse()
        {
            // Arrange
            var product = new Product
            {
                TrackQuantity = true,
                Quantity = 0
            };

            // Act & Assert
            product.IsInStock.Should().BeFalse();
        }

        [Test]
        public void IsInStock_WhenTrackQuantityTrueAndQuantityPositive_ShouldReturnTrue()
        {
            // Arrange
            var product = new Product
            {
                TrackQuantity = true,
                Quantity = 10
            };

            // Act & Assert
            product.IsInStock.Should().BeTrue();
        }

        [Test]
        public void IsLowStock_WhenThresholdSetAndQuantityBelowThreshold_ShouldReturnTrue()
        {
            // Arrange
            var product = new Product
            {
                TrackQuantity = true,
                Quantity = 3,
                LowStockThreshold = 5
            };

            // Act & Assert
            product.IsLowStock.Should().BeTrue();
        }

        [Test]
        public void IsLowStock_WhenThresholdSetAndQuantityAboveThreshold_ShouldReturnFalse()
        {
            // Arrange
            var product = new Product
            {
                TrackQuantity = true,
                Quantity = 10,
                LowStockThreshold = 5
            };

            // Act & Assert
            product.IsLowStock.Should().BeFalse();
        }

        [Test]
        public void IsLowStock_WhenThresholdNotSet_ShouldReturnFalse()
        {
            // Arrange
            var product = new Product
            {
                TrackQuantity = true,
                Quantity = 1,
                LowStockThreshold = null
            };

            // Act & Assert
            product.IsLowStock.Should().BeFalse();
        }

        [Test]
        public void DiscountPercentage_WhenComparePriceHigherThanPrice_ShouldCalculateCorrectPercentage()
        {
            // Arrange
            var product = new Product
            {
                Price = 80m,
                ComparePrice = 100m
            };

            // Act & Assert
            product.DiscountPercentage.Should().Be(20m);
        }

        [Test]
        public void DiscountPercentage_WhenComparePriceLowerOrEqualToPrice_ShouldReturnNull()
        {
            // Arrange
            var product = new Product
            {
                Price = 100m,
                ComparePrice = 80m
            };

            // Act & Assert
            product.DiscountPercentage.Should().BeNull();
        }

        [Test]
        public void DiscountPercentage_WhenComparePriceNotSet_ShouldReturnNull()
        {
            // Arrange
            var product = new Product
            {
                Price = 100m,
                ComparePrice = null
            };

            // Act & Assert
            product.DiscountPercentage.Should().BeNull();
        }

        [Test]
        public void AverageRating_WhenNoReviews_ShouldReturnZero()
        {
            // Arrange
            var product = new Product();

            // Act & Assert
            product.AverageRating.Should().Be(0);
        }

        [Test]
        public void ReviewCount_WhenNoApprovedReviews_ShouldReturnZero()
        {
            // Arrange
            var product = new Product();
            product.Reviews.Add(new Review { Rating = 5, IsApproved = false });
            product.Reviews.Add(new Review { Rating = 4, IsApproved = false });

            // Act & Assert
            product.ReviewCount.Should().Be(0);
        }

        [Test]
        public void ReviewCount_WithApprovedReviews_ShouldReturnCorrectCount()
        {
            // Arrange
            var product = new Product();
            product.Reviews.Add(new Review { Rating = 5, IsApproved = true });
            product.Reviews.Add(new Review { Rating = 4, IsApproved = false });
            product.Reviews.Add(new Review { Rating = 3, IsApproved = true });

            // Act & Assert
            product.ReviewCount.Should().Be(2);
        }

        [Test, AutoData]
        public void Product_WithAutoFixture_ShouldCreateValidProduct(
            string name, 
            string sku, 
            decimal price)
        {
            // Arrange & Act
            var product = new Product
            {
                Name = name,
                SKU = sku,
                Price = Math.Abs(price), // Ensure positive price
                CategoryId = Guid.NewGuid()
            };

            // Assert
            product.Name.Should().NotBeNullOrEmpty();
            product.SKU.Should().NotBeNullOrEmpty();
            product.Price.Should().BeGreaterOrEqualTo(0);
            product.CategoryId.Should().NotBeEmpty();
        }
    }
}