using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapr;
using Kongsli.UrlShortener.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kongsli.UrlShortener.Stats.Controllers;

[ApiController]
public class EventLoggerController
{
    private const string PUBSUB_NAME = "urlshortener-pub-sub";
    private const string TOPIC_NAME = "requests";
    private readonly RequestEventService _requestEventService;
    private readonly ILogger<EventLoggerController> _logger;

    public EventLoggerController(RequestEventService requestEventService, ILogger<EventLoggerController> logger)
    {
        _requestEventService = requestEventService;
        _logger = logger;
    }

    //Subscribe to a topic 
    [Topic(PUBSUB_NAME, TOPIC_NAME)]
    [HttpPost("request")]
    public async Task<IActionResult> ReceiveRequestEvent([FromBody]DaprData<RequestEvent> message)
    {
        var requestEvent = message.Data;
        _logger.LogInformation("Received request event {requestEvent}", requestEvent);
        if (string.IsNullOrWhiteSpace(requestEvent.Id))
        {
            _logger.LogError("Received illegal request where Id is null or empty RequestEvent {requestEvent}", JsonSerializer.Serialize(requestEvent));
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }
        await _requestEventService.Save(requestEvent);
        return new OkResult();
    }
}

public record DaprData<T> ([property: JsonPropertyName("data")] T Data); 
