using System.Collections;
using Kongsli.UrlShortener.Models;
using Kongsli.UrlShortener.Original;
using Kongsli.UrlShortener.Original.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IShortUrlService, EfShortUrlService>();
builder.Services.AddDbContext<ShortUrlDbContext>(options => {
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    options.UseSqlite($"Data Source={Path.Join(path, "shorturls.db")}");
});
builder.Services.AddHealthChecks();
var app = builder.Build();

using var scope = app.Services.CreateScope();
var shortUrlService = scope.ServiceProvider.GetRequiredService<IShortUrlService>();
await shortUrlService.Save(new ShortUrl(new Uri("https://Www.vg.no"), "vg"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseAuthorization();
app.Use(async (context, next) =>
    {
        await next();
        if (context.Response.StatusCode == 404)
        {
            context.Response.ContentType = "text/plain; charset=UTF-8";
            await context.Response.WriteAsync("Not found 🤷");
        }
    });
app.UseUrlRedirector();

foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
{
    if (item.Key?.ToString()?.StartsWith("DAPR") ?? false)
        Console.WriteLine($"{item.Key}={item.Value}");
}
app.Run();
public partial class Program { }