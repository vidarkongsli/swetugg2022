namespace Kongsli.UrlShortener.Models;

public record SaveRequest<T>(string Key, T Value);
