using System.Net;
using System.Text.Json;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.Exceptions;
using Oracle.ManagedDataAccess.Client;

namespace ArtGallery.API.Middleware;

/// <summary>
/// Global exception handling middleware with Oracle-specific error handling.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // Oracle error codes
    private const int ORA_UNIQUE_CONSTRAINT = 1;        // ORA-00001: unique constraint violated
    private const int ORA_FK_CONSTRAINT = 2291;         // ORA-02291: integrity constraint violated - parent key not found
    private const int ORA_FK_CHILD_RECORD = 2292;       // ORA-02292: integrity constraint violated - child record found
    private const int ORA_CHECK_CONSTRAINT = 2290;      // ORA-02290: check constraint violated
    private const int ORA_NOT_NULL_CONSTRAINT = 1400;   // ORA-01400: cannot insert NULL
    private const int ORA_VALUE_TOO_LARGE = 12899;      // ORA-12899: value too large for column
    private const int ORA_INVALID_NUMBER = 1722;        // ORA-01722: invalid number
    private const int ORA_DEADLOCK = 60;                // ORA-00060: deadlock detected
    private const int ORA_RESOURCE_BUSY = 54;           // ORA-00054: resource busy and acquire with NOWAIT
    private const int ORA_TABLE_OR_VIEW_MISSING = 942;  // ORA-00942: table or view does not exist

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;

        var (statusCode, message, details) = exception switch
        {
            OracleException oracleEx => HandleOracleException(oracleEx),
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message, (string?)null),
            ValidationException validation => (HttpStatusCode.BadRequest, validation.Message, string.Join("; ", validation.Errors.SelectMany(e => e.Value))),
            ConflictException conflict => (HttpStatusCode.Conflict, conflict.Message, (string?)null),
            BusinessRuleException business => (HttpStatusCode.BadRequest, business.Message, (string?)null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access", (string?)null),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "The operation was cancelled or timed out", (string?)null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", (string?)null)
        };

        _logger.LogError(exception, 
            "Error handling request {RequestId}: {Message}", 
            requestId, exception.Message);

        var errorResponse = new ApiErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            Details = details ?? (exception is ValidationException validationEx 
                ? string.Join("; ", validationEx.Errors.SelectMany(e => e.Value)) 
                : null),
            RequestId = requestId,
            ValidationErrors = exception is ValidationException ve 
                ? ve.Errors.SelectMany(e => e.Value.Select(m => new ValidationError 
                    { Field = e.Key, Message = m })).ToList() 
                : new List<ValidationError>()
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    private (HttpStatusCode statusCode, string message, string? details) HandleOracleException(OracleException ex)
    {
        return ex.Number switch
        {
            ORA_UNIQUE_CONSTRAINT => (
                HttpStatusCode.Conflict,
                "A record with the same unique identifier already exists.",
                ExtractConstraintName(ex.Message)
            ),
            ORA_FK_CONSTRAINT => (
                HttpStatusCode.BadRequest,
                "The referenced record does not exist. Please ensure all foreign key references are valid.",
                ExtractConstraintName(ex.Message)
            ),
            ORA_FK_CHILD_RECORD => (
                HttpStatusCode.Conflict,
                "Cannot delete this record because it is referenced by other records.",
                ExtractConstraintName(ex.Message)
            ),
            ORA_CHECK_CONSTRAINT => (
                HttpStatusCode.BadRequest,
                "The data violates a validation rule. Please check the values and try again.",
                ExtractConstraintName(ex.Message)
            ),
            ORA_NOT_NULL_CONSTRAINT => (
                HttpStatusCode.BadRequest,
                "A required field is missing. Please provide all mandatory values.",
                ExtractColumnName(ex.Message)
            ),
            ORA_VALUE_TOO_LARGE => (
                HttpStatusCode.BadRequest,
                "One or more values exceed the maximum allowed length.",
                ExtractColumnInfo(ex.Message)
            ),
            ORA_INVALID_NUMBER => (
                HttpStatusCode.BadRequest,
                "Invalid numeric value provided.",
                null
            ),
            ORA_DEADLOCK => (
                HttpStatusCode.Conflict,
                "A database deadlock was detected. Please retry the operation.",
                null
            ),
            ORA_RESOURCE_BUSY => (
                HttpStatusCode.Conflict,
                "The resource is currently busy. Please retry the operation.",
                null
            ),
            ORA_TABLE_OR_VIEW_MISSING => (
                HttpStatusCode.InternalServerError,
                "The target table or view is missing or access is denied. Please verify the Oracle schema and privileges.",
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                $"A database error occurred (ORA-{ex.Number:D5}).",
                null
            )
        };
    }

    private static string? ExtractConstraintName(string message)
    {
        // Extract constraint name from Oracle error message
        // Format: "ORA-xxxxx: ... (SCHEMA.CONSTRAINT_NAME) ..."
        var match = System.Text.RegularExpressions.Regex.Match(message, @"\(([^)]+\.[^)]+)\)");
        return match.Success ? $"Constraint: {match.Groups[1].Value}" : null;
    }

    private static string? ExtractColumnName(string message)
    {
        // Extract column name from Oracle error message
        var match = System.Text.RegularExpressions.Regex.Match(message, "\"([^\"]+)\"");
        return match.Success ? $"Column: {match.Groups[1].Value}" : null;
    }

    private static string? ExtractColumnInfo(string message)
    {
        // Extract column size info from ORA-12899
        var match = System.Text.RegularExpressions.Regex.Match(message, @"actual: (\d+), maximum: (\d+)");
        if (match.Success)
        {
            return $"Value length {match.Groups[1].Value} exceeds maximum of {match.Groups[2].Value}";
        }
        return null;
    }
}

/// <summary>
/// Extension method for registering the exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
