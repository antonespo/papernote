using Microsoft.AspNetCore.Http;

namespace Papernote.SharedMicroservices.Results;

/// <summary>
/// Factory for creating Result objects using .NET built-in status codes
/// </summary>
public static class ResultBuilder
{
    #region Success Results
    
    public static Result Success() => Result.Success();
    
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    
    #endregion

    #region Error Results using .NET StatusCodes

    public static Result NotFound(string? message = null) => 
        Result.Failure(message ?? "Resource not found", GetErrorCode(StatusCodes.Status404NotFound));
    
    public static Result<T> NotFound<T>(string? message = null) => 
        Result<T>.Failure(message ?? "Resource not found", GetErrorCode(StatusCodes.Status404NotFound));

    public static Result BadRequest(string message) => 
        Result.Failure(message, GetErrorCode(StatusCodes.Status400BadRequest));
    
    public static Result<T> BadRequest<T>(string message) => 
        Result<T>.Failure(message, GetErrorCode(StatusCodes.Status400BadRequest));

    public static Result ValidationError(string message) => 
        Result.Failure(message, GetErrorCode(StatusCodes.Status422UnprocessableEntity));
    
    public static Result<T> ValidationError<T>(string message) => 
        Result<T>.Failure(message, GetErrorCode(StatusCodes.Status422UnprocessableEntity));

    public static Result Unauthorized(string? message = null) => 
        Result.Failure(message ?? "Unauthorized access", GetErrorCode(StatusCodes.Status401Unauthorized));
    
    public static Result<T> Unauthorized<T>(string? message = null) => 
        Result<T>.Failure(message ?? "Unauthorized access", GetErrorCode(StatusCodes.Status401Unauthorized));

    public static Result Forbidden(string? message = null) => 
        Result.Failure(message ?? "Access forbidden", GetErrorCode(StatusCodes.Status403Forbidden));
    
    public static Result<T> Forbidden<T>(string? message = null) => 
        Result<T>.Failure(message ?? "Access forbidden", GetErrorCode(StatusCodes.Status403Forbidden));

    public static Result Conflict(string message) => 
        Result.Failure(message, GetErrorCode(StatusCodes.Status409Conflict));
    
    public static Result<T> Conflict<T>(string message) => 
        Result<T>.Failure(message, GetErrorCode(StatusCodes.Status409Conflict));

    public static Result InternalServerError(string? message = null) => 
        Result.Failure(message ?? "An internal server error occurred", GetErrorCode(StatusCodes.Status500InternalServerError));
    
    public static Result<T> InternalServerError<T>(string? message = null) => 
        Result<T>.Failure(message ?? "An internal server error occurred", GetErrorCode(StatusCodes.Status500InternalServerError));

    #endregion

    #region Validation Results

    public static Result<T> FromValidation<T>(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Success<T>(default(T)!);
        
        var error = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        return ValidationError<T>(error);
    }

    public static Result FromValidation(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Success();
        
        var error = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        return ValidationError(error);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Converts HTTP status code to error code string using .NET conventions
    /// </summary>
    private static string GetErrorCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => nameof(StatusCodes.Status400BadRequest),
        StatusCodes.Status401Unauthorized => nameof(StatusCodes.Status401Unauthorized),
        StatusCodes.Status403Forbidden => nameof(StatusCodes.Status403Forbidden),
        StatusCodes.Status404NotFound => nameof(StatusCodes.Status404NotFound),
        StatusCodes.Status409Conflict => nameof(StatusCodes.Status409Conflict),
        StatusCodes.Status422UnprocessableEntity => nameof(StatusCodes.Status422UnprocessableEntity),
        StatusCodes.Status500InternalServerError => nameof(StatusCodes.Status500InternalServerError),
        _ => "UnknownError"
    };

    #endregion
}
