using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NiesPro.AdminPanel.Models;

namespace NiesPro.AdminPanel.Services;

/// <summary>
/// Service API professionnel avec gestion d'erreurs et retry
/// </summary>
public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint, string? service = null);
    Task<T?> PostAsync<T>(string endpoint, object data, string? service = null);
    Task<T?> PutAsync<T>(string endpoint, object data, string? service = null);
    Task<bool> DeleteAsync(string endpoint, string? service = null);
    Task<ServiceHealth> CheckServiceHealthAsync(string serviceName, string baseUrl);
    void SetAccessToken(string token);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly AppSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public string? AccessToken { get; set; }

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger, IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // Configuration SSL pour développement
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NiesPro-AdminPanel/1.0");
        
        if (!string.IsNullOrEmpty(AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint, string? service = null)
    {
        try
        {
            var url = BuildUrl(endpoint, service);
            _logger.LogInformation("GET request to {Url}", url);

            var response = await _httpClient.GetAsync(url);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data, string? service = null)
    {
        try
        {
            var url = BuildUrl(endpoint, service);
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("POST request to {Url}", url);

            var response = await _httpClient.PostAsync(url, content);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data, string? service = null)
    {
        try
        {
            var url = BuildUrl(endpoint, service);
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("PUT request to {Url}", url);

            var response = await _httpClient.PutAsync(url, content);
            return await ProcessResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, string? service = null)
    {
        try
        {
            var url = BuildUrl(endpoint, service);
            _logger.LogInformation("DELETE request to {Url}", url);

            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<ServiceHealth> CheckServiceHealthAsync(string serviceName, string baseUrl)
    {
        var health = new ServiceHealth
        {
            Name = serviceName,
            Url = baseUrl,
            LastCheck = DateTime.Now
        };

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Extraire le port de l'URL
            var uri = new Uri(baseUrl);
            var port = uri.Port;
            var host = uri.Host;
            
            // Vérification TCP simple du port
            using var tcpClient = new System.Net.Sockets.TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port);
            
            // Timeout de 3 secondes
            if (await Task.WhenAny(connectTask, Task.Delay(3000)) == connectTask)
            {
                stopwatch.Stop();
                health.ResponseTime = stopwatch.Elapsed;
                
                if (connectTask.IsCompletedSuccessfully)
                {
                    health.Status = ServiceStatus.Healthy;
                    _logger.LogDebug("Service {ServiceName} is healthy (port {Port} is open)", serviceName, port);
                }
                else
                {
                    health.Status = ServiceStatus.Unhealthy;
                    health.ErrorMessage = "Connection failed";
                }
            }
            else
            {
                health.Status = ServiceStatus.Unhealthy;
                health.ErrorMessage = "Connection timeout";
                stopwatch.Stop();
                health.ResponseTime = stopwatch.Elapsed;
            }
        }
        catch (Exception ex)
        {
            health.Status = ServiceStatus.Unhealthy;
            health.ErrorMessage = ex.Message;
        }

        return health;
    }

    private string BuildUrl(string endpoint, string? service = null)
    {
        var baseUrl = service?.ToLowerInvariant() switch
        {
            "auth" => _settings.ApiEndpoints.Auth,
            "order" => _settings.ApiEndpoints.Order,
            "catalog" => _settings.ApiEndpoints.Catalog,
            "payment" => _settings.ApiEndpoints.Payment,
            _ => _settings.ApiEndpoints.Gateway
        };

        return $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
    }

    private async Task<T?> ProcessResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API request failed: {StatusCode} - {Content}", 
                response.StatusCode, content);
            return default;
        }

        if (string.IsNullOrEmpty(content))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response: {Content}", content);
            return default;
        }
    }

    public void SetAccessToken(string token)
    {
        AccessToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}