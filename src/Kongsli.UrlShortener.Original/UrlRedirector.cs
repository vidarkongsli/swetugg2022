using System.Net;
using Kongsli.UrlShortener.Models;

namespace Kongsli.UrlShortener.Original;

public class UrlRedirectorMiddleware
{
    private readonly RequestDelegate _next;

    public UrlRedirectorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IShortUrlService shortUrlService)
    {
        if (await shortUrlService.Get(httpContext.Request.Path.ToUriComponent()[1..]) is {} shortUrl)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Found;
            httpContext.Response.Headers.Location = shortUrl.Location.AbsoluteUri;
        }
        else
        {
            await _next(httpContext);
        }
    }
}

public static class UrlRedirectorMiddlewareExtensions
{
    public static IApplicationBuilder UseUrlRedirector(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UrlRedirectorMiddleware>();
    }
}