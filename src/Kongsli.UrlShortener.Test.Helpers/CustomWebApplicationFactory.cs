namespace Kongsli.UrlShortener.Test.Helpers;

using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private ITestOutputHelper _testOutputHelper = new TestOutputHelper();

    public CustomWebApplicationFactory() => ClientOptions.AllowAutoRedirect = false;   

    public CustomWebApplicationFactory<TProgram> WithOutput(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        return this;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLoggingToXUnit(_testOutputHelper);
        builder.ConfigureServices(services => 
        {
            services.AddSingleton<IHttpClientFactory>(HttpClientFactory);
        });
    }

    public MockHttpClientFactory HttpClientFactory { get; } = new MockHttpClientFactory();
}