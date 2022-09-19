using System.Net;
using Kongsli.UrlShortener.Models;

namespace Kongsli.UrlShortener.Main;

public class UrlRedirectorMiddleware
{
    private readonly RequestDelegate _next;

    public UrlRedirectorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IShortUrlService shortUrlService,
        RequestEventPublisher requestEventPublisher)
    {
        if (await shortUrlService.Get(httpContext.Request.Path.ToUriComponent()[1..]) is { IsEmpty: false } shortUrl)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Found;
            httpContext.Response.Headers.Location = shortUrl.Location.AbsoluteUri;
        }
        else
        {
            await _next(httpContext);
        }
        await requestEventPublisher.Publish(httpContext.Request,
            httpContext.Response.StatusCode == (int)HttpStatusCode.Found);
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