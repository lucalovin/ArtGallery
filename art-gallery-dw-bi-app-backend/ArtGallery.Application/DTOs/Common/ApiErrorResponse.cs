namespace ArtGallery.Application.DTOs.Common;

/// <summary>
/// API error response format.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed error information.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public List<ValidationError> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the error.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the request ID for tracing.
    /// </summary>
    public string? RequestId { get; set; }
}

/// <summary>
/// Represents a validation error for a specific field.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Gets or sets the field name that failed validation.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
