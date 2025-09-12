using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Papernote.SharedMicroservices.Cache;

public static class CacheConfiguration
{
    public static string ValidateRedisConnectionString(string? connectionString, string serviceName)
    {
        return connectionString
            ?? throw new InvalidOperationException(
                $"Redis connection string not found for {serviceName}. " +
                "Please configure 'ConnectionStrings:Redis' in appsettings.json or environment variables.");
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = ValidateRedisConnectionString(
            configuration.GetConnectionString("Redis"),
            "Microservice Cache");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configOptions = ConfigurationOptions.Parse(redisConnectionString);
            configOptions.AbortOnConnectFail = false;
            configOptions.ConnectRetry = 3;
            configOptions.ConnectTimeout = 5000;
            configOptions.SyncTimeout = 5000;
            return ConnectionMultiplexer.Connect(configOptions);
        });

        services.AddScoped<IBaseCacheService, BaseCacheService>();
        services.AddScoped<IAdvancedCacheService, BaseCacheService>();

        return services;
    }

    public static bool IsValidCacheKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        // Pattern: service:version:operation:segments
        var parts = key.Split(':');
        return parts.Length >= 3 &&
               !string.IsNullOrWhiteSpace(parts[0]) && // service
               !string.IsNullOrWhiteSpace(parts[1]) && // version
               !string.IsNullOrWhiteSpace(parts[2]);   // operation
    }

    public static string? GetServiceFromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var parts = key.Split(':');
        return parts.Length > 0 ? parts[0] : null;
    }

    public static string? GetVersionFromKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var parts = key.Split(':');
        return parts.Length > 1 ? parts[1] : null;
    }
}