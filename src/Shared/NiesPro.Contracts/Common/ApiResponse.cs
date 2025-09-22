using System.Net;

namespace NiesPro.Contracts.Common
{
    /// <summary>
    /// Standard API response wrapper for all services
    /// Migrated from BuildingBlocks.Common.DTOs
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public int StatusCode { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the response is successful
        /// </summary>
        public bool IsSuccess => Success;
        
        public static ApiResponse<T> CreateSuccess(T data)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = "Success", StatusCode = 200 };
        }
        
        public static ApiResponse<T> CreateSuccess(T data, string message)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message, StatusCode = 200 };
        }
        
        public static ApiResponse<T> CreateError(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message, StatusCode = 400 };
        }
        
        public static ApiResponse<T> CreateError(string message, IEnumerable<string> errors)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors, StatusCode = 400 };
        }
        
        public static ApiResponse<T> CreateError(string message, int statusCode)
        {
            return new ApiResponse<T> { Success = false, Message = message, StatusCode = statusCode };
        }
    }

    /// <summary>
    /// API response without data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse CreateSuccess()
        {
            return new ApiResponse { Success = true, Message = "Success", StatusCode = 200 };
        }
        
        public static ApiResponse CreateSuccess(string message)
        {
            return new ApiResponse { Success = true, Message = message, StatusCode = 200 };
        }
        
        public static new ApiResponse CreateError(string message)
        {
            return new ApiResponse { Success = false, Message = message, StatusCode = 400 };
        }
        
        public static new ApiResponse CreateError(string message, int statusCode)
        {
            return new ApiResponse { Success = false, Message = message, StatusCode = statusCode };
        }
    }
}