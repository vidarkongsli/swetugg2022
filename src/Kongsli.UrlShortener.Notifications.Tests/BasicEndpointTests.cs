using System.Net;
using Kongsli.UrlShortener.Test.Helpers;

namespace Kongsli.UrlShortener.Notifications.Tests;

public class BasicEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BasicEndpointTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        => _factory = factory.WithOutput(output);

    [Theory]
    [InlineData("/swagger", HttpStatusCode.MovedPermanently)]
    [InlineData("/swagger/index.html", HttpStatusCode.OK)]
    [InlineData("/health", HttpStatusCode.OK)]
    public async Task GetEndpoints(string url, HttpStatusCode expectedStatusCode)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}