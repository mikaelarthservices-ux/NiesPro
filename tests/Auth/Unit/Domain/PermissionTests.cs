using NUnit.Framework;
using FluentAssertions;
using AutoFixture;
using Auth.Domain.Entities;

namespace Auth.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Permission entity business logic
/// </summary>
[TestFixture]
public class PermissionTests
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
    public void Constructor_ShouldCreatePermissionWithDefaultValues()
    {
        // Act
        var permission = new Permission();

        // Assert
        permission.IsActive.Should().BeTrue();
        permission.RolePermissions.Should().BeEmpty();
        permission.Name.Should().BeEmpty();
        permission.Module.Should().BeEmpty();
    }

    [Test]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var permission = new Permission();
        const string permissionName = "UserManagement.Create";

        // Act
        permission.Name = permissionName;

        // Assert
        permission.Name.Should().Be(permissionName);
    }

    [Test]
    public void Description_ShouldBeSettable()
    {
        // Arrange
        var permission = new Permission();
        const string description = "Allow creating new users";

        // Act
        permission.Description = description;

        // Assert
        permission.Description.Should().Be(description);
    }

    [Test]
    public void Module_ShouldBeSettable()
    {
        // Arrange
        var permission = new Permission();
        const string module = "UserManagement";

        // Act
        permission.Module = module;

        // Assert
        permission.Module.Should().Be(module);
    }

    [Test]
    public void IsActive_DefaultValue_ShouldBeTrue()
    {
        // Arrange & Act
        var permission = new Permission();

        // Assert
        permission.IsActive.Should().BeTrue();
    }

    [Test]
    public void RolePermissions_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var permission = new Permission();

        // Assert
        permission.RolePermissions.Should().NotBeNull();
        permission.RolePermissions.Should().BeEmpty();
    }

    [Test]
    public void RolePermissions_ShouldAllowAddingRolePermissions()
    {
        // Arrange
        var permission = new Permission();
        var rolePermission = _fixture.Create<RolePermission>();

        // Act
        permission.RolePermissions.Add(rolePermission);

        // Assert
        permission.RolePermissions.Should().Contain(rolePermission);
        permission.RolePermissions.Should().HaveCount(1);
    }
}