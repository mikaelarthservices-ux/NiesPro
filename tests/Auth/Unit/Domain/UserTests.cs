using NUnit.Framework;
using FluentAssertions;
using AutoFixture;
using Auth.Domain.Entities;

namespace Auth.Tests.Unit.Domain;

/// <summary>
/// Unit tests for User entity business logic and computed properties
/// </summary>
[TestFixture]
public class UserTests
{
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Test]
    public void Constructor_ShouldCreateUserWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeFalse();
        user.UserRoles.Should().BeEmpty();
        user.Devices.Should().BeEmpty();
        user.Sessions.Should().BeEmpty();
        user.Username.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
    }

    [Test]
    public void FullName_WithBothNames_ShouldReturnCombinedName()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.FirstName, "John")
            .With(u => u.LastName, "Doe")
            .Create();

        // Act & Assert
        user.FullName.Should().Be("John Doe");
    }

    [Test]
    public void FullName_WithFirstNameOnly_ShouldReturnFirstName()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.FirstName, "John")
            .With(u => u.LastName, (string?)null)
            .Create();

        // Act & Assert
        user.FullName.Should().Be("John");
    }

    [Test]
    public void FullName_WithLastNameOnly_ShouldReturnLastName()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.FirstName, (string?)null)
            .With(u => u.LastName, "Doe")
            .Create();

        // Act & Assert
        user.FullName.Should().Be("Doe");
    }

    [Test]
    public void FullName_WithoutNames_ShouldReturnEmptyString()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.FirstName, (string?)null)
            .With(u => u.LastName, (string?)null)
            .Create();

        // Act & Assert
        user.FullName.Should().BeEmpty();
    }

    [Test]
    public void HasValidDevices_WithActiveDevices_ShouldReturnTrue()
    {
        // Arrange
        var activeDevice = _fixture.Build<Device>()
            .With(d => d.IsActive, true)
            .Create();

        var user = _fixture.Build<User>()
            .With(u => u.Devices, new List<Device> { activeDevice })
            .Create();

        // Act & Assert
        user.HasValidDevices.Should().BeTrue();
    }

    [Test]
    public void HasValidDevices_WithInactiveDevices_ShouldReturnFalse()
    {
        // Arrange
        var inactiveDevice = _fixture.Build<Device>()
            .With(d => d.IsActive, false)
            .Create();

        var user = _fixture.Build<User>()
            .With(u => u.Devices, new List<Device> { inactiveDevice })
            .Create();

        // Act & Assert
        user.HasValidDevices.Should().BeFalse();
    }

    [Test]
    public void HasValidDevices_WithoutDevices_ShouldReturnFalse()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.Devices, new List<Device>())
            .Create();

        // Act & Assert
        user.HasValidDevices.Should().BeFalse();
    }

    [Test]
    public void RoleNames_WithRoles_ShouldReturnRoleNames()
    {
        // Arrange
        var role1 = _fixture.Build<Role>().With(r => r.Name, "Admin").Create();
        var role2 = _fixture.Build<Role>().With(r => r.Name, "User").Create();

        var userRole1 = _fixture.Build<UserRole>().With(ur => ur.Role, role1).Create();
        var userRole2 = _fixture.Build<UserRole>().With(ur => ur.Role, role2).Create();

        var user = _fixture.Build<User>()
            .With(u => u.UserRoles, new List<UserRole> { userRole1, userRole2 })
            .Create();

        // Act & Assert
        user.RoleNames.Should().Contain(new[] { "Admin", "User" });
    }

    [Test]
    public void RoleNames_WithoutRoles_ShouldReturnEmpty()
    {
        // Arrange
        var user = _fixture.Build<User>()
            .With(u => u.UserRoles, new List<UserRole>())
            .Create();

        // Act & Assert
        user.RoleNames.Should().BeEmpty();
    }

    [Test]
    public void IsActive_DefaultValue_ShouldBeTrue()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Test]
    public void EmailConfirmed_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.EmailConfirmed.Should().BeFalse();
    }

    [Test]
    public void LastLoginAt_InitialValue_ShouldBeNull()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.LastLoginAt.Should().BeNull();
    }
}