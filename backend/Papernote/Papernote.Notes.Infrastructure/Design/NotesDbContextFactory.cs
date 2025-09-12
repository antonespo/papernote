using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Papernote.Notes.Infrastructure.Persistence;
using Papernote.SharedMicroservices.Database;

namespace Papernote.Notes.Infrastructure.Design;

/// <summary>
/// Design-time factory for creating DbContext during migrations
/// </summary>
public class NotesDbContextFactory : IDesignTimeDbContextFactory<NotesDbContext>
{
    public NotesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotesDbContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(NotesDbContextFactory).Assembly, optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("NotesDatabase");

        connectionString = DatabaseConfiguration.ValidateConnectionString(
            connectionString,
            "NotesDatabase",
            "Notes Migration");

        DatabaseConfiguration.ConfigurePostgreSQL<NotesDbContext>(optionsBuilder, connectionString);

        return new NotesDbContext(optionsBuilder.Options);
    }
}