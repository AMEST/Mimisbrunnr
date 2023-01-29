using Mimisbrunnr.Web.Favorites;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

public class WebCacheModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.Decorate<IFavoriteService, FavoriteServiceCacheDecorator>();
    }
}