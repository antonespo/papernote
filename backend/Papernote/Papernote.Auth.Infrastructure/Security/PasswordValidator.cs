using Papernote.Auth.Core.Application.Configuration;
using Papernote.Auth.Core.Application.Interfaces;
using System.Text.RegularExpressions;

namespace Papernote.Auth.Infrastructure.Security;

public class PasswordValidator : IPasswordValidator
{
    public bool IsValidPassword(string password, PasswordSettings settings)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < settings.RequiredLength)
            return false;

        if (settings.RequireDigit && !password.Any(char.IsDigit))
            return false;

        if (settings.RequireUppercase && !password.Any(char.IsUpper))
            return false;

        if (settings.RequireLowercase && !password.Any(char.IsLower))
            return false;

        if (settings.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            return false;

        return true;
    }

    public string GetValidationErrorMessage(string password, PasswordSettings settings)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required";

        var errors = new List<string>();

        if (password.Length < settings.RequiredLength)
            errors.Add($"Password must be at least {settings.RequiredLength} characters long");

        if (settings.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit");

        if (settings.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        if (settings.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        if (settings.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            errors.Add("Password must contain at least one special character");

        return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
    }
}