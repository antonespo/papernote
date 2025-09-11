namespace Papernote.SharedMicroservices.Results;

public static class ResultBuilder
{
    public static Result Success() => Result.Success();
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result Failure(string error, string? errorCode = null) => Result.Failure(error, errorCode);
    public static Result<T> Failure<T>(string error, string? errorCode = null) => Result<T>.Failure(error, errorCode);

    public static Result<T> FromValidation<T>(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Result<T>.Failure("Validation failed but no errors provided.");
        var error = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        return Result<T>.Failure(error, "ValidationError");
    }

    public static Result FromValidation(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return Result.Success();
        var error = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        return Result.Failure(error, "ValidationError");
    }
}
