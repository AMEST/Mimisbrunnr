using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Favorites;

public interface IFavoriteService
{
    Task<FavoriteModel> Add(FavoriteCreateModel model, UserInfo user);

    Task<FavoriteModel[]> GetFavorites(FavoriteFilterModel filter, UserInfo user);

    Task<FavoriteModel> GetFavorite(FavoriteFindModel filter, UserInfo user);

    Task<bool> EnsureInFavorites(FavoriteFindModel model, UserInfo user);

    Task Remove(string id, UserInfo user);
}

internal class FavoriteService : IFavoriteService
{
    private readonly IFavoritesManager _favorites;
    private readonly IUserManager _userManager;
    private readonly IPageService _pageService;
    private readonly ISpaceService _spaceService;

    public FavoriteService(IFavoritesManager favorites, IUserManager userManager,
        IPageService pageService, ISpaceService spaceService)
    {
        _favorites = favorites;
        _userManager = userManager;
        _pageService = pageService;
        _spaceService = spaceService;
    }

    public async Task<FavoriteModel> Add(FavoriteCreateModel model, UserInfo user)
    {
        await EnsureFavoriteTargetExists(model, user);
        var favorite = model.ToEntity();
        favorite.OwnerEmail = user.Email;
        favorite.Created = DateTime.UtcNow;
        await _favorites.Add(favorite);
        return await MapToFavoriteModel(user, favorite);
    }

    public Task<bool> EnsureInFavorites(FavoriteFindModel model, UserInfo user)
    {
        return model switch
        {
            FavoriteUserFindModel favoriteUserModel => _favorites.EnsureItemInFavorite<FavoriteUser>(user.Email, x => x.UserEmail.ToLower() == favoriteUserModel.UserEmail.ToLower()),
            FavoriteSpaceFindModel favoriteSpaceModel => _favorites.EnsureItemInFavorite<FavoriteSpace>(user.Email, x => x.SpaceKey.ToLower() == favoriteSpaceModel.SpaceKey.ToLower()),
            FavoritePageFindModel favoritePageModel => _favorites.EnsureItemInFavorite<FavoritePage>(user.Email, x => x.PageId.ToLower() == favoritePageModel.PageId.ToLower()),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model.GetType().Name, "Unknown favorite type"),
        };
    }

    public async Task<FavoriteModel> GetFavorite(FavoriteFindModel filter, UserInfo user)
    {
        var favorite = filter switch
        {
            FavoriteUserFindModel favoriteUserModel => await _favorites.GetByExpression<FavoriteUser>(user.Email, x => x.UserEmail.ToLower() == favoriteUserModel.UserEmail.ToLower()),
            FavoriteSpaceFindModel favoriteSpaceModel => await _favorites.GetByExpression<FavoriteSpace>(user.Email, x => x.SpaceKey.ToLower() == favoriteSpaceModel.SpaceKey.ToLower()),
            FavoritePageFindModel favoritePageModel => await _favorites.GetByExpression<FavoritePage>(user.Email, x => x.PageId.ToLower() == favoritePageModel.PageId.ToLower()),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter.GetType().Name, "Unknown favorite type"),
        };
        if(favorite is null)
            return null;
        return await MapToFavoriteModel(user, favorite);
    }

    public async Task<FavoriteModel[]> GetFavorites(FavoriteFilterModel filter, UserInfo user)
    {
        var favorites = await _favorites.FindAllByUserEmail(user.Email, filter?.ToEntity());
        var result = new List<FavoriteModel>();
        foreach (var favorite in favorites)
        {
            try
            {
                var favoriteModel = await MapToFavoriteModel(user, favorite);
                result.Add(favoriteModel);
            }
            catch (Exception e) when (e is UserHasNotPermissionException
             || e is PageNotFoundException
              || e is SpaceNotFoundException
               || e is UserNotFoundException)
            {
                continue;
            }
        }
        return result.ToArray();
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

    private async Task EnsureFavoriteTargetExists(FavoriteCreateModel model, UserInfo requestedBy)
    {
        switch (model)
        {
            case FavoriteUserCreateModel favoriteUserCreateModel:
                if ((await _userManager.GetByEmail(favoriteUserCreateModel.UserEmail)) is null)
                    throw new UserNotFoundException();
                break;
            case FavoriteSpaceCreateModel favoriteSpaceCreateModel:
                if ((await _spaceService.GetByKey(favoriteSpaceCreateModel.SpaceKey, requestedBy)) is null)
                    throw new SpaceNotFoundException();
                break;
            case FavoritePageCreateModel favoritePageCreateModel:
                if ((await _pageService.GetById(favoritePageCreateModel.PageId, requestedBy)) is null)
                    throw new PageNotFoundException();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(model), model.GetType().Name, "Unknown favorite type");
        }
    }

    private async Task<FavoriteModel> MapToFavoriteModel(UserInfo user, Favorite favorite)
    {
        var favoriteModel = favorite.ToModel();
        switch (favorite)
        {
            case FavoriteUser favoriteUser:
                (favoriteModel as FavoriteUserModel).User = (await _userManager.GetByEmail(favoriteUser.UserEmail)).ToModel();
                break;
            case FavoritePage favoritePage:
                (favoriteModel as FavoritePageModel).Page = await _pageService.GetById(favoritePage.PageId, user);
                break;
            case FavoriteSpace favoriteSpace:
                (favoriteModel as FavoriteSpaceModel).Space = await _spaceService.GetByKey(favoriteSpace.SpaceKey, user);
                break;
        }
        return favoriteModel;
    }
}