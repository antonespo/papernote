using Microsoft.EntityFrameworkCore;

namespace Papernote.SharedMicroservices.Database;

/// <summary>
/// Shared database configuration for all microservices
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configure shared PostgreSQL database settings
    /// </summary>
    public static void ConfigurePostgreSQL<TContext>(
        DbContextOptionsBuilder<TContext> options,
        string connectionString) where TContext : DbContext
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "papernote");
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            npgsqlOptions.CommandTimeout(30);
        });

        // Development settings
#if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
#endif
    }

    /// <summary>
    /// Standard connection string validation
    /// </summary>
    public static string ValidateConnectionString(string? connectionString, string serviceName)
    {
        return connectionString
            ?? throw new InvalidOperationException(
                $"Connection string 'DefaultConnection' not found for {serviceName}. " +
                "Please configure it in appsettings.json or environment variables.");
    }
}
