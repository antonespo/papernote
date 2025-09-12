using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Core.Domain.Interfaces;
using Papernote.Auth.Infrastructure.Persistence;
using Papernote.Auth.Infrastructure.Repositories;
using Papernote.Auth.Infrastructure.Security;
using Papernote.Auth.Infrastructure.Services;
using Papernote.SharedMicroservices.Database;

namespace Papernote.Auth.Infrastructure;

/// <summary>
/// Extension methods for Infrastructure layer configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        var validatedConnectionString = DatabaseConfiguration.ValidateConnectionString(
            connectionString, "AuthDatabase", "Auth Service");

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(validatedConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "papernote");
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                npgsqlOptions.CommandTimeout(30);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // Repository registrations
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Security services
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordValidator, PasswordValidator>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserResolutionService, UserResolutionService>();

        return services;
    }
}