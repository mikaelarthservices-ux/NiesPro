using Catalog.Application.DTOs;
using Catalog.Application.Features.Products.Commands.CreateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NiesPro.Contracts.Common;
using NUnit.Framework;

namespace Catalog.Tests.Unit.Application
{
    [TestFixture]
    public class CreateProductCommandHandlerTests
    {
        private Mock<IProductRepository> _productRepositoryMock;
        private Mock<ICategoryRepository> _categoryRepositoryMock;
        private Mock<IBrandRepository> _brandRepositoryMock;
        private Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
        private CreateProductCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _brandRepositoryMock = new Mock<IBrandRepository>();
            _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();
            var logsServiceMock = new Mock<NiesPro.Logging.Client.ILogsServiceClient>();

            _handler = new CreateProductCommandHandler(
                _productRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _brandRepositoryMock.Object,
                _loggerMock.Object,
                logsServiceMock.Object);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldCreateProductSuccessfully()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Description = "Test description",
                Price = 99.99m,
                CategoryId = categoryId,
                IsActive = true
            };

            var category = new Category
            {
                Id = categoryId,
                Name = "Test Category",
                IsActive = true
            };

            var createdProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                SKU = command.SKU,
                Description = command.Description,
                Price = command.Price,
                CategoryId = command.CategoryId,
                IsActive = command.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // Setup mocks
            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _productRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be(command.Name);
            result.Data.SKU.Should().Be(command.SKU);
            result.Data.Price.Should().Be(command.Price);

            // Verify repository calls
            _productRepositoryMock.Verify(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
            _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithDuplicateSKU_ShouldReturnError()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "DUPLICATE001",
                Price = 99.99m,
                CategoryId = Guid.NewGuid()
            };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("A product with this SKU already exists");
            result.Data.Should().BeNull();

            // Verify only SKU check was called
            _productRepositoryMock.Verify(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithInvalidCategory_ShouldReturnError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Price = 99.99m,
                CategoryId = categoryId
            };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Category not found or inactive");
            result.Data.Should().BeNull();

            // Verify calls
            _productRepositoryMock.Verify(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
            _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithInactiveCategory_ShouldReturnError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Price = 99.99m,
                CategoryId = categoryId
            };

            var inactiveCategory = new Category
            {
                Id = categoryId,
                Name = "Inactive Category",
                IsActive = false
            };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveCategory);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Category not found or inactive");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithBrandId_ShouldValidateBrand()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var brandId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Price = 99.99m,
                CategoryId = categoryId,
                BrandId = brandId
            };

            var category = new Category { Id = categoryId, IsActive = true };
            var brand = new Brand { Id = brandId, IsActive = true };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _brandRepositoryMock
                .Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand);

            _productRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = Guid.NewGuid(), Name = command.Name });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            // Verify brand validation was called
            _brandRepositoryMock.Verify(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithInvalidBrand_ShouldReturnError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var brandId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Price = 99.99m,
                CategoryId = categoryId,
                BrandId = brandId
            };

            var category = new Category { Id = categoryId, IsActive = true };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _brandRepositoryMock
                .Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Brand?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Brand not found or inactive");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Price = 99.99m,
                CategoryId = categoryId
            };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("An error occurred while creating the product");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithInactiveBrand_ShouldReturnError()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var brandId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                SKU = "TEST001",
                Price = 99.99m,
                CategoryId = categoryId,
                BrandId = brandId
            };

            var category = new Category { Id = categoryId, Name = "Test Category", IsActive = true };
            var inactiveBrand = new Brand { Id = brandId, Name = "Inactive Brand", IsActive = false };

            _productRepositoryMock
                .Setup(x => x.IsSkuUniqueAsync(command.SKU, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepositoryMock
                .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _brandRepositoryMock
                .Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveBrand);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Brand not found or inactive");
            result.Data.Should().BeNull();

            // Verify brand validation was called
            _brandRepositoryMock.Verify(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()), Times.Once);
            _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}