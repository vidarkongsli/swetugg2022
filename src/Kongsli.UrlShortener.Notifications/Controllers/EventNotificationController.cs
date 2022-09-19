using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kongsli.UrlShortener.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kongsli.UrlShortener.Notifications.Controllers;

public record DaprData<T> ([property: JsonPropertyName("data")] T Data); 

[ApiController]
public class EventNotificationController
{
    private readonly EventNotificationService _eventNotificationService;
    private readonly ILogger<EventNotificationController> _logger;

    public EventNotificationController(EventNotificationService eventNotificationService, ILogger<EventNotificationController> logger)
    {
        _eventNotificationService = eventNotificationService;
        _logger = logger;
    }

    [HttpPost("request")]
    public async Task<IActionResult> ReceiveRequestEvent([FromBody]DaprData<RequestEvent> message)
    {
        var requestEvent = message.Data;
        _logger.LogInformation("Received request event {requestEvent}", requestEvent);
        
        if (string.IsNullOrWhiteSpace(requestEvent.Id))
        {
            _logger.LogError("Received illegal request where Id is null or empty RequestEvent {requestEvent}",
                JsonSerializer.Serialize(requestEvent));
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }
        var wasSent = await _eventNotificationService.Notify(requestEvent);
        return wasSent ? new OkResult() : new StatusCodeResult((int)HttpStatusCode.TooManyRequests); 
    }
}