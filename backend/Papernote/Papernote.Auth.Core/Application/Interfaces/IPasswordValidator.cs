using Papernote.Auth.Core.Application.Configuration;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface IPasswordValidator
{
    bool IsValidPassword(string password, PasswordSettings settings);
    string GetValidationErrorMessage(string password, PasswordSettings settings);
}