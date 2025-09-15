using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NiesPro.Common.Models;
using System.Net;
using System.Text.Json;

namespace NiesPro.WebApi.Middleware
{
    /// <summary>
    /// Custom exception types
    /// </summary>
    public class BusinessException : Exception
    {
        public string Code { get; }

        public BusinessException(string code, string message) : base(message)
        {
            Code = code;
        }
    }

    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message) : base("NOT_FOUND", message) { }
    }

    public class ValidationException : BusinessException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("VALIDATION_FAILED", "One or more validation errors occurred")
        {
            Errors = errors;
        }
    }

    public class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(string message = "Unauthorized access") : base("UNAUTHORIZED", message) { }
    }

    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    response.Success = false;
                    response.Message = notFoundEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case ValidationException validationEx:
                    response.Success = false;
                    response.Message = validationEx.Message;
                    response.Errors = validationEx.Errors.SelectMany(x => x.Value).ToList();
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedException unauthorizedEx:
                    response.Success = false;
                    response.Message = unauthorizedEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case BusinessException businessEx:
                    response.Success = false;
                    response.Message = businessEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    response.Success = false;
                    response.Message = "An internal server error occurred";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}