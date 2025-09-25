using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Application.Features.Users.Commands.RegisterUser;
using NiesPro.Contracts.Common;

namespace Auth.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for authentication endpoints
/// Tests the full authentication flow including JWT tokens
/// </summary>
[TestFixture]
public class AuthControllerTests
{
    private AuthWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new AuthWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        // Clean database before each test
        _factory.CleanupDatabase();
    }

    [Test]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var loginRequest = new LoginCommand
        {
            Email = "test@example.com",
            Password = "password123", 
            DeviceKey = "test-device-key",
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(responseContent);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.AccessToken.Should().NotBeNullOrEmpty();
        apiResponse.Data.RefreshToken.Should().NotBeNullOrEmpty();
        apiResponse.Data.User.Should().NotBeNull();
        apiResponse.Data.User.Email.Should().Be(loginRequest.Email);
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginCommand
        {
            Email = "test@example.com",
            Password = "wrongpassword",
            DeviceKey = "test-device-key", 
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(responseContent);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Message.Should().Contain("Invalid");
    }

    [Test]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "password123",
            DeviceKey = "test-device-key",
            IpAddress = "127.0.0.1", 
            UserAgent = "Test Agent"
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(responseContent);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Message.Should().Contain("Invalid");
    }

    [Test]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var registerRequest = new RegisterUserCommand
        {
            Username = "newuser",
            Email = "newuser@example.com", 
            Password = "NewPassword123!",
            FirstName = "New",
            LastName = "User",
            DeviceKey = "new-device-key",
            DeviceName = "New Device",
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RegisterUserResponse>>(responseContent);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Email.Should().Be(registerRequest.Email);
        apiResponse.Data.Username.Should().Be(registerRequest.Username);
        apiResponse.Data.UserId.Should().NotBeEmpty();
    }

    [Test]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterUserCommand
        {
            Username = "anotheruser",
            Email = "test@example.com", // This email already exists in test data
            Password = "NewPassword123!",
            FirstName = "Another", 
            LastName = "User",
            DeviceKey = "another-device-key",
            DeviceName = "Another Device",
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RegisterUserResponse>>(responseContent);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Message.Should().Contain("already registered");
    }

    [Test]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterUserCommand
        {
            Username = "testuser2",
            Email = "invalid-email", // Invalid email format
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            DeviceKey = "test-device-2",
            DeviceName = "Test Device 2",
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Health_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Swagger_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Auth API");
    }
}

/// <summary>
/// Response DTOs matching the API responses
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class RegisterUserResponse  
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
}