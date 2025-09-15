using System.Net;

namespace BuildingBlocks.Common.DTOs
{
    /// <summary>
    /// Standard API response wrapper providing consistent response format across all APIs
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The actual response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Collection of error messages (if any)
        /// </summary>
        public IEnumerable<string>? Errors { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Request/Response timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unique correlation ID for request tracking
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Request trace ID for distributed tracing
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Additional metadata (optional)
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        #region Factory Methods

        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        /// <param name="data">Response data</param>
        /// <param name="message">Success message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Successful API response</returns>
        public static ApiResponse<T> Success(T data, string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = (int)statusCode
            };
        }

        /// <summary>
        /// Creates a successful response without data
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Successful API response</returns>
        public static ApiResponse<T> Success(string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                StatusCode = (int)statusCode
            };
        }

        /// <summary>
        /// Creates an error response with message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errors">Collection of detailed errors</param>
        /// <returns>Error API response</returns>
        public static ApiResponse<T> Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)statusCode,
                Errors = errors
            };
        }

        /// <summary>
        /// Creates a validation error response
        /// </summary>
        /// <param name="errors">Validation error messages</param>
        /// <param name="message">General validation message</param>
        /// <returns>Validation error API response</returns>
        public static ApiResponse<T> ValidationError(IEnumerable<string> errors, string message = "Validation failed")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                Errors = errors
            };
        }

        /// <summary>
        /// Creates a not found response
        /// </summary>
        /// <param name="message">Not found message</param>
        /// <returns>Not found API response</returns>
        public static ApiResponse<T> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }

        /// <summary>
        /// Creates an unauthorized response
        /// </summary>
        /// <param name="message">Unauthorized message</param>
        /// <returns>Unauthorized API response</returns>
        public static ApiResponse<T> Unauthorized(string message = "Access denied")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
        }

        /// <summary>
        /// Creates a forbidden response
        /// </summary>
        /// <param name="message">Forbidden message</param>
        /// <returns>Forbidden API response</returns>
        public static ApiResponse<T> Forbidden(string message = "Insufficient permissions")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }

        /// <summary>
        /// Creates an internal server error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>Internal server error API response</returns>
        public static ApiResponse<T> InternalServerError(string message = "An internal server error occurred")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        #endregion

        #region Fluent API

        /// <summary>
        /// Sets the correlation ID for request tracking
        /// </summary>
        /// <param name="correlationId">Correlation ID</param>
        /// <returns>API response with correlation ID set</returns>
        public ApiResponse<T> WithCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
            return this;
        }

        /// <summary>
        /// Sets the trace ID for distributed tracing
        /// </summary>
        /// <param name="traceId">Trace ID</param>
        /// <returns>API response with trace ID set</returns>
        public ApiResponse<T> WithTraceId(string traceId)
        {
            TraceId = traceId;
            return this;
        }

        /// <summary>
        /// Adds metadata to the response
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <param name="value">Metadata value</param>
        /// <returns>API response with metadata added</returns>
        public ApiResponse<T> WithMetadata(string key, object value)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata[key] = value;
            return this;
        }

        /// <summary>
        /// Sets multiple metadata entries
        /// </summary>
        /// <param name="metadata">Metadata dictionary</param>
        /// <returns>API response with metadata set</returns>
        public ApiResponse<T> WithMetadata(Dictionary<string, object> metadata)
        {
            Metadata = metadata;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// Non-generic API response for operations that don't return data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// Creates a successful response without data
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Successful API response</returns>
        public static new ApiResponse Success(string message = "Operation completed successfully", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = (int)statusCode
            };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errors">Collection of detailed errors</param>
        /// <returns>Error API response</returns>
        public static new ApiResponse Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, IEnumerable<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = (int)statusCode,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Pagination parameters for list queries
    /// </summary>
    public class PaginationRequest
    {
        private int _page = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
        }

        /// <summary>
        /// Search term for filtering
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Field name to sort by
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (true for descending, false for ascending)
        /// </summary>
        public bool SortDescending { get; set; } = false;

        /// <summary>
        /// Additional filters
        /// </summary>
        public Dictionary<string, string>? Filters { get; set; }

        /// <summary>
        /// Calculate skip value for database queries
        /// </summary>
        public int Skip => (Page - 1) * PageSize;

        /// <summary>
        /// Get take value for database queries
        /// </summary>
        public int Take => PageSize;
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Collection of items for current page
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Total number of records across all pages
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Indicates if there's a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Indicates if there's a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Number of the first item on current page
        /// </summary>
        public int FirstItemOnPage => TotalRecords == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;

        /// <summary>
        /// Number of the last item on current page
        /// </summary>
        public int LastItemOnPage => Math.Min(CurrentPage * PageSize, TotalRecords);

        /// <summary>
        /// Creates a paginated response
        /// </summary>
        /// <param name="items">Items for current page</param>
        /// <param name="totalRecords">Total number of records</param>
        /// <param name="currentPage">Current page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paginated response</returns>
        public static PaginatedResponse<T> Create(IEnumerable<T> items, int totalRecords, int currentPage, int pageSize)
        {
            return new PaginatedResponse<T>
            {
                Items = items,
                TotalRecords = totalRecords,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }
    }
}