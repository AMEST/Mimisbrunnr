using Mimisbrunnr.Web.Host.Services.Features;

namespace Mimisbrunnr.Web.Host.Middlewares;

internal class FeatureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _featureName;

    public FeatureMiddleware(RequestDelegate next, string featureName)
    {
        _next = next;
        _featureName = featureName;
    }

    public async Task InvokeAsync(HttpContext context, IFeatureService featureService)
    {
        var featureEnabled = await featureService.IsFeatureEnabled(_featureName);
        if (featureEnabled)
        {
            await _next(context);
            return;
        }
        context.Response.StatusCode = 404;
    }
}