using System.Net;
using System.Net.Http.Json;

namespace Kongsli.UrlShortener.Models;

public class HttpClientShortUrlService : IShortUrlService
{
    public const string HTTP_CLIENT_NAME = "state";
    private readonly HttpClient _client;
    private const string STORE_NAME = "shorturls";
    private const string INVENTORY_KEY = "urls-inventory";

    public HttpClientShortUrlService(IHttpClientFactory httpClientFactory)
        => _client = httpClientFactory.CreateClient(HTTP_CLIENT_NAME);

    public async Task<ShortUrl> Get(string shortPath)
    {
        if (shortPath.Length == 0) return ShortUrl.Empty();
        var result = await _client.GetAsync($"{STORE_NAME}/{shortPath}");
        if (result.StatusCode == HttpStatusCode.NoContent) return ShortUrl.Empty();
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<ShortUrl>() ?? ShortUrl.Empty();
        }
        throw new Exception($"When calling {result.RequestMessage?.RequestUri}, error was {result.ReasonPhrase}");
    }

    public async Task<ICollection<ShortUrl>> Get()
    {
        var shortUrlList = new List<ShortUrl>();
        var inventory = await GetInventory();
        foreach (var path in inventory)
        {
            if (await Get(path) is { } shortUrl)
            {
                shortUrlList.Add(shortUrl);
            }
        }
        return shortUrlList.AsReadOnly();
    }

    public async Task<ICollection<string>> GetInventory()
    {
        var result = await _client.GetAsync($"{STORE_NAME}/{INVENTORY_KEY}");
        if (result.StatusCode == HttpStatusCode.NoContent) return Array.Empty<string>();
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<ICollection<string>>() ?? Array.Empty<string>();
        }
        throw new Exception($"When calling {result.RequestMessage?.RequestUri}, error was {result.StatusCode} {result.ReasonPhrase}");
    }

    public async Task Save(params ShortUrl[] shortUrls)
    {
        var result = await _client.PostAsJsonAsync(STORE_NAME,
            shortUrls.Select(shortUrl => new SaveRequest<ShortUrl>(shortUrl.ShortPath, shortUrl)));
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"When calling {result.RequestMessage?.RequestUri}, error was {result.ReasonPhrase}");
        }
        var inventory = new List<string>(await GetInventory());
        var added = false;
        foreach (var shortUrl in shortUrls)
        {
            if (!inventory.Contains(shortUrl.ShortPath))
            {
                inventory.Add(shortUrl.ShortPath);
                added = true;
            }
        }
        if (added)
        {
            await SaveInventory(inventory);
        }
    }

    public async Task Delete(string shortUrl)
    {
        var inventory = await GetInventory();
        if (inventory.Contains(shortUrl))
        {
            inventory.Remove(shortUrl);
            await SaveInventory(inventory);
            var result = await _client.DeleteAsync($"{STORE_NAME}/{shortUrl}");
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"When calling {result.RequestMessage?.RequestUri}, error was {result.ReasonPhrase}");
            }
        }
    }

    private async Task SaveInventory(ICollection<string> inventory)
    {
        var result = await _client.PostAsJsonAsync($"{STORE_NAME}", new [] {new
        {
            Key = INVENTORY_KEY,
            Value = inventory
        }});
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"When calling {result.RequestMessage?.RequestUri}, error was {result.StatusCode} {result.ReasonPhrase}");
        }
    }
}
