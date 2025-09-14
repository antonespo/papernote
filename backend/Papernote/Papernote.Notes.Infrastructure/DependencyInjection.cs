using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.Notes.Infrastructure.Persistence;
using Papernote.Notes.Infrastructure.Repositories;
using Papernote.Notes.Infrastructure.Services;
using Papernote.SharedMicroservices.Database;
using Papernote.SharedMicroservices.Http;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace Papernote.Notes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotesInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        var validatedConnectionString = DatabaseConfiguration.ValidateConnectionString(
            connectionString, "NotesDatabase", "Notes Service");

        services.AddDbContext<NotesDbContext>(options =>
        {
            options.UseNpgsql(validatedConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "papernote");
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                npgsqlOptions.CommandTimeout(30);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteService, NoteService>();

        return services;
    }

    public static IServiceCollection AddNotesAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddAuthServiceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authServiceBaseUrl = configuration["AuthService:BaseUrl"]
            ?? throw new InvalidOperationException("AuthService:BaseUrl configuration is required");

        var authServiceApiKey = configuration["AuthService:ApiKey"]
            ?? throw new InvalidOperationException("AuthService:ApiKey configuration is required");

        services.AddTransient(_ => new InternalApiKeyHandler(authServiceApiKey));

        services.AddRefitClient<IAuthServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(authServiceBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<InternalApiKeyHandler>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddScoped<IAuthUserResolutionService, AuthUserResolutionService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}