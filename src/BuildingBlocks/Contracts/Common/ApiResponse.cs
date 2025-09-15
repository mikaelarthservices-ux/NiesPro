namespace NiesPro.Contracts.Common
{
    /// <summary>
    /// Standard response wrapper for all API operations
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create a successful response with data
        /// </summary>
        public static ApiResponse<T> CreateSuccess(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Create an error response with message
        /// </summary>
        public static ApiResponse<T> CreateError(string message)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = new List<string> { message }
            };
        }

        /// <summary>
        /// Create an error response with multiple errors
        /// </summary>
        public static ApiResponse<T> CreateError(List<string> errors)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = "Multiple errors occurred",
                Errors = errors
            };
        }
    }
}