using Microsoft.AspNetCore.Mvc;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.API.Extensions;

/// <summary>
/// Extension methods to reduce controller logic duplication
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Validates ModelState and returns validation error result if invalid
    /// </summary>
    public static Result? ValidateModelState(this ControllerBase controller)
    {
        if (controller.ModelState.IsValid)
            return null;

        var errors = controller.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
            .ToList();

        var errorMessage = string.Join("; ", errors);
        return ResultBuilder.ValidationError(errorMessage);
    }
}