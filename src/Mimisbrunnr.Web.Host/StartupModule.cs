using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.DataImport;
using Mimisbrunnr.DataImport.Confluence;
using Mimisbrunnr.Persistent;
using Mimisbrunnr.Storage.MongoDb;
using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Host.Services.CacheDecorators;
using Mimisbrunnr.Web.Host.Services.Features;
using Mimisbrunnr.Web.Host.Services.Metrics;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Skidbladnir.Caching.Distributed.MongoDB;
using Skidbladnir.DataProtection.MongoDb;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host;

internal class StartupModule : Module
{
    public override Type[] DependsModules => new[]
    {
        typeof(AspNetModule), typeof(MongoDbStoreModule), typeof(WebModule),
        typeof(WebCacheModule), typeof(PersistentModule),
        typeof(MetricsModule), typeof(ConfluenceDataImportModule)
    };

    public override void Configure(IServiceCollection services)
    {
        var mongoStoreConfiguration = Configuration.Get<MongoDbStoreModuleConfiguration>();
        var bearerConfiguration = Configuration.Get<BearerTokenConfiguration>();
        services.AddSingleton(bearerConfiguration);
        services.AddDataProtectionMongoDb(mongoStoreConfiguration.ConnectionString);
        services.AddSingleton<IAuthorizationHandler, EnsureUserAuthorizationHandler>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IFeatureService, FeatureService>();
        services.AddSingleton<ISecurityTokenService, SecurityTokenService>();
        services.AddSingleton<IWikiService, DataImportWikiService>();
        
        ConfigureDistributedCache(services);
    }

    private void ConfigureDistributedCache(IServiceCollection services)
    {
        var cacheConfiguration = Configuration.Get<CachingConfiguration>();
        var mongoConfiguration = Configuration.Get<MongoDbStoreModuleConfiguration>();
        switch (cacheConfiguration.Type)
        {
            case CachingType.MongoDb:
                services.AddMongoDistributedCache(mongoConfiguration.ConnectionString);
                break;
            case CachingType.Redis:
                if (string.IsNullOrEmpty(cacheConfiguration.RedisConnectionString))
                    throw new ArgumentNullException(nameof(cacheConfiguration.RedisConnectionString));
                services.AddStackExchangeRedisCache(c => c.Configuration = cacheConfiguration.RedisConnectionString)
                        .Decorate<IDistributedCache, RedisCacheFallbackDecorator>();
                break;
            default:
                services.AddDistributedMemoryCache();
                break;
        }
    }
}