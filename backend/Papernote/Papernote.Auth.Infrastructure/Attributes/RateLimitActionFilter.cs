using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Papernote.Auth.Core.Application.Interfaces;
using System.Reflection;
using System.Text.Json;

namespace Papernote.Auth.Infrastructure.Attributes;

/// <summary>
/// Enterprise action filter for login rate limiting only
/// </summary>
public class RateLimitActionFilter
{
    private readonly IRateLimitService _rateLimitService;
    private readonly ILogger<RateLimitActionFilter> _logger;

    public RateLimitActionFilter(
        IRateLimitService rateLimitService,
        ILogger<RateLimitActionFilter> logger)
    {
        _rateLimitService = rateLimitService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next,
        string operation,
        string usernameProperty)
    {
        // Only apply rate limiting to login operations
        if (operation?.ToLowerInvariant() != "login")
        {
            await next();
            return;
        }

        var username = ExtractUsername(context, usernameProperty);
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogDebug("Could not extract username for login rate limiting");
            await next();
            return;
        }

        var rateLimitResult = await _rateLimitService.CheckAttemptAsync(username, "login", context.HttpContext.RequestAborted);
        if (!rateLimitResult.IsSuccess)
        {
            _logger.LogError("Rate limit service error for login: {Error}", rateLimitResult.Error);
            await next();
            return;
        }

        if (!rateLimitResult.Value.IsAllowed)
        {
            await HandleRateLimitExceeded(context, rateLimitResult.Value.RetryAfter);
            return;
        }

        var executedContext = await next();

        if (IsAuthFailure(executedContext))
        {
            var recordResult = await _rateLimitService.RecordFailedAttemptAsync(username, "login", context.HttpContext.RequestAborted);
            if (!recordResult.IsSuccess)
            {
                _logger.LogWarning("Failed to record login rate limit attempt: {Error}", recordResult.Error);
            }
        }
    }

    private string? ExtractUsername(ActionExecutingContext context, string usernameProperty)
    {
        try
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var usernameValue = GetPropertyValue(argument, usernameProperty);
                if (!string.IsNullOrWhiteSpace(usernameValue))
                {
                    return usernameValue;
                }
            }

            _logger.LogDebug("Username property '{Property}' not found in action arguments", usernameProperty);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting username from action arguments");
            return null;
        }
    }

    private static string? GetPropertyValue(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        var value = property?.GetValue(obj);
        return value?.ToString();
    }

    private static bool IsAuthFailure(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            return objectResult.StatusCode == StatusCodes.Status401Unauthorized ||
                   objectResult.StatusCode == StatusCodes.Status400BadRequest ||
                   objectResult.StatusCode == StatusCodes.Status422UnprocessableEntity;
        }

        if (context.Result is StatusCodeResult statusResult)
        {
            return statusResult.StatusCode == StatusCodes.Status401Unauthorized ||
                   statusResult.StatusCode == StatusCodes.Status400BadRequest ||
                   statusResult.StatusCode == StatusCodes.Status422UnprocessableEntity;
        }

        return false;
    }

    private async Task HandleRateLimitExceeded(ActionExecutingContext context, TimeSpan? retryAfter)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        response.ContentType = "application/problem+json";

        if (retryAfter.HasValue)
        {
            response.Headers.RetryAfter = ((int)retryAfter.Value.TotalSeconds).ToString();
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Title = "Too Many Requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Too many login attempts. Please try again later.",
            Instance = context.HttpContext.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Result = new ContentResult
        {
            Content = json,
            ContentType = "application/problem+json",
            StatusCode = StatusCodes.Status429TooManyRequests
        };

        await Task.CompletedTask;
    }
}