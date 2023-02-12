using Mimisbrunnr.Users;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

internal class WebCacheModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.Decorate<IPageManager, PageManagerCacheDecorator>();
        services.Decorate<ISpaceManager, SpaceManagerCacheDecorator>();
        services.Decorate<IUserManager, UserManagerCacheDecorator>();
    }
}