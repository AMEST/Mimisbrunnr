namespace Mimisbrunnr.Web.Host.Middlewares;

// Runs after static files and routing, before the SPA's index.html fallback.
internal class SpaAssetFallbackMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint() is not null)
            return next(context);

        var path = context.Request.Path;
        var value = path.Value ?? string.Empty;
        if (path.StartsWithSegments("/js") || path.StartsWithSegments("/css") ||
            path.StartsWithSegments("/img") || path.StartsWithSegments("/assets") ||
            path.Equals("/service-worker.js") || path.Equals("/asset-recovery.js") ||
            path.Equals("/manifest.json") || path.Equals("/favicon.ico") ||
            path.Equals("/robots.txt") ||
            (value.StartsWith("/workbox-", StringComparison.OrdinalIgnoreCase) &&
             value.EndsWith(".js", StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.Headers.CacheControl = "no-store";
            return Task.CompletedTask;
        }

        return next(context);
    }
}
