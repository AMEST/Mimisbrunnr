using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Storage.MongoDb;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Services;
using Skidbladnir.DataProtection.MongoDb;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host;

public class StartupModule : Module
{
    public override Type[] DependsModules => new[]
    {
        typeof(AspNetModule), typeof(MongoDbStoreModule),  typeof(WebModule)
    };

    public override void Configure(IServiceCollection services)
    {
        var mongoStoreConfiguration = Configuration.Get<MongoDbStoreModuleConfiguration>();
        services.AddDataProtectionMongoDb(mongoStoreConfiguration.ConnectionString);
        services.AddSingleton<IAuthorizationHandler, EnsureUserAuthorizationHandler>();
        services.AddSingleton<IPermissionService, PermissionService>();
    }
}