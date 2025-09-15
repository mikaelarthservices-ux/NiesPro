using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Auth.Application.Common.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for performance monitoring
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly Stopwatch _timer;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _timer = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            var requestName = typeof(TRequest).Name;

            // Log performance metrics
            if (elapsedMilliseconds > 500) // Long running request threshold
            {
                _logger.LogWarning("Long Running Request: {RequestName} took {ElapsedMilliseconds}ms to complete", 
                    requestName, elapsedMilliseconds);

                // Could also log request details for analysis
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Long running request details - Name: {RequestName}, Duration: {ElapsedMilliseconds}ms", 
                        requestName, elapsedMilliseconds);
                }
            }
            else if (elapsedMilliseconds > 100) // Medium running request
            {
                _logger.LogInformation("Medium Request: {RequestName} completed in {ElapsedMilliseconds}ms", 
                    requestName, elapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("Fast Request: {RequestName} completed in {ElapsedMilliseconds}ms", 
                    requestName, elapsedMilliseconds);
            }

            // Performance metrics that could be sent to monitoring systems
            LogPerformanceMetrics(requestName, elapsedMilliseconds);

            return response;
        }

        /// <summary>
        /// Log performance metrics for monitoring systems
        /// </summary>
        private void LogPerformanceMetrics(string requestName, long elapsedMilliseconds)
        {
            // Here you could integrate with Application Insights, DataDog, etc.
            // Example patterns:
            
            // Application Insights
            // _telemetryClient.TrackDependency("MediatR", requestName, DateTime.UtcNow.AddMilliseconds(-elapsedMilliseconds), 
            //     TimeSpan.FromMilliseconds(elapsedMilliseconds), true);

            // Custom metrics
            // _metricsCollector.RecordRequestDuration(requestName, elapsedMilliseconds);

            // For now, just structured logging
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["RequestName"] = requestName,
                ["Duration"] = elapsedMilliseconds,
                ["PerformanceCategory"] = GetPerformanceCategory(elapsedMilliseconds)
            });

            _logger.LogDebug("Performance metric recorded");
        }

        /// <summary>
        /// Categorize performance based on duration
        /// </summary>
        private string GetPerformanceCategory(long elapsedMilliseconds)
        {
            return elapsedMilliseconds switch
            {
                < 50 => "Excellent",
                < 100 => "Good",
                < 500 => "Acceptable",
                < 1000 => "Slow",
                _ => "Critical"
            };
        }
    }
}
