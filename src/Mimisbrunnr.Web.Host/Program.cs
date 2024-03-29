using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices;
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

app.UseSpaStaticFiles();

app.UseRouting();

app.UseMetrics();

app.UseAuthentication();

app.UseAuthorization();

app.UseUserValidationMiddleware();

app.MapControllers();

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

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
});

app.Run();