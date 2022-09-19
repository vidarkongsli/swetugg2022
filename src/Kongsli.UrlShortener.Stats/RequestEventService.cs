using Kongsli.UrlShortener.Models;

namespace Kongsli.UrlShortener.Stats;

public class RequestEventService
{
    private readonly HttpClient _client;
    private const string STORE_NAME = "requestevents";

    public RequestEventService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(HttpClientShortUrlService.HTTP_CLIENT_NAME);
    }

    public async Task Save(params RequestEvent[] requestEvents)
    {
        var result = await _client.PostAsJsonAsync(STORE_NAME,
            requestEvents.Select(r => new SaveRequest<RequestEvent>(r.Id, r)));
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"When calling {result.RequestMessage?.RequestUri}, error was {result.StatusCode} {result.ReasonPhrase}");
        }
    }
}