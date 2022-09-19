using Kongsli.UrlShortener.Models;
using Microsoft.EntityFrameworkCore;

namespace Kongsli.UrlShortener.Original.Data;

public class ShortUrlDbContext : DbContext
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
    
    public ShortUrlDbContext(DbContextOptions<ShortUrlDbContext> options)
        : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ShortUrl>().HasKey(url => url.ShortPath);
}
