using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Host.Middlewares;
using Prometheus;

namespace Mimisbrunnr.Web.Host.Services.Metrics
{
    internal static class MetricsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        {
            var configuration = builder.ApplicationServices.GetService<MetricsConfiguration>();
            if (!configuration.Enabled)
                return builder;

            return builder.Map(configuration.Endpoint, metricsApp =>
            {
                if (configuration.BasicAuth)
                    metricsApp.UseMiddleware<BasicAuthMiddleware>(configuration);
                metricsApp.UseMetricServer(string.Empty);
            })
            .Use(async (context, next) =>
            {
                if (!context.IsStaticFiles())
                {
                    var routeData = context.GetRouteData();
                    if (string.IsNullOrEmpty(routeData.Values["controller"] as string))
                        context.GetRouteData().Values.Add("controller", context.GetCustomControllerName(configuration));
                    if (string.IsNullOrEmpty(routeData.Values["action"] as string))
                        context.GetRouteData().Values.Add("action", context.Request.Path.Value);
                }
                await next();
            })
            .UseHttpMetrics(); ;
        }

        private static string GetCustomControllerName(this HttpContext context, MetricsConfiguration configuration)
        {
            var path = context.Request.Path.Value;
            if (path.StartsWith("/ws/"))
                return "signalR";

            if (path.StartsWith(configuration.Endpoint))
                return "prometheus";

            return "spa";
        }

        private static bool IsStaticFiles(this HttpContext context)
        {
            var path = context.Request.Path.Value;
            var method = context.Request.Method;
            var pathExtension = Path.GetExtension(path);
            return method.Equals("get", StringComparison.OrdinalIgnoreCase)
                    && (!string.IsNullOrEmpty(pathExtension)
                    || path.StartsWith("/sockjs-node"));
        }
    }
}