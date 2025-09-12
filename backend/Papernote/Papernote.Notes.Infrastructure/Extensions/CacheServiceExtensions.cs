using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Infrastructure.Services;
using Papernote.SharedMicroservices.Cache;

namespace Papernote.Notes.Infrastructure.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRedisCache(configuration);

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