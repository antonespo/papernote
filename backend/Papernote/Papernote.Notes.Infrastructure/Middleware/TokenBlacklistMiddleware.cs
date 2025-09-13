using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Papernote.Notes.Infrastructure.Services;
using Papernote.SharedMicroservices.Cache;
using System.IdentityModel.Tokens.Jwt;

namespace Papernote.Notes.Infrastructure.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenBlacklistMiddleware> _logger;

    public TokenBlacklistMiddleware(RequestDelegate next, ILogger<TokenBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAdvancedCacheService cacheService)
    {
        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..].Trim();

                try
                {
                    var jti = ExtractJtiFromToken(token);
                    if (!string.IsNullOrEmpty(jti))
                    {
                        var cacheKeyStrategy = new AuthTokenCacheKeyStrategy();
                        var cacheKey = cacheKeyStrategy.GetRevokedTokenKey(jti);

                        var isRevoked = await cacheService.ExistsAsync(cacheKey, context.RequestAborted);
                        if (isRevoked)
                        {
                            _logger.LogWarning("Revoked token attempted access: {JTI}", jti);
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Token has been revoked");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking token blacklist status");
                }
            }
        }

        await _next(context);
    }

    private static string? ExtractJtiFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return null;

            var jsonToken = handler.ReadJwtToken(token);
            return jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch
        {
            return null;
        }
    }
}

public static class TokenBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenBlacklist(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenBlacklistMiddleware>();
    }
}