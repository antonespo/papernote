using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Infrastructure.Services;

namespace Papernote.Auth.Infrastructure.Extensions;

public static class AuthCacheServiceExtensions
{
    public static IServiceCollection AddCachedUserResolutionService(this IServiceCollection services)
    {
        services.Decorate<IUserResolutionService, CachedUserResolutionService>();

        services.AddScoped<ICachedUserResolutionService>(provider =>
            (CachedUserResolutionService)provider.GetRequiredService<IUserResolutionService>());

        return services;
    }
}