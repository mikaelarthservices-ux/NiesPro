using NUnit.Framework;
using FluentAssertions;
using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Customer.Application.Features.Customers.Queries.GetCustomerById;
using Customer.Application.Common.Models;
using Customer.Domain.Interfaces;
using Customer.Domain.Aggregates.CustomerAggregate;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Customer.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for GetCustomerByIdQueryHandler following NiesPro Enterprise standards
    /// </summary>
    [TestFixture]
    public class GetCustomerByIdQueryHandlerTests
    {
        private IFixture _fixture;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<ILogsServiceClient> _mockLogsService;
        private Mock<ILogger<GetCustomerByIdQueryHandler>> _mockLogger;
        private GetCustomerByIdQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockLogsService = new Mock<ILogsServiceClient>();
            _mockLogger = new Mock<ILogger<GetCustomerByIdQueryHandler>>();

            _handler = new GetCustomerByIdQueryHandler(
                _mockCustomerRepository.Object,
                _mockLogsService.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WithExistingCustomerId_ShouldReturnCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerByIdQuery { CustomerId = customerId };

            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "1234567890",
                IsActive = true,
                CustomerType = "Regular",
                Addresses = new List<CustomerAddress>
                {
                    new CustomerAddress
                    {
                        Id = Guid.NewGuid(),
                        Type = "Home",
                        AddressLine1 = "123 Main St",
                        City = "Test City",
                        PostalCode = "12345",
                        Country = "Test Country",
                        IsDefault = true,
                        CustomerId = customerId
                    }
                }
            };

            _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(customerId);
            result.Data.FirstName.Should().Be("John");
            result.Data.LastName.Should().Be("Doe");
            result.Data.Email.Should().Be("john.doe@example.com");
            result.Data.Addresses.Should().HaveCount(1);
            result.Data.Addresses.First().AddressType.Should().Be("Home");

            _mockCustomerRepository.Verify(x => x.GetByIdAsync(customerId), Times.Once);
        }

        [Test]
        public async Task Handle_WithNonExistentCustomerId_ShouldReturnNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerByIdQuery { CustomerId = customerId };

            _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync((Customer.Domain.Aggregates.CustomerAggregate.Customer)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Customer not found");

            _mockCustomerRepository.Verify(x => x.GetByIdAsync(customerId), Times.Once);
        }

        [Test]
        public async Task Handle_WhenRepositoryThrows_ShouldReturnErrorResponse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerByIdQuery { CustomerId = customerId };

            _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("An error occurred while retrieving customer");

            _mockCustomerRepository.Verify(x => x.GetByIdAsync(customerId), Times.Once);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldLogInformation()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerByIdQuery { CustomerId = customerId };

            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                IsActive = true,
                Addresses = new List<CustomerAddress>()
            };

            _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            // Verify logging calls
            _mockLogsService.Verify(x => x.LogInformationAsync(
                It.Is<string>(s => s.Contains("Retrieving customer")),
                It.IsAny<Dictionary<string, object>>()), Times.Once);

            _mockLogsService.Verify(x => x.LogInformationAsync(
                It.Is<string>(s => s.Contains("Customer retrieved successfully")),
                It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithInvalidGuid_ShouldReturnNotFound()
        {
            // Arrange
            var query = new GetCustomerByIdQuery { CustomerId = Guid.Empty };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid customer ID");

            _mockCustomerRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}