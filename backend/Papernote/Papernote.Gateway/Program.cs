using Microsoft.Extensions.Diagnostics.HealthChecks;
using Papernote.Gateway.HealthChecks;
using Papernote.SharedMicroservices.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection(CorsSettings.SectionName));

builder.Services.AddCors();

builder.Services.AddHttpClient();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Gateway is running"))
    .AddTypeActivatedCheck<AuthServiceHealthCheck>("auth-service")
    .AddTypeActivatedCheck<NotesServiceHealthCheck>("notes-service");

var app = builder.Build();

var corsSettings = app.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();
if (corsSettings?.AllowedOrigins?.Length > 0)
{
    app.UseCors(policy => policy
        .WithOrigins(corsSettings.AllowedOrigins)
        .WithMethods(corsSettings.AllowedMethods.Length > 0 ? corsSettings.AllowedMethods : ["GET", "POST", "PUT", "DELETE", "OPTIONS"])
        .WithHeaders(corsSettings.AllowedHeaders.Length > 0 ? corsSettings.AllowedHeaders : ["Content-Type", "Authorization"])
        .AllowCredentials());
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'");

    if (context.Request.IsHttps)
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    await next();
});

app.MapReverseProxy();

app.MapHealthChecks("/health");

app.Run();