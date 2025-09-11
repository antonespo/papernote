using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.Notes.Infrastructure.Persistence;
using Papernote.Notes.Infrastructure.Repositories;
using Papernote.Notes.Infrastructure.Services;
using Papernote.SharedMicroservices.Database;

namespace Papernote.Notes.Infrastructure;

/// <summary>
/// Extension methods for Infrastructure layer configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddNotesInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        var validatedConnectionString = DatabaseConfiguration.ValidateConnectionString(
            connectionString, "Notes Service");

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
}