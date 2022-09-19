using System.Text.Json;
using Kongsli.UrlShortener.Models;
using Kongsli.UrlShortener.Notifications.Models;

namespace Kongsli.UrlShortener.Notifications;

public class EventNotificationService
{
    public const string HTTP_CLIENT_NAME = "bindings";
    private const string BINDING_NAME = "email";
    
    private readonly HttpClient _client;
    private readonly ILogger<EventNotificationService> _logger;

    public EventNotificationService(IHttpClientFactory httpClientFactory, ILogger<EventNotificationService> logger)
    {
        _client = httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
        _logger = logger;
    }

    internal async Task<bool> Notify(RequestEvent requestEvent)
    {
        if (!requestEvent.IsMatch && requestEvent.ShortPath != "/")
        {
            var notificationMessage = ToNotificationMessage(requestEvent);
            _logger.LogInformation("Sending notification message {message} for event {eventId}",
                JsonSerializer.Serialize(notificationMessage), requestEvent.Id);
            var result = await _client.PostAsJsonAsync(BINDING_NAME, notificationMessage);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("Error sending notification  {message} for event {eventId}",
                    JsonSerializer.Serialize(notificationMessage), requestEvent.Id);
                    return false;
            }
        }
        else
        {
            _logger.LogInformation("Event {eventId} did not match notification criteria.", requestEvent.Id);
        }
        return true;
    }

    private static EmailMessage ToNotificationMessage(RequestEvent requestEvent)
        => new("Unmatched short url", $"Short url {requestEvent.ShortPath} did not match any existing rule.");
}