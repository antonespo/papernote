using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Infrastructure.Services;
using Papernote.SharedMicroservices.Cache;
using StackExchange.Redis;

namespace Papernote.Notes.Infrastructure.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRedisCache(configuration);

        var redisConnectionString = CacheConfiguration.ValidateRedisConnectionString(
            configuration.GetConnectionString("Redis"),
            "Notes Cache Service");

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
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ICacheKeyStrategy, NotesCacheKeyStrategy>();

        return services;
    }

    public static IServiceCollection AddCachedNoteService(this IServiceCollection services)
    {
        services.Decorate<INoteService, CachedNoteService>();
        services.AddScoped<ICachedNoteService>(provider =>
            (CachedNoteService)provider.GetRequiredService<INoteService>());

        return services;
    }
}