using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Papernote.Auth.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : Attribute, IAsyncActionFilter
{
    public string Operation { get; }
    public string UsernameProperty { get; set; } = "Username";

    public RateLimitAttribute(string operation)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var rateLimitFilter = context.HttpContext.RequestServices
            .GetRequiredService<RateLimitActionFilter>();

        await rateLimitFilter.OnActionExecutionAsync(context, next, Operation, UsernameProperty);
    }
}