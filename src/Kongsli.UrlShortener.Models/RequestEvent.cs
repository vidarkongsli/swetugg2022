namespace Kongsli.UrlShortener.Models;

public record RequestEvent (string Id, string ShortPath, bool IsMatch, DateTimeOffset TimeStamp,
    string? Browser = null, string? Region = null, string? Country = null, string? IpAddress = null);