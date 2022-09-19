using System.Collections;
using Kongsli.UrlShortener.Main;
using Kongsli.UrlShortener.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IShortUrlService, HttpClientShortUrlService>();
builder.Services.AddTransient<RequestEventPublisher>();
var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");

builder.Services.AddHttpClient(HttpClientShortUrlService.HTTP_CLIENT_NAME, client => {
    client.BaseAddress = new Uri($"http://localhost:{daprHttpPort}/v1.0/state/");
});
builder.Services.AddHttpClient(RequestEventPublisher.HTTP_CLIENT_NAME, client => {
    client.BaseAddress = new Uri($"http://localhost:{daprHttpPort}/v1.0/publish/");
});

var app = builder.Build();
app.Logger.LogInformation("Using Dapr sidecar on port {port}", daprHttpPort);

// using var scope = app.Services.CreateScope();
// var shortUrlService = scope.ServiceProvider.GetRequiredService<IShortUrlService>();
// await shortUrlService.Save(new ShortUrl("https://Www.vg.no".ToUri(), "vg"));

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