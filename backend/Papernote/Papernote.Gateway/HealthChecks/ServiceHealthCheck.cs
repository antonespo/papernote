using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Papernote.Gateway.HealthChecks;

public class AuthServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthServiceHealthCheck(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authBaseUrl = _configuration["Gateway:Services:Auth:BaseUrl"] ?? "https://localhost:7001";
            var response = await _httpClient.GetAsync(new Uri($"{authBaseUrl}/health"), cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Auth service is reachable at {authBaseUrl}")
                : HealthCheckResult.Unhealthy($"Auth service at {authBaseUrl} returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            var authBaseUrl = _configuration["Gateway:Services:Auth:BaseUrl"] ?? "https://localhost:7001";
            return HealthCheckResult.Unhealthy($"Auth service check failed for {authBaseUrl}: {ex.Message}");
        }
    }
}

public class NotesServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public NotesServiceHealthCheck(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notesBaseUrl = _configuration["Gateway:Services:Notes:BaseUrl"] ?? "https://localhost:7002";
            var response = await _httpClient.GetAsync(new Uri($"{notesBaseUrl}/health"), cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Notes service is reachable at {notesBaseUrl}")
                : HealthCheckResult.Unhealthy($"Notes service at {notesBaseUrl} returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            var notesBaseUrl = _configuration["Gateway:Services:Notes:BaseUrl"] ?? "https://localhost:7002";
            return HealthCheckResult.Unhealthy($"Notes service check failed for {notesBaseUrl}: {ex.Message}");
        }
    }
}