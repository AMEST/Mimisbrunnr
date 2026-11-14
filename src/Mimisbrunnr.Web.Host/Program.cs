using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Mimisbrunnr.Persistent;
using Mimisbrunnr.Storage.MongoDb;
using Mimisbrunnr.Web.Host;
using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Host.Middlewares;
using Mimisbrunnr.Web.Host.Services.Metrics;
using Skidbladnir.Modules;
using VueCliMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSkidbladnirModules<StartupModule>(configuration =>
{
    var mongoConfig = builder.Configuration.GetSection("Storage").Get<MongoDbStoreModuleConfiguration>();
    configuration.Add(mongoConfig);
    var cachingConfiguration = builder.Configuration.GetSection("Caching").Get<CachingConfiguration>();
    configuration.Add(cachingConfiguration);
    var persistentConfig = builder.Configuration.GetSection("Persistent").Get<PersistentModuleConfiguration>();
    configuration.Add(persistentConfig);
    var metricsConfiguration = builder.Configuration.GetSection("Metrics").Get<MetricsConfiguration>();
    configuration.Add(metricsConfiguration);
    var bearerConfiguration = builder.Configuration.GetSection("Bearer").Get<BearerTokenConfiguration>();
    configuration.Add(bearerConfiguration);
}, builder.Configuration);

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseSwaggerFeature();

app.UseMcpFeature();

// The service worker script and the HTML shell must never be cached aggressively:
// otherwise the browser keeps running the previous worker and serves a stale
// index.html that references assets deleted by the next deployment.
static void DisableCaching(StaticFileResponseContext context)
{
    var fileName = context.File.Name;
    if (fileName.Equals("service-worker.js", StringComparison.OrdinalIgnoreCase) ||
        fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase) ||
        fileName.Equals("asset-recovery.js", StringComparison.OrdinalIgnoreCase))
    {
        var headers = context.Context.Response.Headers;
        headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
        headers[HeaderNames.Pragma] = "no-cache";
        headers[HeaderNames.Expires] = "0";
    }
}

app.UseSpaStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = DisableCaching
});

app.UseRouting();

app.UseMetrics();

app.UseAuthentication();

app.UseAuthorization();

app.UseUserValidationMiddleware();

app.MapControllers();

app.MapMcp("/mcp").RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapToVueCliProxy(
        "{*path}",
        new SpaOptions { SourcePath = "ClientApp"},
        npmScript: "serve",
        regex: "Compiled successfully",
        forceKill: true
    );
}

if (!app.Environment.IsDevelopment())
    app.UseMiddleware<SpaAssetFallbackMiddleware>();

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    // The SPA fallback serves index.html for deep links - keep it uncached too.
    spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
    {
        OnPrepareResponse = DisableCaching
    };
});

app.Run();