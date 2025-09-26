using AutoFixture;
using Catalog.Application.DTOs;
using Catalog.Application.Features.Categories.Queries.GetCategories;
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
    /// Unit tests for GetCategoriesQueryHandler
    /// Tests the existing handler with real interface methods
    /// </summary>
    [TestFixture]
    public class GetCategoriesQueryHandlerTests
    {
        private Mock<ICategoryRepository> _categoryRepositoryMock;
        private Mock<ILogger<GetCategoriesQueryHandler>> _loggerMock;
        private GetCategoriesQueryHandler _handler;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<GetCategoriesQueryHandler>>();
            var logsServiceMock = new Mock<NiesPro.Logging.Client.ILogsServiceClient>();
            _handler = new GetCategoriesQueryHandler(_categoryRepositoryMock.Object, _loggerMock.Object, logsServiceMock.Object);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task Handle_WithRootOnlyTrue_ShouldCallGetRootCategories()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = true,
                IncludeInactive = false
            };

            var categories = _fixture.Build<Category>()
                .With(x => x.IsActive, true)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .CreateMany(3)
                .ToList();

            _categoryRepositoryMock
                .Setup(x => x.GetRootCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(3);

            // Verify correct repository method was called
            _categoryRepositoryMock.Verify(x => x.GetRootCategoriesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithRootOnlyFalse_ShouldCallGetAllCategories()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = false,
                IncludeInactive = false
            };

            var categories = _fixture.Build<Category>()
                .With(x => x.IsActive, true)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .CreateMany(5)
                .ToList();

            _categoryRepositoryMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(5);

            // Verify correct repository method was called
            _categoryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetRootCategoriesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithIncludeInactiveTrue_ShouldReturnAllCategories()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = false,
                IncludeInactive = true
            };

            var activeCategories = _fixture.Build<Category>()
                .With(x => x.IsActive, true)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .CreateMany(3);

            var inactiveCategories = _fixture.Build<Category>()
                .With(x => x.IsActive, false)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .CreateMany(2);

            var allCategories = activeCategories.Concat(inactiveCategories).ToList();

            _categoryRepositoryMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(allCategories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(5); // All categories (active + inactive)
        }

        [Test]
        public async Task Handle_WithIncludeInactiveFalse_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = false,
                IncludeInactive = false
            };

            var activeCategories = _fixture.Build<Category>()
                .With(x => x.IsActive, true)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .CreateMany(3);

            var inactiveCategories = _fixture.Build<Category>()
                .With(x => x.IsActive, false)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .CreateMany(2);

            var allCategories = activeCategories.Concat(inactiveCategories).ToList();

            _categoryRepositoryMock
                .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(allCategories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(3); // Only active categories
            result.Data.Should().OnlyContain(c => c.IsActive);
        }

        [Test]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnError()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = true,
                IncludeInactive = false
            };

            _categoryRepositoryMock
                .Setup(x => x.GetRootCategoriesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("error");
            result.Data.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithValidCategories_ShouldMapToDtosCorrectly()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = true,
                IncludeInactive = false
            };

            var category = _fixture.Build<Category>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Name, "Electronics")
                .With(x => x.Description, "Electronic devices")
                .With(x => x.IsActive, true)
                .Without(x => x.ParentCategory)
                .Without(x => x.SubCategories)
                .Without(x => x.Products)
                .Create();

            var categories = new List<Category> { category };

            _categoryRepositoryMock
                .Setup(x => x.GetRootCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);

            var dto = result.Data.First();
            dto.Id.Should().Be(category.Id);
            dto.Name.Should().Be(category.Name);
            dto.Description.Should().Be(category.Description);
            dto.IsActive.Should().Be(category.IsActive);
        }

        [Test]
        public async Task Handle_WithEmptyResult_ShouldReturnEmptyCollection()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                RootOnly = true,
                IncludeInactive = false
            };

            _categoryRepositoryMock
                .Setup(x => x.GetRootCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }
    }
}