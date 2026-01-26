namespace SaveYourNote.Api.Models;

/// <summary>
/// Standard API response wrapper for successful operations
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Status of the operation
    /// </summary>
    /// <example>success</example>
    public string Status { get; set; } = "success";

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    /// <example>Message received and logged</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual data payload
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    /// <example>2026-01-26T10:30:00Z</example>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Standard API error response
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Status of the operation (always "error")
    /// </summary>
    /// <example>error</example>
    public string Status { get; set; } = "error";

    /// <summary>
    /// Error message
    /// </summary>
    /// <example>Invalid signature</example>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (optional)
    /// </summary>
    /// <example>The provided signature does not match the expected value</example>
    public string? Details { get; set; }

    /// <summary>
    /// Timestamp of the error
    /// </summary>
    /// <example>2026-01-26T10:30:00Z</example>
    public DateTime Timestamp { get; set; }
}
