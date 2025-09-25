using AutoFixture;
using Catalog.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Catalog.Tests.Unit.Domain
{
    /// <summary>
    /// Unit tests for Brand entity
    /// Validates brand business logic and computed properties
    /// </summary>
    [TestFixture]
    public class BrandTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            
            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public void Brand_ShouldBeCreatedWithValidProperties()
        {
            // Arrange & Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = "Apple",
                Description = "Technology company",
                LogoUrl = "https://example.com/apple-logo.png",
                Website = "https://apple.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            brand.Id.Should().NotBe(Guid.Empty);
            brand.Name.Should().Be("Apple");
            brand.Description.Should().Be("Technology company");
            brand.LogoUrl.Should().Be("https://example.com/apple-logo.png");
            brand.Website.Should().Be("https://apple.com");
            brand.IsActive.Should().BeTrue();
            brand.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void Brand_ShouldAllowNullableProperties()
        {
            // Arrange & Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = "Simple Brand",
                Description = null,
                LogoUrl = null,
                Website = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            brand.Description.Should().BeNull();
            brand.LogoUrl.Should().BeNull();
            brand.Website.Should().BeNull();
            brand.Name.Should().NotBeNull();
        }

        [Test]
        public void Brand_WithProducts_ShouldMaintainProductCollection()
        {
            // Arrange
            var brand = _fixture.Build<Brand>()
                .With(x => x.IsActive, true)
                .Without(x => x.Products)
                .Create();

            var products = _fixture.CreateMany<Product>(3).ToList();
            
            // Act
            brand.Products = products;

            // Assert
            brand.Products.Should().NotBeNull();
            brand.Products.Should().HaveCount(3);
            brand.Products.Should().ContainInOrder(products);
        }

        [Test]
        public void Brand_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var brand = new Brand();

            // Assert
            brand.Id.Should().NotBe(Guid.Empty); // BaseEntity auto-generates ID
            brand.Name.Should().BeEmpty(); // String property defaults to empty string
            brand.IsActive.Should().BeTrue(); // Brand defaults to active
            brand.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10)); // BaseEntity auto-sets CreatedAt
            brand.Products.Should().NotBeNull().And.BeEmpty(); // Collection is initialized as empty list
            brand.Slug.Should().BeEmpty(); // Slug defaults to empty string
        }

        [Test]
        public void Brand_ShouldHandleEmptyProductCollection()
        {
            // Arrange
            var brand = _fixture.Build<Brand>()
                .With(x => x.IsActive, true)
                .Without(x => x.Products)
                .Create();

            // Act
            brand.Products = new List<Product>();

            // Assert
            brand.Products.Should().NotBeNull();
            brand.Products.Should().BeEmpty();
        }

        [Test]
        public void Brand_WithLongName_ShouldAcceptValue()
        {
            // Arrange
            var longName = new string('A', 200);
            
            // Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = longName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            brand.Name.Should().Be(longName);
            brand.Name.Length.Should().Be(200);
        }

        [Test]
        public void Brand_WithLongDescription_ShouldAcceptValue()
        {
            // Arrange
            var longDescription = new string('B', 1000);
            
            // Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = "Test Brand",
                Description = longDescription,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            brand.Description.Should().Be(longDescription);
            brand.Description.Length.Should().Be(1000);
        }

        [Test]
        public void Brand_ShouldSupportActiveInactiveStates()
        {
            // Arrange
            var activeBrand = _fixture.Build<Brand>()
                .With(x => x.IsActive, true)
                .Without(x => x.Products)
                .Create();

            var inactiveBrand = _fixture.Build<Brand>()
                .With(x => x.IsActive, false)
                .Without(x => x.Products)
                .Create();

            // Assert
            activeBrand.IsActive.Should().BeTrue();
            inactiveBrand.IsActive.Should().BeFalse();
        }

        [Test]
        public void Brand_WithSpecialCharactersInName_ShouldAcceptValue()
        {
            // Arrange & Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = "Brand & Co. (2024)",
                Description = "Brand with special characters: @#$%^&*()",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            brand.Name.Should().Be("Brand & Co. (2024)");
            brand.Description.Should().Be("Brand with special characters: @#$%^&*()");
        }

        [Test]
        public void Brand_ShouldHandleTimestamps()
        {
            // Arrange
            var createdTime = DateTime.UtcNow.AddDays(-30);
            var updatedTime = DateTime.UtcNow;

            // Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = "Time Test Brand",
                IsActive = true,
                CreatedAt = createdTime,
                UpdatedAt = updatedTime
            };

            // Assert
            brand.CreatedAt.Should().Be(createdTime);
            brand.UpdatedAt.Should().Be(updatedTime);
            brand.UpdatedAt.Should().BeAfter(brand.CreatedAt);
        }

        [Test]
        public void Brand_ShouldAllowUrlFormatValidation()
        {
            // Arrange
            var validLogoUrl = "https://cdn.example.com/brands/logo.png";
            var validWebsiteUrl = "https://www.brandwebsite.com";

            // Act
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = "URL Test Brand",
                LogoUrl = validLogoUrl,
                Website = validWebsiteUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            brand.LogoUrl.Should().Be(validLogoUrl);
            brand.Website.Should().Be(validWebsiteUrl);
            
            // Basic URL format validation (could be extended with proper URL validation)
            brand.LogoUrl.Should().StartWith("https://");
            brand.Website.Should().StartWith("https://");
        }
    }
}