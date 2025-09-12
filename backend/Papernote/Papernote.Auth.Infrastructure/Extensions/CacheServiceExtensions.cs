using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Infrastructure.Cache;
using Papernote.Auth.Infrastructure.Services;
using Papernote.SharedMicroservices.Cache;

namespace Papernote.Auth.Infrastructure.Extensions;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddAuthCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRedisCache(configuration);

        services.AddScoped<IAuthCacheKeyStrategy, AuthCacheKeyStrategy>();
        services.AddScoped<AuthCacheService>();

        return services;
    }
}