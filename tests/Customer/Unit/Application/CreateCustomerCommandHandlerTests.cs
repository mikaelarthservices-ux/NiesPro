using NUnit.Framework;
using FluentAssertions;
using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Customer.Application.Features.Customers.Commands.CreateCustomer;
using Customer.Application.Common.Models;
using Customer.Domain.Interfaces;
using Customer.Domain.Aggregates.CustomerAggregate;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Customer.Tests.Unit.Application
{
    /// <summary>
    /// Unit tests for CreateCustomerCommandHandler following NiesPro Enterprise standards
    /// </summary>
    [TestFixture]
    public class CreateCustomerCommandHandlerTests
    {
        private IFixture _fixture;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ILogsServiceClient> _mockLogsService;
        private Mock<IAuditServiceClient> _mockAuditService;
        private Mock<ILogger<CreateCustomerCommandHandler>> _mockLogger;
        private CreateCustomerCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogsService = new Mock<ILogsServiceClient>();
            _mockAuditService = new Mock<IAuditServiceClient>();
            _mockLogger = new Mock<ILogger<CreateCustomerCommandHandler>>();

            _handler = new CreateCustomerCommandHandler(
                _mockCustomerRepository.Object,
                _mockUnitOfWork.Object,
                _mockLogsService.Object,
                _mockAuditService.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldCreateCustomerSuccessfully()
        {
            // Arrange
            var command = _fixture.Build<CreateCustomerCommand>()
                .With(x => x.Email, "test@example.com")
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, "Doe")
                .With(x => x.Phone, "1234567890")
                .With(x => x.CustomerType, "Regular")
                .With(x => x.Addresses, new List<CreateCustomerAddressRequest>
                {
                    new CreateCustomerAddressRequest
                    {
                        AddressType = "Home",
                        Street = "123 Main St",
                        City = "Test City",
                        PostalCode = "12345",
                        Country = "Test Country",
                        IsDefault = true
                    }
                })
                .Create();

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync((Customer.Domain.Aggregates.CustomerAggregate.Customer)null!);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(command.Email);
            result.Data.FirstName.Should().Be(command.FirstName);
            result.Data.LastName.Should().Be(command.LastName);
            result.Data.Addresses.Should().HaveCount(1);
            result.Data.Addresses.First().AddressType.Should().Be("Home");

            _mockCustomerRepository.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
            _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer.Domain.Aggregates.CustomerAggregate.Customer>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = _fixture.Build<CreateCustomerCommand>()
                .With(x => x.Email, "existing@example.com")
                .With(x => x.Addresses, new List<CreateCustomerAddressRequest>())
                .Create();

            var existingCustomer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                FirstName = "Existing",
                LastName = "Customer"
            };

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(existingCustomer);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Customer with this email already exists");

            _mockCustomerRepository.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
            _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer.Domain.Aggregates.CustomerAggregate.Customer>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithMultipleAddresses_ShouldCreateCustomerWithAllAddresses()
        {
            // Arrange
            var command = _fixture.Build<CreateCustomerCommand>()
                .With(x => x.Email, "test@example.com")
                .With(x => x.Addresses, new List<CreateCustomerAddressRequest>
                {
                    new CreateCustomerAddressRequest
                    {
                        AddressType = "Home",
                        Street = "123 Home St",
                        City = "Home City",
                        PostalCode = "12345",
                        Country = "Home Country",
                        IsDefault = true
                    },
                    new CreateCustomerAddressRequest
                    {
                        AddressType = "Work",
                        Street = "456 Work Ave",
                        City = "Work City",
                        PostalCode = "67890",
                        Country = "Work Country",
                        IsDefault = false
                    }
                })
                .Create();

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync((Customer.Domain.Aggregates.CustomerAggregate.Customer)null!);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Addresses.Should().HaveCount(2);
            result.Data.Addresses.Should().Contain(a => a.AddressType == "Home" && a.IsDefault);
            result.Data.Addresses.Should().Contain(a => a.AddressType == "Work" && !a.IsDefault);

            _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer.Domain.Aggregates.CustomerAggregate.Customer>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WhenRepositoryThrows_ShouldReturnErrorResponse()
        {
            // Arrange
            var command = _fixture.Build<CreateCustomerCommand>()
                .With(x => x.Email, "test@example.com")
                .With(x => x.Addresses, new List<CreateCustomerAddressRequest>())
                .Create();

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync((Customer.Domain.Aggregates.CustomerAggregate.Customer)null!);

            _mockCustomerRepository.Setup(x => x.AddAsync(It.IsAny<Customer.Domain.Aggregates.CustomerAggregate.Customer>()))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("An error occurred while creating customer");

            _mockCustomerRepository.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
            _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer.Domain.Aggregates.CustomerAggregate.Customer>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldLogInformationAndAudit()
        {
            // Arrange
            var command = _fixture.Build<CreateCustomerCommand>()
                .With(x => x.Email, "test@example.com")
                .With(x => x.FirstName, "John")
                .With(x => x.LastName, "Doe")
                .With(x => x.Addresses, new List<CreateCustomerAddressRequest>())
                .Create();

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync((Customer.Domain.Aggregates.CustomerAggregate.Customer)null!);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            // Verify logging calls
            _mockLogsService.Verify(x => x.LogInformationAsync(
                It.Is<string>(s => s.Contains("Creating customer")),
                It.IsAny<Dictionary<string, object>>()), Times.Once);

            _mockLogsService.Verify(x => x.LogInformationAsync(
                It.Is<string>(s => s.Contains("Customer created successfully")),
                It.IsAny<Dictionary<string, object>>()), Times.Once);

            // Verify audit trail
            _mockAuditService.Verify(x => x.AuditCreateAsync(
                It.Is<string>(s => s == "System"),
                It.Is<string>(s => s == "System"),
                It.Is<string>(s => s == "Customer"),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}