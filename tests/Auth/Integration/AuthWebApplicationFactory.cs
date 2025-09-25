using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Auth.Infrastructure.Data;
using Auth.Domain.Entities;
using Auth.Application.Contracts.Services;

namespace Auth.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for Auth.API integration tests
/// Provides test-specific configuration and in-memory database
/// </summary>
public class AuthWebApplicationFactory : WebApplicationFactory<Auth.API.Program>
{
    private readonly string _environment;
    
    public AuthWebApplicationFactory(string environment = "Test")
    {
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseInMemoryDatabase("AuthTestDb");
                options.EnableSensitiveDataLogging();
            });

            // Configure logging for tests
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Ensure database is created and seeded
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            
            context.Database.EnsureCreated();
            SeedTestData(context);
        });
    }

    /// <summary>
    /// Seeds the test database with initial data
    /// </summary>
    private void SeedTestData(AuthDbContext context)
    {
        if (context.Users.Any())
            return; // Already seeded

        // Create test roles
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "Administrator role",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userRole = new Role
        {
            Id = Guid.NewGuid(), 
            Name = "User",
            Description = "Standard user role",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(adminRole, userRole);

        // Create test permissions
        var permissions = new[]
        {
            new Permission
            {
                Id = Guid.NewGuid(),
                Name = "Users.Read",
                Description = "Read user information",
                Module = "Users",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Permission
            {
                Id = Guid.NewGuid(),
                Name = "Users.Write", 
                Description = "Create and update users",
                Module = "Users",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Permissions.AddRange(permissions);

        // Create test user with hashed password
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "$2a$11$8oJ5Y7WzGJTq8DZBhp.b6OxbCqE4y6pLgDrJ1HEhH8QxNzU3zXzZa", // "password123"
            FirstName = "Test",
            LastName = "User", 
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(testUser);

        // Create user role assignment
        var userRoleAssignment = new UserRole
        {
            UserId = testUser.Id,
            RoleId = userRole.Id,
            AssignedAt = DateTime.UtcNow
        };

        context.UserRoles.Add(userRoleAssignment);

        // Create test device
        var testDevice = new Device
        {
            Id = Guid.NewGuid(),
            DeviceKey = "test-device-key",
            DeviceName = "Test Device",
            DeviceType = DeviceType.Desktop,
            UserId = testUser.Id,
            IsActive = true,
            IsTrusted = true,
            LastUsedAt = DateTime.UtcNow,
            LastIpAddress = "127.0.0.1",
            UserAgent = "Test Agent",
            CreatedAt = DateTime.UtcNow
        };

        context.Devices.Add(testDevice);

        context.SaveChanges();
    }

    /// <summary>
    /// Creates a scoped service provider for testing
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// Gets the test database context
    /// </summary>
    public AuthDbContext GetDbContext()
    {
        var scope = CreateScope();
        return scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    public void CleanupDatabase()
    {
        using var context = GetDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        SeedTestData(context);
    }
}