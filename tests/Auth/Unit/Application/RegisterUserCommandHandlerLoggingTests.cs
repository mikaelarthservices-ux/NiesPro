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
/// Tests spécifiques pour l'intégration NiesPro Logging dans RegisterUserCommandHandler
/// </summary>
[TestFixture]
public class RegisterUserCommandHandlerLoggingTests
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
    public async Task Handle_SuccessfulRegistration_ShouldCallAuditCreateAsync()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(c => c.Email, "test@example.com")
            .With(c => c.Username, "testuser")
            .With(c => c.Password, "SecurePassword123!")
            .With(c => c.IpAddress, "192.168.1.1")
            .With(c => c.UserAgent, "TestAgent/1.0")
            .Create();

        var hashedPassword = "hashed_password";
        var createdUser = _fixture.Build<User>()
            .With(u => u.Id, Guid.NewGuid())
            .With(u => u.Email, command.Email)
            .With(u => u.Username, command.Username)
            .With(u => u.PasswordHash, hashedPassword)
            .Create();

        var device = _fixture.Build<Device>()
            .With(d => d.Id, Guid.NewGuid())
            .With(d => d.UserId, createdUser.Id)
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
        _mockDeviceRepository.Setup(x => x.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Vérifier que l'audit est appelé avec les bons paramètres
        _mockAuditService.Verify(x => x.AuditCreateAsync(
            createdUser.Id.ToString(), // userId
            createdUser.Username,       // userName
            "User",                     // entityName
            createdUser.Id.ToString(),  // entityId
            It.Is<Dictionary<string, object>>(metadata => 
                metadata.ContainsKey("Email") && 
                metadata["Email"].ToString() == command.Email &&
                metadata.ContainsKey("IpAddress") &&
                metadata["IpAddress"].ToString() == command.IpAddress &&
                metadata.ContainsKey("DeviceId") // DeviceId doit être présent mais on ne teste pas la valeur exacte
            )), 
            Times.Once);
    }

    [Test]
    public async Task Handle_SuccessfulRegistration_ShouldCallLogsServiceWithCorrectProperties()
    {
        // Arrange
        var command = _fixture.Build<RegisterUserCommand>()
            .With(c => c.Email, "test@example.com")
            .With(c => c.Username, "testuser")
            .Create();

        var createdUser = _fixture.Build<User>()
            .With(u => u.Id, Guid.NewGuid())
            .With(u => u.Email, command.Email)
            .With(u => u.Username, command.Username)
            .Create();

        // Setup mocks pour succès
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockPasswordService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);
        _mockDeviceRepository.Setup(x => x.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<Device>());
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Vérifier l'appel de logging de succès
        _mockLogsService.Verify(x => x.LogAsync(
            LogLevel.Information,
            It.Is<string>(s => s.Contains("User registration successful") && s.Contains(createdUser.Id.ToString())),
            null, // pas d'exception
            It.Is<Dictionary<string, object>>(props => 
                props.ContainsKey("UserId") &&
                props["UserId"].ToString() == createdUser.Id.ToString() &&
                props.ContainsKey("Email") &&
                props["Email"].ToString() == command.Email
            )), 
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenExceptionOccurs_ShouldCallLogErrorAsync()
    {
        // Arrange
        var command = _fixture.Create<RegisterUserCommand>();
        var expectedException = new InvalidOperationException("Test exception");

        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockPasswordService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Throws(expectedException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        // Vérifier l'appel de logging d'erreur
        _mockLogsService.Verify(x => x.LogErrorAsync(
            expectedException,
            It.Is<string>(s => s.Contains("Error during user registration") && s.Contains(command.Email)),
            It.Is<Dictionary<string, object>>(props => 
                props.ContainsKey("Email") &&
                props["Email"].ToString() == command.Email &&
                props.ContainsKey("Username") &&
                props["Username"].ToString() == command.Username
            )), 
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenRollbackExceptionOccurs_ShouldLogRollbackError()
    {
        // Arrange
        var command = _fixture.Create<RegisterUserCommand>();
        var mainException = new InvalidOperationException("Main exception");
        var rollbackException = new InvalidOperationException("Rollback exception");

        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockPasswordService.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Throws(mainException);
        _mockUnitOfWork.Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(rollbackException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        // Vérifier que l'erreur de rollback est loggée
        _mockLogsService.Verify(x => x.LogErrorAsync(
            rollbackException,
            "Error during transaction rollback",
            It.IsAny<Dictionary<string, object>>()), 
            Times.Once);

        // Vérifier aussi l'erreur principale
        _mockLogsService.Verify(x => x.LogErrorAsync(
            mainException,
            It.Is<string>(s => s.Contains("Error during user registration")),
            It.IsAny<Dictionary<string, object>>()), 
            Times.Once);
    }

    [Test]
    public async Task Handle_WithValidRequest_ShouldNotCallLoggingServicesForValidationErrors()
    {
        // Arrange - Email déjà existant
        var command = _fixture.Create<RegisterUserCommand>();
        var existingUser = _fixture.Build<User>()
            .With(u => u.Email, command.Email)
            .Create();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email is already registered");

        // Vérifier qu'aucun service de logging n'est appelé pour les erreurs de validation
        _mockLogsService.Verify(x => x.LogAsync(
            It.IsAny<LogLevel>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>()), 
            Times.Never);

        _mockLogsService.Verify(x => x.LogErrorAsync(
            It.IsAny<Exception>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>()), 
            Times.Never);

        _mockAuditService.Verify(x => x.AuditCreateAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>()), 
            Times.Never);
    }
}