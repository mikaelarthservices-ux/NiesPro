using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Gateway.API.Services;

/// <summary>
/// Interface pour la validation des tokens JWT
/// </summary>
public interface ITokenValidationService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> IsTokenBlacklistedAsync(string jti);
    Task BlacklistTokenAsync(string jti, DateTime expiration);
}

/// <summary>
/// Service professionnel de validation des tokens JWT avec blacklist
/// </summary>
public class TokenValidationService : ITokenValidationService
{
    private readonly ILogger<TokenValidationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _blacklistCache;
    private readonly HttpClient _httpClient;

    public TokenValidationService(
        ILogger<TokenValidationService> logger,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _blacklistCache = memoryCache;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Valide un token JWT auprès du service Auth.API
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation attempted with empty token");
                return false;
            }

            // Validation auprès d'Auth.API
            var authApiUrl = _configuration["Microservices:AuthAPI:BaseUrl"];
            var validationEndpoint = $"{authApiUrl}/api/auth/validate-token";

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(validationEndpoint, null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token validation successful");
                return true;
            }

            _logger.LogWarning("Token validation failed. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return false;
        }
    }

    /// <summary>
    /// Vérifie si un token est dans la blacklist
    /// </summary>
    public Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return Task.FromResult(false);

        var isBlacklisted = _blacklistCache.TryGetValue($"blacklist_{jti}", out _);
        
        if (isBlacklisted)
        {
            _logger.LogInformation("Token found in blacklist: {Jti}", jti);
        }

        return Task.FromResult(isBlacklisted);
    }

    /// <summary>
    /// Ajoute un token à la blacklist
    /// </summary>
    public Task BlacklistTokenAsync(string jti, DateTime expiration)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return Task.CompletedTask;

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiration,
            Priority = CacheItemPriority.High
        };

        _blacklistCache.Set($"blacklist_{jti}", true, cacheOptions);
        
        _logger.LogInformation("Token added to blacklist: {Jti}, Expiration: {Expiration}", 
            jti, expiration);

        return Task.CompletedTask;
    }
}