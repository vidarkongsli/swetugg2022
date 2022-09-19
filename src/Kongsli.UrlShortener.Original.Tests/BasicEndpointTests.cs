namespace Kongsli.UrlShortener.Original.Tests;

using Kongsli.UrlShortener.Original.Tests.Helpers;

public class BasicEndpointTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BasicEndpointTests(CustomWebApplicationFactory<Program> factory)
        => _factory = factory;

    [Theory]
    [InlineData("/swagger")]
    [InlineData("/health")]
    public async Task GetEndpoints(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();    
    }
}