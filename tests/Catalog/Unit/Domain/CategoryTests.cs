using AutoFixture;
using AutoFixture.NUnit3;
using Catalog.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Catalog.Tests.Unit.Domain
{
    [TestFixture]
    public class CategoryTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Category_Creation_ShouldSetBasicProperties()
        {
            // Arrange
            var name = "Electronics";
            var description = "Electronic devices and accessories";
            var slug = "electronics";

            // Act
            var category = new Category
            {
                Name = name,
                Description = description,
                Slug = slug
            };

            // Assert
            category.Name.Should().Be(name);
            category.Description.Should().Be(description);
            category.Slug.Should().Be(slug);
            category.IsActive.Should().BeTrue(); // Default value
            category.SortOrder.Should().Be(0); // Default value
        }

        [Test]
        public void HasSubCategories_WhenNoSubCategories_ShouldReturnFalse()
        {
            // Arrange
            var category = new Category();

            // Act & Assert
            category.HasSubCategories.Should().BeFalse();
        }

        [Test]
        public void HasSubCategories_WithSubCategories_ShouldReturnTrue()
        {
            // Arrange
            var category = new Category();
            var subCategory = new Category
            {
                Name = "Smartphones",
                ParentCategoryId = category.Id
            };
            category.SubCategories.Add(subCategory);

            // Act & Assert
            category.HasSubCategories.Should().BeTrue();
        }

        [Test]
        public void ProductCount_WhenNoProducts_ShouldReturnZero()
        {
            // Arrange
            var category = new Category();

            // Act & Assert
            category.ProductCount.Should().Be(0);
        }

        [Test]
        public void ProductCount_WithActiveProducts_ShouldReturnCorrectCount()
        {
            // Arrange
            var category = new Category();
            category.Products.Add(new Product { Name = "Product 1", IsActive = true });
            category.Products.Add(new Product { Name = "Product 2", IsActive = false });
            category.Products.Add(new Product { Name = "Product 3", IsActive = true });

            // Act & Assert
            category.ProductCount.Should().Be(2);
        }

        [Test]
        public void Category_HierarchicalStructure_ShouldWorkCorrectly()
        {
            // Arrange
            var parentCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics"
            };

            var childCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Smartphones",
                ParentCategoryId = parentCategory.Id,
                ParentCategory = parentCategory
            };

            parentCategory.SubCategories.Add(childCategory);

            // Act & Assert
            childCategory.ParentCategoryId.Should().Be(parentCategory.Id);
            childCategory.ParentCategory.Should().Be(parentCategory);
            parentCategory.SubCategories.Should().Contain(childCategory);
            parentCategory.HasSubCategories.Should().BeTrue();
        }

        [Test]
        public void Category_SortOrder_ShouldAllowCustomOrdering()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Category C", SortOrder = 3 },
                new Category { Name = "Category A", SortOrder = 1 },
                new Category { Name = "Category B", SortOrder = 2 }
            };

            // Act
            var sortedCategories = categories.OrderBy(c => c.SortOrder).ToList();

            // Assert
            sortedCategories[0].Name.Should().Be("Category A");
            sortedCategories[1].Name.Should().Be("Category B");
            sortedCategories[2].Name.Should().Be("Category C");
        }

        [Test, AutoData]
        public void Category_WithAutoFixture_ShouldCreateValidCategory(
            string name,
            string description)
        {
            // Arrange & Act
            var category = new Category
            {
                Name = name,
                Description = description,
                Slug = name?.ToLower().Replace(" ", "-") ?? "default-slug"
            };

            // Assert
            category.Name.Should().NotBeNullOrEmpty();
            category.Description.Should().NotBeNullOrEmpty();
            category.Slug.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Category_Slug_ShouldBeUrlFriendly()
        {
            // Arrange
            var category = new Category
            {
                Name = "Consumer Electronics & Gadgets",
                Slug = "consumer-electronics-gadgets"
            };

            // Act & Assert
            category.Slug.Should().NotContain(" ");
            category.Slug.Should().NotContain("&");
            category.Slug.Should().Be("consumer-electronics-gadgets");
        }
    }
}