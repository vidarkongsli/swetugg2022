namespace Kongsli.UrlShortener.Models;

public record ShortUrl(Uri Location, string ShortPath, bool IsEmpty = false)
{
    public static ShortUrl Empty() => new(new Uri("https://icann.org"), string.Empty, true);
}
