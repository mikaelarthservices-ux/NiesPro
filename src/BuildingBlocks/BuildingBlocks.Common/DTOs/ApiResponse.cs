using System.Net;

namespace BuildingBlocks.Common.DTOs
{
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
            return new ApiResponse<T> { Success = true, Data = data, Message = "Success" };
        }
        
        public static ApiResponse<T> CreateSuccess(T data, string message)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }
        
        public static ApiResponse<T> CreateError(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message };
        }
        
        public static ApiResponse<T> CreateError(string message, IEnumerable<string> errors)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
        }
    }
}
