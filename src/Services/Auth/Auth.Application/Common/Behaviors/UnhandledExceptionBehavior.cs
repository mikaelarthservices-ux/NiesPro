using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Auth.Application.Common.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for global exception handling
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

        public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                var requestId = Guid.NewGuid();

                _logger.LogError(ex, "Unhandled exception occurred for request {RequestName} with ID {RequestId}", 
                    requestName, requestId);

                // Log additional context for debugging
                _logger.LogError("Exception details - Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    ex.GetType().Name, ex.Message, ex.StackTrace);

                // Create appropriate error response
                return CreateErrorResponse(ex, requestName);
            }
        }

        /// <summary>
        /// Create error response based on exception type
        /// </summary>
        private TResponse CreateErrorResponse(Exception ex, string requestName)
        {
            var errorMessage = GetUserFriendlyErrorMessage(ex, requestName);

            // Check if TResponse is ApiResponse<T>
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                var dataType = responseType.GetGenericArguments()[0];
                var createErrorMethod = typeof(ApiResponse<>)
                    .MakeGenericType(dataType)
                    .GetMethod("CreateError", new[] { typeof(string) });

                if (createErrorMethod != null)
                {
                    return (TResponse)createErrorMethod.Invoke(null, new object[] { errorMessage })!;
                }
            }

            // If not ApiResponse<T>, re-throw the exception
            throw ex;
        }

        /// <summary>
        /// Get user-friendly error message based on exception type
        /// </summary>
        private string GetUserFriendlyErrorMessage(Exception ex, string requestName)
        {
            return ex switch
            {
                // Database-related exceptions
                TimeoutException => "The operation timed out. Please try again.",
                UnauthorizedAccessException => "You are not authorized to perform this operation.",
                ArgumentException => "Invalid input provided. Please check your data.",
                InvalidOperationException => "The requested operation cannot be performed at this time.",
                
                // Custom business exceptions (you can add your own)
                // DomainException domainEx => domainEx.Message,
                // ValidationException validationEx => validationEx.Message,
                
                // Default fallback
                _ => $"An error occurred while processing your {requestName.Replace("Command", "").Replace("Query", "")} request. Please try again or contact support if the problem persists."
            };
        }
    }
}
