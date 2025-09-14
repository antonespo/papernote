using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Papernote.SharedMicroservices.Security;

public static class InternalApiSecurityExtensions
{
    public static IServiceCollection AddInternalApiSecurity(this IServiceCollection services)
    {
        services.AddScoped<InternalApiKeyAttribute>();
        return services;
    }

    public static IApplicationBuilder UseInternalApiSecurity(this IApplicationBuilder app)
    {
        return app.UseMiddleware<InternalApiKeyMiddleware>();
    }
}