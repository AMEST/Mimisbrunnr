using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Favorites.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Favorites;

public class FavoritesModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IFavoritesManager, FavoritesManager>();
    }
}