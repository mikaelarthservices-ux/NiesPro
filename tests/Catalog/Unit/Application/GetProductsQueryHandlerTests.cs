using AutoFixture;
using Catalog.Application.DTOs;
using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NiesPro.Contracts.Common;
using NUnit.Framework;

namespace Catalog.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for GetProductsQueryHandler
    /// Tests the existing handler with real interface methods
    /// </summary>
    [TestFixture]
    public class GetProductsQueryHandlerTests
    {
        private Mock<IProductRepository> _productRepositoryMock;
        private Mock<ILogger<GetProductsQueryHandler>> _loggerMock;
        private GetProductsQueryHandler _handler;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _loggerMock = new Mock<ILogger<GetProductsQueryHandler>>();
            var logsServiceMock = new Mock<NiesPro.Logging.Client.ILogsServiceClient>();
            _handler = new GetProductsQueryHandler(_productRepositoryMock.Object, _loggerMock.Object, logsServiceMock.Object);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task Handle_WithValidQuery_ShouldReturnProducts()
        {
            // Arrange
            var query = new GetProductsQuery
            {
                Page = 1,
                PageSize = 10
            };

            var products = _fixture.CreateMany<Product>(5).ToList();
            var totalCount = 5;

            _productRepositoryMock
                .Setup(x => x.GetPagedProductsAsync(
                    query.Page, 
                    query.PageSize,
                    null, // category
                    null, // brand
                    null, // minPrice
                    null, // maxPrice
                    query.SortBy,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((products, totalCount));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Items.Should().HaveCount(5);
            result.Data.TotalCount.Should().Be(totalCount);

            // Verify repository call
            _productRepositoryMock.Verify(x => x.GetPagedProductsAsync(
                query.Page, query.PageSize, null, null, null, null, query.SortBy, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithCategoryFilter_ShouldPassCorrectParameters()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var query = new GetProductsQuery
            {
                Page = 1,
                PageSize = 20,
                CategoryId = categoryId
            };

            var products = new List<Product>();
            var totalCount = 0;

            _productRepositoryMock
                .Setup(x => x.GetPagedProductsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((products, totalCount));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            
            // Verify category ID was converted to string
            _productRepositoryMock.Verify(x => x.GetPagedProductsAsync(
                query.Page, query.PageSize, categoryId.ToString(), null, null, null, query.SortBy, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithPriceRange_ShouldPassCorrectParameters()
        {
            // Arrange
            var query = new GetProductsQuery
            {
                Page = 1,
                PageSize = 10,
                MinPrice = 50.00m,
                MaxPrice = 200.00m
            };

            var products = new List<Product>();
            var totalCount = 0;

            _productRepositoryMock
                .Setup(x => x.GetPagedProductsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((products, totalCount));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            
            _productRepositoryMock.Verify(x => x.GetPagedProductsAsync(
                query.Page, query.PageSize, null, null, 50.00m, 200.00m, query.SortBy, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnError()
        {
            // Arrange
            var query = new GetProductsQuery
            {
                Page = 1,
                PageSize = 10
            };

            _productRepositoryMock
                .Setup(x => x.GetPagedProductsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("error");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithAllFilters_ShouldPassAllParameters()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var brandId = Guid.NewGuid();
            var query = new GetProductsQuery
            {
                Page = 2,
                PageSize = 15,
                CategoryId = categoryId,
                BrandId = brandId,
                MinPrice = 25.00m,
                MaxPrice = 150.00m,
                SortBy = "price"
            };

            var products = new List<Product>();
            var totalCount = 0;

            _productRepositoryMock
                .Setup(x => x.GetPagedProductsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((products, totalCount));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            
            _productRepositoryMock.Verify(x => x.GetPagedProductsAsync(
                2, 15, categoryId.ToString(), brandId.ToString(), 25.00m, 150.00m, "price", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithDefaultValues_ShouldUseQueryDefaults()
        {
            // Arrange
            var query = new GetProductsQuery(); // Uses default values

            var products = new List<Product>();
            var totalCount = 0;

            _productRepositoryMock
                .Setup(x => x.GetPagedProductsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((products, totalCount));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            
            // Verify default values are used (Page=1, PageSize=20, SortBy="name")
            _productRepositoryMock.Verify(x => x.GetPagedProductsAsync(
                1, 20, null, null, null, null, "name", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}