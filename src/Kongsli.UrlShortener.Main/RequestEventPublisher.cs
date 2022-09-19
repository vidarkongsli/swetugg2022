using Kongsli.UrlShortener.Models;

namespace Kongsli.UrlShortener.Main;

public class RequestEventPublisher
{
    public const string HTTP_CLIENT_NAME = "publish";
    public const string PUBSUB_NAME = "urlshortener-pub-sub";
    public const string TOPIC_NAME = "requests";
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public RequestEventPublisher(IHttpClientFactory httpClientFactory, ILogger<RequestEventPublisher> logger)
    {
        _client = httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
        _logger = logger;
    }

    public async Task Publish(HttpRequest httpRequest, bool found)
    {
        var shortPath = httpRequest.Path.ToUriComponent()[1..];
        if (shortPath.Length == 0)
        {
            shortPath = "/";
        }
        var requestEvent = new RequestEvent(Guid.NewGuid().ToString(),
            shortPath, found, DateTimeOffset.UtcNow,
            IpAddress: httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString());
        _logger.LogInformation("Sending request event {event}", requestEvent);
        
        await _client.PostAsJsonAsync($"{PUBSUB_NAME}/{TOPIC_NAME}", requestEvent);
    }
}