namespace Kongsli.UrlShortener.Models;

public interface IShortUrlService
{
    Task<ICollection<ShortUrl>> Get();
    Task<ShortUrl> Get(string shortPath);
    Task<ICollection<string>> GetInventory();
    Task Save(params ShortUrl[] shortUrls);
    Task Delete(string shortUrl);
}
