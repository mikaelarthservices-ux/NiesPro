using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Auth.Application.Common.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for comprehensive logging
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("Starting request {RequestName} with ID {RequestId}", 
                requestName, requestId);

            // Log request details (be careful with sensitive data)
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Request {RequestName} details: {@Request}", 
                    requestName, SanitizeRequest(request));
            }

            try
            {
                var response = await next();
                
                stopwatch.Stop();
                
                _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                // Log response details for debug
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Request {RequestName} response: {@Response}", 
                        requestName, SanitizeResponse(response));
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {ElapsedMs}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }

        /// <summary>
        /// Sanitize request to remove sensitive information
        /// </summary>
        private object SanitizeRequest(TRequest request)
        {
            var requestType = request?.GetType();
            if (requestType == null) return "null request";

            // Create sanitized version by hiding password fields
            var properties = requestType.GetProperties()
                .Where(p => p.CanRead)
                .ToDictionary(
                    p => p.Name,
                    p => IsSensitiveProperty(p.Name) ? "***HIDDEN***" : p.GetValue(request)
                );

            return properties;
        }

        /// <summary>
        /// Sanitize response to remove sensitive information
        /// </summary>
        private object SanitizeResponse(TResponse response)
        {
            // For now, just return type name to avoid logging sensitive data
            return response?.GetType().Name ?? "null response";
        }

        /// <summary>
        /// Check if property contains sensitive information
        /// </summary>
        private bool IsSensitiveProperty(string propertyName)
        {
            var sensitiveFields = new[] 
            { 
                "password", "token", "secret", "key", "hash", 
                "currentpassword", "newpassword", "confirmpassword" 
            };

            return sensitiveFields.Any(field => 
                propertyName.Contains(field, StringComparison.OrdinalIgnoreCase));
        }
    }
}
