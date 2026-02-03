namespace BackSharedGroceries.Common
{
    /// <summary>
    /// Represents the result of a service operation without a return value.
    /// This class provides a standardized way to communicate operation outcomes from the service layer to the controller layer.
    /// It includes success/failure status, error messages, and HTTP status code intent.
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Contains a single error message if the operation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Contains a list of error messages for validation or multi-error scenarios.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Indicates the type of result, which maps to HTTP status codes in the controller.
        /// </summary>
        public ServiceResultType ResultType { get; set; } = ServiceResultType.Ok;

        /// <summary>
        /// Creates a successful result (HTTP 200 OK).
        /// </summary>
        public static ServiceResult Ok() => new() { Success = true, ResultType = ServiceResultType.Ok };

        /// <summary>
        /// Creates a not found result (HTTP 404 Not Found).
        /// </summary>
        /// <param name="error">Error message describing what was not found.</param>
        public static ServiceResult NotFound(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.NotFound };

        /// <summary>
        /// Creates a conflict result (HTTP 409 Conflict).
        /// </summary>
        /// <param name="error">Error message describing the conflict.</param>
        public static ServiceResult Conflict(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.Conflict };

        /// <summary>
        /// Creates an unauthorized result (HTTP 401 Unauthorized).
        /// </summary>
        /// <param name="error">Error message describing the authorization failure.</param>
        public static ServiceResult Unauthorized(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.Unauthorized };

        /// <summary>
        /// Creates a bad request result (HTTP 400 Bad Request).
        /// </summary>
        /// <param name="error">Error message describing the invalid request.</param>
        public static ServiceResult BadRequest(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.BadRequest };

        /// <summary>
        /// Creates a validation error result with multiple error messages (HTTP 400 Bad Request).
        /// </summary>
        /// <param name="errors">List of validation error messages.</param>
        public static ServiceResult ValidationError(List<string> errors) => new() { Success = false, Errors = errors, ResultType = ServiceResultType.BadRequest };
    }

    /// <summary>
    /// Represents the result of a service operation with a return value.
    /// This generic version extends ServiceResult to include typed data when the operation succeeds.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// Contains the data returned by a successful operation.
        /// Will be null if the operation failed.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful result with data (HTTP 200 OK).
        /// </summary>
        /// <param name="data">The data to return to the caller.</param>
        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data, ResultType = ServiceResultType.Ok };

        /// <summary>
        /// Creates a not found result (HTTP 404 Not Found).
        /// </summary>
        /// <param name="error">Error message describing what was not found.</param>
        public new static ServiceResult<T> NotFound(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.NotFound };

        /// <summary>
        /// Creates a conflict result (HTTP 409 Conflict).
        /// </summary>
        /// <param name="error">Error message describing the conflict.</param>
        public new static ServiceResult<T> Conflict(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.Conflict };

        /// <summary>
        /// Creates an unauthorized result (HTTP 401 Unauthorized).
        /// </summary>
        /// <param name="error">Error message describing the authorization failure.</param>
        public new static ServiceResult<T> Unauthorized(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.Unauthorized };

        /// <summary>
        /// Creates a bad request result (HTTP 400 Bad Request).
        /// </summary>
        /// <param name="error">Error message describing the invalid request.</param>
        public new static ServiceResult<T> BadRequest(string error) => new() { Success = false, ErrorMessage = error, ResultType = ServiceResultType.BadRequest };

        /// <summary>
        /// Creates a validation error result with multiple error messages (HTTP 400 Bad Request).
        /// </summary>
        /// <param name="errors">List of validation error messages.</param>
        public new static ServiceResult<T> ValidationError(List<string> errors) => new() { Success = false, Errors = errors, ResultType = ServiceResultType.BadRequest };
    }

    /// <summary>
    /// Enum representing the type of service result to map to HTTP status codes.
    /// This allows the service layer to declare the semantic intent of the result,
    /// which the controller layer then translates into proper HTTP responses.
    /// </summary>
    public enum ServiceResultType
    {
        /// <summary>
        /// Operation succeeded (HTTP 200 OK).
        /// </summary>
        Ok = 200,

        /// <summary>
        /// Invalid request data (HTTP 400 Bad Request).
        /// </summary>
        BadRequest = 400,

        /// <summary>
        /// Authentication failed or token invalid (HTTP 401 Unauthorized).
        /// </summary>
        Unauthorized = 401,

        /// <summary>
        /// Requested resource not found (HTTP 404 Not Found).
        /// </summary>
        NotFound = 404,

        /// <summary>
        /// Resource already exists or state conflict (HTTP 409 Conflict).
        /// </summary>
        Conflict = 409
    }
}
