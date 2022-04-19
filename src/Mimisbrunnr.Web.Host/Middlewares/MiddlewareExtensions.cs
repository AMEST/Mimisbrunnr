using Mimisbrunnr.Web.Host.Services.Features;

namespace Mimisbrunnr.Web.Host.Middlewares;

internal static class MiddlewareExtensions
{
    public static IApplicationBuilder UseFeatureMiddleware(
        this IApplicationBuilder builder,
        string featureName
    )
    {
        return builder.UseMiddleware<FeatureMiddleware>(featureName);
    }

    public static IApplicationBuilder UseSwaggerFeature(this IApplicationBuilder builder)
    {
        return builder.MapWhen(x => x.Request.Path.Value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase),
            map =>
            {
                map.UseFeatureMiddleware($"{FeatureService.ApplicationFeaturePrefix}_swagger");
                map.UseSwagger();
                map.UseSwaggerUI();
            }
        );
    }
}