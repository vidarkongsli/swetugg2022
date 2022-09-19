
using Kongsli.UrlShortener.Original.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Kongsli.UrlShortener.Original.Tests.Helpers;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ShortUrlDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<ShortUrlDbContext>(options => 
                options.UseInMemoryDatabase("InMemoryForTesting"));
        });
    }
}
