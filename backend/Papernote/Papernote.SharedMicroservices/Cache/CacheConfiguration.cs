using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Papernote.SharedMicroservices.Cache;

/// <summary>
/// Shared Redis cache configuration for all microservices
/// </summary>
public static class CacheConfiguration
{
    /// <summary>
    /// Configure Redis distributed cache
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ValidateRedisConnectionString(
            configuration.GetConnectionString("Redis"),
            "Redis Cache");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "PaperNote";
        });

        return services;
    }

    /// <summary>
    /// Standard Redis connection string validation
    /// </summary>
    public static string ValidateRedisConnectionString(string? connectionString, string serviceName)
    {
        return connectionString
            ?? throw new InvalidOperationException(
                $"Redis connection string not found for {serviceName}. " +
                "Please configure 'Redis' connection string in appsettings.json or environment variables.");
    }
}