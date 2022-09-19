using RichardSzalay.MockHttp;

namespace Kongsli.UrlShortener.Test.Helpers;

public class MockHttpClientFactory : IHttpClientFactory
{
    private readonly Dictionary<string, MockHttpMessageHandler> _handlers = new();

    public HttpClient CreateClient(string name)
    {
        var mockMessageHandler = _handlers.ContainsKey(name)
            ? _handlers[name] : new MockHttpMessageHandler();
        var httpClient = mockMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost/");
        return httpClient;
    }

    public MockHttpMessageHandler CreateHttpResponsesFor(string name)
    {
        if (!_handlers.ContainsKey(name))
        {
            _handlers.Add(name, new MockHttpMessageHandler());
        }
        return _handlers[name];
    }
}