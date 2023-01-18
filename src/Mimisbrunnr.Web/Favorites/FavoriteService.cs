using System.Threading.Tasks.Dataflow;
using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Favorites;

public interface IFavoriteService
{
    Task<FavoriteModel> Add(FavoriteCreateModel model, UserInfo user);

    Task<FavoriteModel[]> GetFavorites(UserInfo user);

    Task<bool> EnsureInFavorites(FavoriteModel model, UserInfo user);

    Task Remove(string id, UserInfo user);
}

internal class FavoriteService : IFavoriteService
{
    private readonly IFavoritesManager _favorites;

    public FavoriteService(IFavoritesManager favorites)
    {
        _favorites = favorites;
    }

    public async Task<FavoriteModel> Add(FavoriteCreateModel model, UserInfo user)
    {
        var favorite = model.ToEntity();
        favorite.OwnerEmail = user.Email;
        await _favorites.Add(favorite);
        return favorite.ToModel();
    }

    public Task<bool> EnsureInFavorites(FavoriteModel model, UserInfo user)
    {
        return model switch
        {
            FavoriteUserModel favoriteUserModel => _favorites.EnsureItemInFavorite<FavoriteUser>(user.Email, x => x.UserEmail == favoriteUserModel.UserEmail),
            FavoriteSpaceModel favoriteSpaceModel => _favorites.EnsureItemInFavorite<FavoriteSpace>(user.Email, x => x.SpaceKey == favoriteSpaceModel.SpaceKey),
            FavoritePageModel favoritePageModel => _favorites.EnsureItemInFavorite<FavoritePage>(user.Email, x => x.PageId == favoritePageModel.PageId),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model.GetType().Name, "Unknown favorite type"),
        };

    }

    public async Task<FavoriteModel[]> GetFavorites(UserInfo user)
    {
        var favorites = await _favorites.FindAllByUserEmail(user.Email);
        return favorites.Select(x => x.ToModel()).ToArray();
    }

    public async Task Remove(string id, UserInfo user)
    {
        var favorite = await _favorites.FindById(id);
        if (favorite is null)
            return;
        if (!favorite.OwnerEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            throw new UserHasNotPermissionException("Can't delete favorite from another user favorites");

        await _favorites.Remove(favorite);
    }
}