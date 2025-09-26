using NUnit.Framework;
using FluentAssertions;
using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Domain.Entities;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Auth.Tests.Unit.Application;

/// <summary>
/// Unit tests for LoginCommandHandler authentication logic
/// </summary>
[TestFixture]
public class LoginCommandHandlerTests
{
    private IFixture _fixture;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IDeviceRepository> _mockDeviceRepository;
    private Mock<IUserSessionRepository> _mockUserSessionRepository;
    private Mock<IPasswordService> _mockPasswordService;
    private Mock<IJwtService> _mockJwtService;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private Mock<ILogsServiceClient> _mockLogsService;
    private Mock<IAuditServiceClient> _mockAuditService;
    private LoginCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockUserRepository = new Mock<IUserRepository>();
        _mockDeviceRepository = new Mock<IDeviceRepository>();
        _mockUserSessionRepository = new Mock<IUserSessionRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();
        _mockLogsService = new Mock<ILogsServiceClient>();
        _mockAuditService = new Mock<IAuditServiceClient>();

        _handler = new LoginCommandHandler(
            _mockUserRepository.Object,
            _mockDeviceRepository.Object,
            _mockUserSessionRepository.Object,
            _mockPasswordService.Object,
            _mockJwtService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockLogsService.Object,
            _mockAuditService.Object
        );
    }

    [Test]
    public async Task Handle_WithValidCredentials_ShouldLoginSuccessfully()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(c => c.Email, "test@example.com")
            .With(c => c.Password, "password123")
            .With(c => c.DeviceKey, "device123")
            .Create();

        var user = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.IsActive, true)
            .With(u => u.PasswordHash, "hashed_password")
            .With(u => u.UserRoles, new List<UserRole>
            {
                new() { Role = new Role { Name = "User" } }
            })
            .Create();

        var device = _fixture.Build<Device>()
            .With(d => d.DeviceKey, command.DeviceKey)
            .With(d => d.IsActive, true)
            .Create();

        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Setup mocks
        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _mockDeviceRepository.Setup(x => x.GetByDeviceKeyAsync(command.DeviceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockJwtService.Setup(x => x.GenerateToken(user.Id, user.Email, It.IsAny<List<string>>()))
            .Returns(accessToken);

        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockJwtService.Setup(x => x.GetTokenExpiration(accessToken))
            .Returns(expiresAt);

        _mockUserSessionRepository.Setup(x => x.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .Returns((UserSession session, CancellationToken ct) => Task.FromResult(session));

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be(accessToken);
        result.Data.RefreshToken.Should().Be(refreshToken);
        result.Data.User.Email.Should().Be(command.Email);

        // Verify interactions
        _mockUserRepository.Verify(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockPasswordService.Verify(x => x.VerifyPassword(command.Password, user.PasswordHash), Times.Once);
        _mockJwtService.Verify(x => x.GenerateToken(user.Id, user.Email, It.IsAny<List<string>>()), Times.Once);
        _mockUserSessionRepository.Verify(x => x.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithInvalidEmail_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(c => c.Email, "nonexistent@example.com")
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");

        // Verify password was not checked
        _mockPasswordService.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(c => c.Password, "wrongpassword")
            .Create();

        var user = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.PasswordHash, "hashed_password")
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password");

        // Verify no token generation
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithInactiveUser_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.IsActive, false)
            .With(u => u.PasswordHash, "hashed_password")
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Account is deactivated");

        // Verify no token generation
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Test]
    public async Task Handle_WithInvalidDevice_ShouldReturnError()
    {
        // Arrange
        var command = _fixture.Build<LoginCommand>()
            .With(c => c.DeviceKey, "unknown_device")
            .Create();

        var user = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.IsActive, true)
            .With(u => u.PasswordHash, "hashed_password")
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _mockDeviceRepository.Setup(x => x.GetByDeviceKeyAsync(command.DeviceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Device not authorized");

        // Verify no token generation
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Test]
    public async Task Handle_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.IsActive, true)
            .With(u => u.PasswordHash, "hashed_password")
            .With(u => u.LastLoginAt, (DateTime?)null)
            .With(u => u.UserRoles, new List<UserRole>())
            .Create();

        var device = _fixture.Build<Device>()
            .With(d => d.DeviceKey, command.DeviceKey)
            .Create();

        SetupSuccessfulLogin(command, user, device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify LastLoginAt was updated
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.LastLoginAt.HasValue && 
            u.LastLoginAt.Value > DateTime.UtcNow.AddMinutes(-1)), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldCreateUserSession()
    {
        // Arrange
        var command = _fixture.Create<LoginCommand>();
        var user = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .With(u => u.IsActive, true)
            .With(u => u.UserRoles, new List<UserRole>())
            .Create();

        var device = _fixture.Create<Device>();

        SetupSuccessfulLogin(command, user, device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify session creation
        _mockUserSessionRepository.Verify(x => x.CreateAsync(It.Is<UserSession>(s =>
            s.UserId == user.Id &&
            s.DeviceId == device.Id &&
            s.IpAddress == command.IpAddress &&
            s.UserAgent == command.UserAgent &&
            s.IsActive == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupSuccessfulLogin(LoginCommand command, User user, Device device)
    {
        var accessToken = "access_token_" + Guid.NewGuid();
        var refreshToken = "refresh_token_" + Guid.NewGuid();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _mockDeviceRepository.Setup(x => x.GetByDeviceKeyAsync(command.DeviceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns(accessToken);

        _mockJwtService.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockJwtService.Setup(x => x.GetTokenExpiration(It.IsAny<string>()))
            .Returns(DateTime.UtcNow.AddHours(1));

        _mockUserSessionRepository.Setup(x => x.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .Returns((UserSession session, CancellationToken ct) => Task.FromResult(session));

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));
    }
}