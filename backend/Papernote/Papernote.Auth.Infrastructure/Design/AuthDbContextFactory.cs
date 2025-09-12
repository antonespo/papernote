using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Papernote.Auth.Infrastructure.Persistence;
using Papernote.SharedMicroservices.Database;

namespace Papernote.Auth.Infrastructure.Design;

/// <summary>
/// Design-time factory for creating DbContext during migrations
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(AuthDbContextFactory).Assembly, optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("AuthDatabase");

        connectionString = DatabaseConfiguration.ValidateConnectionString(
            connectionString,
            "AuthDatabase",
            "Auth Migration");

        DatabaseConfiguration.ConfigurePostgreSQL<AuthDbContext>(optionsBuilder, connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}