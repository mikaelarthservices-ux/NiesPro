using Microsoft.Extensions.Logging;
using NiesPro.AdminPanel.Models;
using System.Security.Cryptography;
using System.Text;

namespace NiesPro.AdminPanel.Services;

/// <summary>
/// Service d'authentification avec gestion sécurisée des tokens
/// </summary>
public interface IAuthenticationService
{
    Task<AuthResponse?> LoginAsync(string email, string password);
    Task<bool> RefreshTokenAsync();
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    UserInfo? CurrentUser { get; }
    string? AccessToken { get; }
    event EventHandler<bool>? AuthenticationChanged;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiService _apiService;
    private readonly ILogger<AuthenticationService> _logger;
    private Timer? _refreshTimer;
    private AuthResponse? _currentAuth;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentAuth?.AccessToken);
    public UserInfo? CurrentUser => _currentAuth?.User;
    public string? AccessToken => _currentAuth?.AccessToken;

    public event EventHandler<bool>? AuthenticationChanged;

    public AuthenticationService(IApiService apiService, ILogger<AuthenticationService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", email);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password,
                DeviceKey = GenerateDeviceKey()
            };

            var response = await _apiService.PostAsync<ApiResponse<AuthResponse>>(
                "api/v1/auth/login", loginRequest, "auth");

            if (response?.Success == true && response.Data != null)
            {
                _currentAuth = response.Data;
                _apiService.SetAccessToken(_currentAuth.AccessToken);
                
                // Configurer le timer de refresh
                SetupTokenRefresh();
                
                _logger.LogInformation("Login successful for user: {Email}", email);
                AuthenticationChanged?.Invoke(this, true);
                
                return _currentAuth;
            }
            else
            {
                _logger.LogWarning("Login failed for user: {Email}. Response: {Message}", 
                    email, response?.Message);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", email);
            return null;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_currentAuth?.RefreshToken))
        {
            return false;
        }

        try
        {
            _logger.LogInformation("Refreshing access token");

            var refreshRequest = new { RefreshToken = _currentAuth.RefreshToken };
            var response = await _apiService.PostAsync<ApiResponse<AuthResponse>>(
                "api/v1/auth/refresh", refreshRequest, "auth");

            if (response?.Success == true && response.Data != null)
            {
                _currentAuth = response.Data;
                _apiService.SetAccessToken(_currentAuth.AccessToken);
                
                _logger.LogInformation("Token refresh successful");
                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Message}", response?.Message);
                await LogoutAsync();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_currentAuth?.AccessToken))
            {
                // Appeler l'API de logout si possible
                await _apiService.PostAsync<object>("api/v1/auth/logout", new { }, "auth");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during logout API call");
        }
        finally
        {
            // Nettoyer l'état local
            _refreshTimer?.Dispose();
            _refreshTimer = null;
            _currentAuth = null;
            _apiService.SetAccessToken(string.Empty);
            
            _logger.LogInformation("User logged out");
            AuthenticationChanged?.Invoke(this, false);
        }
    }

    private void SetupTokenRefresh()
    {
        if (_currentAuth == null) return;

        _refreshTimer?.Dispose();

        // Programmer le refresh à 80% de la durée de vie du token
        var refreshInterval = TimeSpan.FromSeconds(_currentAuth.ExpiresIn * 0.8);
        
        _refreshTimer = new Timer(async _ =>
        {
            await RefreshTokenAsync();
        }, null, refreshInterval, refreshInterval);

        _logger.LogInformation("Token refresh scheduled for {Minutes} minutes", 
            refreshInterval.TotalMinutes);
    }

    private string GenerateDeviceKey()
    {
        // Générer une clé unique basée sur la machine
        var machineId = Environment.MachineName + Environment.UserName;
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineId));
        return Convert.ToBase64String(hash)[..16]; // Prendre les 16 premiers caractères
    }
}