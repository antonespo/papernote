using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Papernote.SharedMicroservices.Results;

/// <summary>
/// Extensions for converting Result objects to proper HTTP responses using .NET 8 ProblemDetails
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IActionResult with proper status codes and ProblemDetails
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return CreateProblemDetailsResult(result.Error, result.ErrorCode);
    }

    /// <summary>
    /// Converts a Result<T> to an IActionResult with proper status codes and ProblemDetails
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is not null 
                ? new OkObjectResult(result.Value) 
                : new NoContentResult();
        }

        return CreateProblemDetailsResult(result.Error, result.ErrorCode);
    }

    /// <summary>
    /// Creates a ProblemDetails result using .NET built-in status codes
    /// </summary>
    private static IActionResult CreateProblemDetailsResult(string? error, string? errorCode)
    {
        var statusCode = GetStatusCodeFromErrorCode(errorCode);
        
        var problemDetails = new ProblemDetails
        {
            Title = GetTitleFromStatusCode(statusCode),
            Detail = error,
            Status = statusCode,
            Type = GetRfc7807TypeFromStatusCode(statusCode)
        };

        return new ObjectResult(problemDetails) 
        { 
            StatusCode = statusCode 
        };
    }

    /// <summary>
    /// Maps error codes to HTTP status codes using .NET StatusCodes
    /// </summary>
    private static int GetStatusCodeFromErrorCode(string? errorCode) => errorCode switch
    {
        nameof(StatusCodes.Status400BadRequest) => StatusCodes.Status400BadRequest,
        nameof(StatusCodes.Status401Unauthorized) => StatusCodes.Status401Unauthorized,
        nameof(StatusCodes.Status403Forbidden) => StatusCodes.Status403Forbidden,
        nameof(StatusCodes.Status404NotFound) => StatusCodes.Status404NotFound,
        nameof(StatusCodes.Status409Conflict) => StatusCodes.Status409Conflict,
        nameof(StatusCodes.Status422UnprocessableEntity) => StatusCodes.Status422UnprocessableEntity,
        nameof(StatusCodes.Status500InternalServerError) => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status400BadRequest
    };

    /// <summary>
    /// Gets a human-readable title for the error using .NET conventions
    /// </summary>
    private static string GetTitleFromStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "Error"
    };

    /// <summary>
    /// Gets the RFC 7807 problem type URI using standard specifications
    /// </summary>
    private static string GetRfc7807TypeFromStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    };
}