using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Papernote.Notes.Core.Application.Mappings;
using Papernote.Notes.Infrastructure;
using Papernote.Notes.Infrastructure.Extensions;
using Papernote.Notes.Infrastructure.Middleware;
using Papernote.SharedMicroservices.Configuration;
using Papernote.SharedMicroservices.Security;

var builder = WebApplication.CreateBuilder(args);

Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "PaperNote Notes API",
        Description = "RESTful API for managing notes, tags and sharing functionality in the PaperNote application",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Antonio Esposito",
            Url = new Uri("https://github.com/antonespo/papernote")
        }
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT Bearer token obtained from the Auth API"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                    }
                },
                Array.Empty<string> ()
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.EnableAnnotations();
    options.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ??
    throw new InvalidOperationException("Jwt:SigningKey configuration is required");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "papernote-auth",
            ValidAudience = "papernote-api",
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection(CorsSettings.SectionName));

var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ??
    new CorsSettings { PolicyName = "DefaultCorsPolicy" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings.PolicyName, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin();
        }

        policy.AllowAnyMethod()
              .AllowAnyHeader();

        if (allowedOrigins.Length > 0 && !allowedOrigins.Contains("*"))
        {
            policy.AllowCredentials();
        }
    });
});

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<NoteMappingProfile>());

var connectionString = builder.Configuration.GetConnectionString("NotesDatabase") ??
    throw new InvalidOperationException("Connection string 'NotesDatabase' not found in configuration.");

builder.Services.AddNotesInfrastructure(connectionString);
builder.Services.AddNotesAuthentication(builder.Configuration);
builder.Services.AddAuthServiceClient(builder.Configuration);

builder.Services.AddInternalApiSecurity();

builder.Services.AddCacheServices(builder.Configuration);
builder.Services.AddCachedNoteService();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Notes API is running"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PaperNote Notes API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "PaperNote Notes API";
    });
}

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseCors(corsSettings.PolicyName);

app.UseInternalApiSecurity();

app.UseAuthentication();
app.UseTokenBlacklist();
app.UseAuthorization();

app.MapControllers();

app.Run();