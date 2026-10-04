using Riok.Mapperly.Abstractions;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Favorites.Contracts;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps favorite entities to and from their web models
/// </summary>
[Mapper]
public static partial class FavoriteMapper
{
    /// <summary>
    /// Maps a favorite user to a favorite user model
    /// </summary>
    /// <param name="favorite">Favorite user to map</param>
    /// <returns>The mapped favorite user model</returns>
    [MapperIgnoreSource(nameof(FavoriteUser.UserEmail))] 
    [MapperIgnoreSource(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUserModel.User))] // Ignore because, User fill in Service after map
    public static partial FavoriteUserModel ToModel(this FavoriteUser favorite);
    
    /// <summary>
    /// Maps a favorite space to a favorite space model
    /// </summary>
    /// <param name="favorite">Favorite space to map</param>
    /// <returns>The mapped favorite space model</returns>
    [MapperIgnoreSource(nameof(FavoriteSpace.SpaceKey))]
    [MapperIgnoreSource(nameof(FavoriteSpace.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteSpaceModel.Space))] // Ignore because, Space fill in Service after map
    public static partial FavoriteSpaceModel ToModel(this FavoriteSpace favorite);

    /// <summary>
    /// Maps a favorite page to a favorite page model
    /// </summary>
    /// <param name="favorite">Favorite page to map</param>
    /// <returns>The mapped favorite page model</returns>
    [MapperIgnoreSource(nameof(FavoritePage.PageId))]
    [MapperIgnoreSource(nameof(FavoritePage.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoritePageModel.Page))]  // Ignore because, Page fill in Service after map
    public static partial FavoritePageModel ToModel(this FavoritePage favorite);

    /// <summary>
    /// Maps a favorite to the corresponding favorite model based on its concrete type
    /// </summary>
    /// <param name="favorite">Favorite to map</param>
    /// <returns>The mapped favorite model</returns>
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

    /// <summary>
    /// Maps a favorite user create model to a favorite user entity
    /// </summary>
    /// <param name="createModel">Favorite user create model to map</param>
    /// <returns>The mapped favorite user entity</returns>
    [MapperIgnoreTarget(nameof(FavoriteUser.Id))]
    [MapperIgnoreTarget(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUser.Created))]
    public static partial FavoriteUser ToEntity(this FavoriteUserCreateModel createModel);

    /// <summary>
    /// Maps a favorite space create model to a favorite space entity
    /// </summary>
    /// <param name="createModel">Favorite space create model to map</param>
    /// <returns>The mapped favorite space entity</returns>
    [MapperIgnoreTarget(nameof(FavoriteUser.Id))]
    [MapperIgnoreTarget(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUser.Created))]
    public static partial FavoriteSpace ToEntity(this FavoriteSpaceCreateModel createModel);

    /// <summary>
    /// Maps a favorite page create model to a favorite page entity
    /// </summary>
    /// <param name="createModel">Favorite page create model to map</param>
    /// <returns>The mapped favorite page entity</returns>
    [MapperIgnoreTarget(nameof(FavoriteUser.Id))]
    [MapperIgnoreTarget(nameof(FavoriteUser.OwnerEmail))]
    [MapperIgnoreTarget(nameof(FavoriteUser.Created))]
    public static partial FavoritePage ToEntity(this FavoritePageCreateModel createModel);

    /// <summary>
    /// Maps a favorite filter model to a favorite filter
    /// </summary>
    /// <param name="filter">Favorite filter model to map</param>
    /// <returns>The mapped favorite filter</returns>
    public static partial FavoriteFilter ToEntity(this FavoriteFilterModel filter);

    /// <summary>
    /// Maps a favorite create model to the corresponding favorite entity based on its concrete type
    /// </summary>
    /// <param name="createModel">Favorite create model to map</param>
    /// <returns>The mapped favorite entity</returns>
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