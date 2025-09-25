using NUnit.Framework;
using FluentAssertions;
using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Users.Commands.RegisterUser;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Domain.Entities;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Auth.Tests.Unit.Application;

/// <summary>
/// Unit tests for RegisterUserCommandHandler business logic
/// </summary>
[TestFixture]
public class RegisterUserCommandHandlerTests
{
    private IFixture _fixture;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IRoleRepository> _mockRoleRepository;
    private Mock<IDeviceRepository> _mockDeviceRepository;
    private Mock<IPasswordService> _mockPasswordService;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<RegisterUserCommandHandler>> _mockLogger;
    private Mock<ILogsServiceClient> _mockLogsService;
    private Mock<IAuditServiceClient> _mockAuditService;
    private RegisterUserCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockDeviceRepository = new Mock<IDeviceRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RegisterUserCommandHandler>>();
        _mockLogsService = new Mock<ILogsServiceClient>();
        _mockAuditService = new Mock<IAuditServiceClient>();

        _handler = new RegisterUserCommandHandler(
            _mockUserRepository.Object,
            _mockRoleRepository.Object,
            _mockDeviceRepository.Object,
            _mockPasswordService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockLogsService.Object,
            _mockAuditService.Object
        );
    }

    [Test]
    public async Task Handle_WithValidRequest_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(c => c.Email, "test@example.com")
            .With(c => c.Username, "testuser")
            .With(c => c.Password, "SecurePassword123!")
            .Create();

        var hashedPassword = "hashed_password";
        var defaultRole = _fixture.Build<Role>()
            .With(r => r.Name, "User")
            .Create();

        var createdUser = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.Username, command.Username)
            .With(u => u.PasswordHash, hashedPassword)
            .Create();

        // Setup mocks
        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService.Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockRoleRepository.Setup(x => x.GetByNameAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultRole);

        _mockDeviceRepository.Setup(x => x.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<Device>());

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email);
        result.Data.Username.Should().Be(command.Username);

        // Verify interactions
        _mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.Email == command.Email && 
            u.Username == command.Username &&
            u.PasswordHash == hashedPassword), It.IsAny<CancellationToken>()), Times.Once);

        _mockPasswordService.Verify(x => x.HashPassword(command.Password), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify NiesPro Logging integration
        _mockAuditService.Verify(x => x.AuditCreateAsync(
            It.IsAny<string>(), // userId
            It.IsAny<string>(), // userName
            "User",             // entityName
            It.IsAny<string>(), // entityId
            It.IsAny<Dictionary<string, object>>()), // metadata
            Times.Once);
            
        _mockLogsService.Verify(x => x.LogAsync(
            LogLevel.Information,
            It.Is<string>(s => s.Contains("User registration successful")),
            null, // exception
            It.IsAny<Dictionary<string, object>>()), // properties
            Times.Once);
    }

    [Test]
    public async Task Handle_WithExistingEmail_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(c => c.Email, "existing@example.com")
            .Create();

        var existingUser = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email is already registered");

        // Verify no user creation was attempted
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockPasswordService.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithExistingUsername_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(c => c.Username, "existinguser")
            .Create();

        var existingUser = _fixture.Build<User>()
            .With(u => u.Username, command.Username)
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Username is already taken");

        // Verify no user creation was attempted
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenPasswordServiceFails_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Create<RegisterUserCommand>();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService.Setup(x => x.HashPassword(command.Password))
            .Throws(new InvalidOperationException("Password hashing failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Registration failed. Please try again.");

        // Verify rollback was attempted
        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify error logging was called
        _mockLogsService.Verify(x => x.LogErrorAsync(
            It.IsAny<Exception>(),
            It.Is<string>(s => s.Contains("Error during user registration")),
            It.IsAny<Dictionary<string, object>>()), 
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenDatabaseFails_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Create<RegisterUserCommand>();
        var hashedPassword = "hashed_password";

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService.Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Registration failed. Please try again.");
    }

    [Test]
    public async Task Handle_WithoutDefaultRole_ShouldStillRegisterUser()
    {
        // Arrange
        var command = _fixture.Create<RegisterUserCommand>();
        var hashedPassword = "hashed_password";
        var createdUser = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService.Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockRoleRepository.Setup(x => x.GetByNameAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null); // No default role found

        _mockDeviceRepository.Setup(x => x.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<Device>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Verify user was still created even without default role
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}