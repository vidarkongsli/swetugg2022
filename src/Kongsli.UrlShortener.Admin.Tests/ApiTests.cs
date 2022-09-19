using System.Net;
using Kongsli.UrlShortener.Models;
using Kongsli.UrlShortener.Test.Helpers;
using RichardSzalay.MockHttp;

namespace Kongsli.UrlShortener.Admin.Tests;

public class ApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ApiTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        => _factory = factory.WithOutput(output);

    private void InitializeDrivenAdapters()
    {
        var stateHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("state");
        stateHttpClient.Clear();
        stateHttpClient.When(HttpMethod.Get, "http://localhost/shorturls/urls-inventory")
            .Respond("application/json", EmbeddedResource.ReadTestData<ApiTests>("State", "GetShortUrlsResponse.json"));

        stateHttpClient.When(HttpMethod.Get, "http://localhost/shorturls/ex")
            .Respond("application/json", EmbeddedResource.ReadTestData<ApiTests>("State", "GetShortUrlsExpressenResponse.json"));

        stateHttpClient.When(HttpMethod.Get, "http://localhost/shorturls/saob")
            .Respond("application/json", EmbeddedResource.ReadTestData<ApiTests>("State", "GetShortUrlsSaobResponse.json"));

        stateHttpClient.When(HttpMethod.Get, "http://localhost/shorturls/af")
            .Respond("application/json", EmbeddedResource.ReadTestData<ApiTests>("State", "GetShortUrlsAftonbladetResponse.json"));

    //    stateHttpClient.Fallback.Respond(request => throw new Exception($"HttpClient stub 'state' did not have knowledge of {request.Method} {request.RequestUri}"));
    }

    [Fact]
    public async Task ShouldBeAbleToListShortUrls()
    {
        InitializeDrivenAdapters();
        var client = _factory.CreateClient();
        var result = await client.GetAsync("/api/shorturls");
        result.EnsureSuccessStatusCode();

        var shortUrlList = await result.Content.ReadFromJsonAsync<ICollection<ShortUrl>>();
        Assert.Equal(3, shortUrlList?.Count);
        Assert.False(shortUrlList?.Single(s => s.ShortPath == "saob").IsEmpty);
    }

    [Theory]
    [InlineData("af")]
    [InlineData("ex")]
    [InlineData("saob")]
    public async Task ShouldBeAbleToRetrieveShortUrls(string shortUrl)
    {
        InitializeDrivenAdapters();
        var client = _factory.CreateClient();
        var result = await client.GetAsync($"/api/shorturls/{shortUrl}");
        result.EnsureSuccessStatusCode();

        var shortUrlData = await result.Content.ReadFromJsonAsync<ShortUrl>();

        Assert.False(shortUrlData?.IsEmpty);
    }

    [Fact]
    public async Task ShouldBeAbleToAddShortUrl()
    {
        InitializeDrivenAdapters();

        var stateHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("state");

        stateHttpClient.When(HttpMethod.Get, "http://localhost/shorturls/ch")
            .Respond(HttpStatusCode.NoContent);

        stateHttpClient.When(HttpMethod.Post, "http://localhost/shorturls")
            .WithPartialContent("chalmers").Respond(HttpStatusCode.NoContent);

        stateHttpClient.When(HttpMethod.Post, "http://localhost/shorturls")
            .WithPartialContent("urls-inventory").Respond(HttpStatusCode.NoContent);

        var client = _factory.CreateClient();
        var result = await client.PostAsJsonAsync("/api/shorturls", new ShortUrl(new Uri("https://www.chalmers.se/"), "ch"));
        result.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldBeAbleToDeleteShortUrl()
    {
        InitializeDrivenAdapters();
        var stateHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("state");
        var storeDeleteCall = stateHttpClient.When(HttpMethod.Delete, "http://localhost/shorturls/ex")
            .Respond(HttpStatusCode.NoContent);

        var updateInventoryCall = stateHttpClient.When(HttpMethod.Post, "http://localhost/shorturls")
            .With(request => {
                if (request.Content == null) return false;
                var requestBody = request.Content.ReadFromJsonAsync<ICollection<SaveRequest<ICollection<string>>>>().Result;
                if (requestBody == null) return false;
                return Enumerable.SequenceEqual(requestBody.First().Value.OrderBy(y => y), new[] { "af", "saob" });
            })
            .Respond(HttpStatusCode.NoContent);

        var client = _factory.CreateClient();
        var result = await client.DeleteAsync("/api/shorturls/ex");

        result.EnsureSuccessStatusCode();
        Assert.Equal(1, stateHttpClient.GetMatchCount(storeDeleteCall));
        Assert.Equal(1, stateHttpClient.GetMatchCount(updateInventoryCall));
    }
}