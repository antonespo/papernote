using Microsoft.Extensions.Diagnostics.HealthChecks;
using Papernote.Notes.Infrastructure;
using Papernote.Notes.Infrastructure.Extensions;
using Papernote.Notes.Core.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<NoteMappingProfile>());

var connectionString = builder.Configuration.GetConnectionString("NotesDatabase")
    ?? throw new InvalidOperationException("Connection string 'NotesDatabase' not found in configuration.");

builder.Services.AddNotesInfrastructure(connectionString);

builder.Services.AddCacheServices(builder.Configuration);

builder.Services.AddCachedNoteService();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Notes API is running"));

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
