using Kongsli.UrlShortener.Models;
using Microsoft.EntityFrameworkCore;

namespace Kongsli.UrlShortener.Original.Data;

public class EfShortUrlService : IShortUrlService
{
    private readonly ShortUrlDbContext _context;
    public EfShortUrlService(ShortUrlDbContext shortUrlContext)
        => _context = shortUrlContext;

    public async Task<ICollection<ShortUrl>> Get()
    {
        return await _context.ShortUrls.ToArrayAsync() ?? Array.Empty<ShortUrl>();
    }

    public async Task<ShortUrl> Get(string shortPath)
    {
        return (await _context.ShortUrls.Where(url => url.ShortPath == shortPath).SingleOrDefaultAsync()) ?? ShortUrl.Empty();
    }

    public async Task<ICollection<string>> GetInventory()
    {
        return await _context.ShortUrls.Select(url => url.ShortPath).ToListAsync();
    }

    public async Task Save(params ShortUrl[] shortUrls) 
    {
        _context.ShortUrls.AddRange(shortUrls);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(string shortUrl)
    {
        var existing = await _context.ShortUrls.Where(url => url.ShortPath == shortUrl).ToListAsync();
        if (existing.Count == 0) return;
        _context.ShortUrls.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }
}