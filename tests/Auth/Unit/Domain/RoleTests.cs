using NUnit.Framework;
using FluentAssertions;
using AutoFixture;
using Auth.Domain.Entities;

namespace Auth.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Role entity business logic
/// </summary>
[TestFixture]
public class RoleTests
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
    public void Constructor_ShouldCreateRoleWithDefaultValues()
    {
        // Act
        var role = new Role();

        // Assert
        role.IsActive.Should().BeTrue();
        role.UserRoles.Should().BeEmpty();
        role.RolePermissions.Should().BeEmpty();
        role.Name.Should().BeEmpty();
    }

    [Test]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var role = new Role();
        const string roleName = "Administrator";

        // Act
        role.Name = roleName;

        // Assert
        role.Name.Should().Be(roleName);
    }

    [Test]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var role = new Role();
        const string description = "System administrator role";

        // Act
        role.Description = description;

        // Assert
        role.Description.Should().Be(description);
    }

    [Test]
    public void IsActive_ShouldBeSettable()
    {
        // Arrange
        var role = new Role();

        // Act
        role.IsActive = false;

        // Assert
        role.IsActive.Should().BeFalse();
    }

    [Test]
    public void UserRoles_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var role = new Role();

        // Assert
        role.UserRoles.Should().NotBeNull();
        role.UserRoles.Should().BeEmpty();
    }

    [Test]
    public void RolePermissions_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var role = new Role();

        // Assert
        role.RolePermissions.Should().NotBeNull();
        role.RolePermissions.Should().BeEmpty();
    }

    [Test]
    public void UserRoles_ShouldAllowAddingUserRoles()
    {
        // Arrange
        var role = new Role();
        var userRole = _fixture.Create<UserRole>();

        // Act
        role.UserRoles.Add(userRole);

        // Assert
        role.UserRoles.Should().Contain(userRole);
        role.UserRoles.Should().HaveCount(1);
    }

    [Test]
    public void RolePermissions_ShouldAllowAddingPermissions()
    {
        // Arrange
        var role = new Role();
        var rolePermission = _fixture.Create<RolePermission>();

        // Act
        role.RolePermissions.Add(rolePermission);

        // Assert
        role.RolePermissions.Should().Contain(rolePermission);
        role.RolePermissions.Should().HaveCount(1);
    }
}