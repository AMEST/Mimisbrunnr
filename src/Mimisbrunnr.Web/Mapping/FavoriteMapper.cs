using Riok.Mapperly.Abstractions;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Favorites.Contracts;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class FavoriteMapper
{
    [MapperIgnoreSource(nameof(FavoriteUser.UserEmail))] 
    [MapperIgnoreSource(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUserModel.User))] // Ignore because, User fill in Service after map
    public static partial FavoriteUserModel ToModel(this FavoriteUser favorite);
    
    [MapperIgnoreSource(nameof(FavoriteSpace.SpaceKey))]
    [MapperIgnoreSource(nameof(FavoriteSpace.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteSpaceModel.Space))] // Ignore because, Space fill in Service after map
    public static partial FavoriteSpaceModel ToModel(this FavoriteSpace favorite);

    [MapperIgnoreSource(nameof(FavoritePage.PageId))]
    [MapperIgnoreSource(nameof(FavoritePage.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoritePageModel.Page))]  // Ignore because, Page fill in Service after map
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

    [MapperIgnoreTarget(nameof(FavoriteUser.Id))]
    [MapperIgnoreTarget(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUser.Created))]
    public static partial FavoriteUser ToEntity(this FavoriteUserCreateModel createModel);

    [MapperIgnoreTarget(nameof(FavoriteUser.Id))]
    [MapperIgnoreTarget(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUser.Created))]
    public static partial FavoriteSpace ToEntity(this FavoriteSpaceCreateModel createModel);

    [MapperIgnoreTarget(nameof(FavoriteUser.Id))]
    [MapperIgnoreTarget(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUser.Created))]
    public static partial FavoritePage ToEntity(this FavoritePageCreateModel createModel);
    public static partial FavoriteFilter ToEntity(this FavoriteFilterModel filter);

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