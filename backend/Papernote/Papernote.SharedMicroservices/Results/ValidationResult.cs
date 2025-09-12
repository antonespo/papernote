namespace Papernote.SharedMicroservices.Results;

public class ValidationError
{
    public string PropertyName { get; }
    public string ErrorMessage { get; }
    public string? ErrorCode { get; }

    public ValidationError(string propertyName, string errorMessage, string? errorCode = null)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }
}

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ValidationError> Errors { get; } = new();

    public ValidationResult() { }
    public ValidationResult(IEnumerable<ValidationError> errors)
    {
        Errors.AddRange(errors);
    }

    public static ValidationResult Success() => new();
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new(errors);
}
