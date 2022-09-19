using Kongsli.UrlShortener.Notifications;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<EventNotificationService>();
var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
builder.Services.AddHttpClient(EventNotificationService.HTTP_CLIENT_NAME, client => {
    client.BaseAddress = new Uri($"http://localhost:{daprHttpPort}/v1.0/bindings/");
});

var app = builder.Build();
app.Logger.LogInformation("Using Dapr sidecar on port {port}", daprHttpPort);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging();
}

app.UseHealthChecks("/health");
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }