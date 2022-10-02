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

    public static IApplicationBuilder UseUserValidationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ValidateUserStateMiddleware>()
                .UseMiddleware<ValidateTokenNotRevokedMiddleware>()
                .MapWhen(
                    ctx => ctx.Request.Path.Value.Equals("/error/account-disabled", StringComparison.OrdinalIgnoreCase),
                    b => b.Run(async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(@"
                            <h2>Account has been disabled. Contact with administrators.</h2>
                            <hr>
                            Mimisbrunnr wiki system.
                        ");
                    }));
    }
}