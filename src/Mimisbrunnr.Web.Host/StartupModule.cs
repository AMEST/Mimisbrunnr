using Mimisbrunner.Users;
using Mimisbrunnr.Storage.MongoDb;
using Mimisbrunnr.Web.Host.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host;

public class StartupModule : Module
{
    public override Type[] DependsModules => new[]
    {
        typeof(AspNetModule), typeof(MongoDbStoreModule), typeof(UsersModule)
    };
}