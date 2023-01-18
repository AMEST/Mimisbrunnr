using Riok.Mapperly.Abstractions;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Favorites.Contracts;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class FavoriteMapper
{
    public static partial FavoriteUserModel ToModel(this FavoriteUser favorite);
    public static partial FavoriteSpaceModel ToModel(this FavoriteSpace favorite);
    public static partial FavoritePageModel ToModel(this FavoritePage favorite);

    public static FavoriteModel ToModel(this Favorite favorite)
    {
        return favorite switch
        {
            FavoriteUser favoriteUser => favoriteUser.ToModel(),
            FavoriteSpace favoriteSpace => favoriteSpace.ToModel(),
            FavoritePage favoritePage => favoritePage.ToModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(favorite), favorite.GetType().Name, "Unknown favorite type"),
        };
    }

    public static partial FavoriteUser ToEntity(this FavoriteUserCreateModel createModel);
    public static partial FavoriteSpace ToEntity(this FavoriteSpaceCreateModel createModel);
    public static partial FavoritePage ToEntity(this FavoritePageCreateModel createModel);

    public static Favorite ToEntity(this FavoriteCreateModel createModel)
    {
        return createModel switch
        {
            FavoriteUserCreateModel favoriteUser => favoriteUser.ToEntity(),
            FavoriteSpaceCreateModel favoriteSpace => favoriteSpace.ToEntity(),
            FavoritePageCreateModel favoritePage => favoritePage.ToEntity(),
            _ => throw new ArgumentOutOfRangeException(nameof(createModel), createModel.GetType().Name, "Unknown favoriteCreateModel type"),
        };
    }
}