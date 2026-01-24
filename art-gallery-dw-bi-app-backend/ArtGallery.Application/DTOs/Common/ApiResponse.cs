namespace ArtGallery.Application.DTOs.Common;

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the list of errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the response.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failure response with errors.
    /// </summary>
    public static ApiResponse<T> FailureResponse(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = new List<string> { error }
        };
    }

    /// <summary>
    /// Creates a failure response with multiple errors.
    /// </summary>
    public static ApiResponse<T> FailureResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors
        };
    }
}
