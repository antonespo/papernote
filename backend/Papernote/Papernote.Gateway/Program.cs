using Papernote.SharedMicroservices.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS configuration
builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection(CorsSettings.SectionName));

builder.Services.AddCors();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure CORS
var corsSettings = app.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();
if (corsSettings?.AllowedOrigins?.Length > 0)
{
    app.UseCors(policy => policy
        .WithOrigins(corsSettings.AllowedOrigins)
        .WithMethods(corsSettings.AllowedMethods.Length > 0 ? corsSettings.AllowedMethods : ["GET", "POST", "PUT", "DELETE", "OPTIONS"])
        .WithHeaders(corsSettings.AllowedHeaders.Length > 0 ? corsSettings.AllowedHeaders : ["Content-Type", "Authorization"])
        .AllowCredentials());
}

// Security Headers
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

// Map YARP routes
app.MapReverseProxy();

// Health checks
app.MapHealthChecks("/health");

app.Run();