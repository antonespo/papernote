using Microsoft.Extensions.Diagnostics.HealthChecks;
using Papernote.Auth.Core.Application.Configuration;
using Papernote.Auth.Core.Application.Mappings;
using Papernote.Auth.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Services.Configure<AuthSettings>(
    builder.Configuration.GetSection(AuthSettings.SectionName));

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AuthMappingProfile>());


// Infrastructure services
var connectionString = builder.Configuration.GetConnectionString("AuthDatabase")
    ?? throw new InvalidOperationException("Connection string 'AuthDatabase' not found in configuration.");

builder.Services.AddAuthInfrastructure(connectionString);

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Auth API is running"))
    .AddNpgSql(connectionString, name: "auth-database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
