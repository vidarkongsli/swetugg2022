using Dapr.Client;

namespace Kongsli.UrlShortener.Models;

public class ShortUrlService : IShortUrlService
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "shorturls";
    private const string INVENTORY_KEY = "urls-inventory";

    public ShortUrlService(DaprClient daprClient) => _daprClient = daprClient;

    public async Task<ShortUrl> Get(string shortPath)
    {
        if (shortPath.Length == 0) return ShortUrl.Empty();
        if (await _daprClient.GetStateAsync<ShortUrl>(STORE_NAME, shortPath) is {} shortUrl) {
            return shortUrl;
        }
        return ShortUrl.Empty();
    }

    public async Task<ICollection<ShortUrl>> Get()
    {
        var shortUrlList = new List<ShortUrl>();
        var inventory = await GetInventory();
        foreach (var path in inventory)
        {
            if (await Get(path) is {} shortUrl)
            {
                shortUrlList.Add(shortUrl);
            }
        }
        return shortUrlList.AsReadOnly();
    }

    public async Task<ICollection<string>> GetInventory()
        => await _daprClient.GetStateAsync<ICollection<string>>(STORE_NAME, INVENTORY_KEY)
            ?? Array.Empty<string>();

    public async Task Save(params ShortUrl[] shortUrls)
    {
        var inventory = new List<string>(await GetInventory());
        var added = false;
        foreach (var shortUrl in shortUrls)
        {
            if (!inventory.Contains(shortUrl.ShortPath))
            {
                inventory.Add(shortUrl.ShortPath);
                added = true;
            }
            await _daprClient.SaveStateAsync(STORE_NAME, shortUrl.ShortPath, shortUrl);
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
            await _daprClient.DeleteStateAsync(STORE_NAME, shortUrl);
        }
    }

    private Task SaveInventory(ICollection<string> inventory) => _daprClient.SaveStateAsync(STORE_NAME, INVENTORY_KEY, inventory);
}