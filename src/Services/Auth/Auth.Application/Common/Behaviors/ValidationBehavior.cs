using MediatR;
using FluentValidation;
using BuildingBlocks.Common.DTOs;

namespace Auth.Application.Common.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior for FluentValidation integration
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                return CreateValidationErrorResponse(failures);
            }

            return await next();
        }

        /// <summary>
        /// Create validation error response for ApiResponse pattern
        /// </summary>
        private TResponse CreateValidationErrorResponse(IList<FluentValidation.Results.ValidationFailure> failures)
        {
            var errors = failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}").ToList();
            var errorMessage = string.Join("; ", errors);

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

            // Fallback for non-ApiResponse types
            throw new ValidationException(failures);
        }
    }
}
