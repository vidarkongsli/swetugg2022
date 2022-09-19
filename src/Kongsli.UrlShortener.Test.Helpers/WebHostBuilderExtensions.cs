using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Kongsli.UrlShortener.Test.Helpers;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder ConfigureLoggingToXUnit(this IWebHostBuilder builder, ITestOutputHelper output)
    {
        builder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider>(serviceProvider => new XUnitLoggerProvider(output));
        });
        return builder;
    }    
}