using Kongsli.UrlShortener.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IShortUrlService, HttpClientShortUrlService>();
var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");

builder.Services.AddHttpClient(HttpClientShortUrlService.HTTP_CLIENT_NAME, client => {
    client.BaseAddress = new Uri($"http://localhost:{daprHttpPort}/v1.0/state/");
});

var app = builder.Build();
app.Logger.LogInformation("Using Dapr sidecar on port {port}", daprHttpPort);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }