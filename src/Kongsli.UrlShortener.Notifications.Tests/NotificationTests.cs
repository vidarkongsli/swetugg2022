using System.Net;
using Kongsli.UrlShortener.Models;
using Kongsli.UrlShortener.Notifications.Controllers;
using Kongsli.UrlShortener.Test.Helpers;
using RichardSzalay.MockHttp;

namespace Kongsli.UrlShortener.Notifications.Tests;

public class NotificationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public NotificationTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        => _factory = factory.WithOutput(output);

    [Fact]
    public async Task ShouldSendNotificationIfNoMatchEventReceived()
    {
        var bindingsHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("bindings");
        bindingsHttpClient.Clear();
        bindingsHttpClient.Expect(HttpMethod.Post, "http://localhost/email")
            .WithPartialContent("Short url /ex did not match")
            .Respond(HttpStatusCode.OK);

        bindingsHttpClient.Fallback.Respond(request => throw new Exception(
            $"HttpClient stub 'bindings' did not have knowledge of {request.Method} {request.RequestUri}"));
        
        var client = _factory.CreateClient();

        var requestEvent = new RequestEvent(Guid.NewGuid().ToString(),
            "/ex", false, DateTimeOffset.UtcNow,
            IpAddress: "127.0.0.1");
        
        var requestEventMessage = new DaprData<RequestEvent>(requestEvent);
        var result = await client.PostAsJsonAsync("/request", requestEventMessage);
        result.EnsureSuccessStatusCode();
        bindingsHttpClient.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ShouldNotSendNotificationIfMatchEventReceived()
    {
        var bindingsHttpClient = _factory.HttpClientFactory.CreateHttpResponsesFor("bindings");
        bindingsHttpClient.Clear();
        
        bindingsHttpClient.Fallback.Respond(request => throw new Exception(
            $"HttpClient stub 'bindings' did not have knowledge of {request.Method} {request.RequestUri}"));
        
        var client = _factory.CreateClient();

        var requestEvent = new RequestEvent(Guid.NewGuid().ToString(),
            "/ex", true, DateTimeOffset.UtcNow,
            IpAddress: "127.0.0.1");
        
        var requestEventMessage = new DaprData<RequestEvent>(requestEvent);
        var result = await client.PostAsJsonAsync("/request", requestEventMessage);
        result.EnsureSuccessStatusCode();
    }
}