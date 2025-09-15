using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NiesPro.WebApi.Middleware
{
    /// <summary>
    /// Interface for device validation service
    /// </summary>
    public interface IDeviceValidationService
    {
        Task<bool> ValidateDeviceAsync(string deviceKey, CancellationToken cancellationToken = default);
        Task<string?> GetDeviceInfoAsync(string deviceKey, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Middleware for device key validation
    /// </summary>
    public class DeviceAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DeviceAuthenticationMiddleware> _logger;
        private readonly IDeviceValidationService _deviceService;

        // Routes qui ne nécessitent pas de validation device
        private readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/swagger",
            "/health",
            "/api/auth/register-device",
            "/api/auth/login"
        };

        public DeviceAuthenticationMiddleware(
            RequestDelegate next,
            ILogger<DeviceAuthenticationMiddleware> logger,
            IDeviceValidationService deviceService)
        {
            _next = next;
            _logger = logger;
            _deviceService = deviceService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip validation for excluded paths
            if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            // Skip validation for OPTIONS requests (CORS preflight)
            if (context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            var deviceKey = ExtractDeviceKey(context.Request);

            if (string.IsNullOrEmpty(deviceKey))
            {
                _logger.LogWarning("Missing device key for request to {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Device key is required");
                return;
            }

            var isValidDevice = await _deviceService.ValidateDeviceAsync(deviceKey);

            if (!isValidDevice)
            {
                _logger.LogWarning("Invalid device key {DeviceKey} for request to {Path}", 
                    deviceKey, context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid device key");
                return;
            }

            // Add device info to context for use in controllers
            var deviceInfo = await _deviceService.GetDeviceInfoAsync(deviceKey);
            context.Items["DeviceKey"] = deviceKey;
            context.Items["DeviceInfo"] = deviceInfo;

            await _next(context);
        }

        private static string? ExtractDeviceKey(HttpRequest request)
        {
            // Try header first
            if (request.Headers.TryGetValue("X-Device-Key", out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            // Try query parameter as fallback
            if (request.Query.TryGetValue("deviceKey", out var queryValue))
            {
                return queryValue.FirstOrDefault();
            }

            return null;
        }
    }

    /// <summary>
    /// Default implementation of device validation service
    /// </summary>
    public class DefaultDeviceValidationService : IDeviceValidationService
    {
        private readonly ILogger<DefaultDeviceValidationService> _logger;

        public DefaultDeviceValidationService(ILogger<DefaultDeviceValidationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> ValidateDeviceAsync(string deviceKey, CancellationToken cancellationToken = default)
        {
            // TODO: Implement real device validation against database
            // For now, accept any non-empty device key
            var isValid = !string.IsNullOrEmpty(deviceKey);
            
            if (isValid)
            {
                _logger.LogInformation("Device {DeviceKey} validated successfully", deviceKey);
            }
            
            return Task.FromResult(isValid);
        }

        public Task<string?> GetDeviceInfoAsync(string deviceKey, CancellationToken cancellationToken = default)
        {
            // TODO: Implement real device info retrieval
            return Task.FromResult<string?>(null);
        }
    }
}