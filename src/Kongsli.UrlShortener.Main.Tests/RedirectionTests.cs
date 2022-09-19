using System.Net;
using Kongsli.UrlShortener.Test.Helpers;
using RichardSzalay.MockHttp;

namespace Kongsli.UrlShortener.Main.Tests;

public class RedirectionTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _out;

    public RedirectionTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory.WithOutput(output);
        _out = output;          
    }

    private void InitializeDrivenAdapters()
    {
        var stateHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("state");
        stateHttpClient.Clear();
        stateHttpClient.When(HttpMethod.Get, "http://localhost/shorturls/ex")
            .Respond("application/json", EmbeddedResource.ReadTestData<RedirectionTests>("State", "GetShortUrlsExpressenResponse.json"));

        stateHttpClient.Fallback.Respond(request => {
            _out.WriteLine($"Request: {request.Method} {request.RequestUri}");
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        });

        var publishHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("publish");
        publishHttpClient.Clear();
        publishHttpClient
            .Expect(HttpMethod.Post, $"http://localhost/{RequestEventPublisher.PUBSUB_NAME}/{RequestEventPublisher.TOPIC_NAME}")
            .Respond(HttpStatusCode.NoContent);
    }

    private void VerifyDrivenAdapterExpectations()
    {
        _factory.HttpClientFactory.CreateHttpResponsesFor("publish").VerifyNoOutstandingExpectation();
    }
    
    [Theory]
    [InlineData("/ex", HttpStatusCode.Found)]
    [InlineData("", HttpStatusCode.NotFound)]
    public async Task ShouldRedirectIfFound(string path, HttpStatusCode expectedHttpStatusCode)
    {
        InitializeDrivenAdapters();
        var client = _factory.CreateClient();
        var result = await client.GetAsync(path);

        Assert.Equal(expectedHttpStatusCode, result.StatusCode);
        VerifyDrivenAdapterExpectations();
    }
}