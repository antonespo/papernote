using System.Net.Http.Headers;

namespace Papernote.SharedMicroservices.Http;

public class InternalApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;
    private const string ApiKeyHeaderName = "X-Internal-ApiKey";

    public InternalApiKeyHandler(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        request.Headers.Add(ApiKeyHeaderName, _apiKey);
        
        return await base.SendAsync(request, cancellationToken);
    }
}