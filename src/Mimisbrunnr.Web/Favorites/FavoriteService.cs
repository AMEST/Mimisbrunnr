using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Favorites;

/// <summary>
/// Service for managing user favorites
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Adds a new favorite
    /// </summary>
    /// <param name="model">Favorite to add</param>
    /// <param name="user">Owner of the favorite</param>
    /// <returns>The added favorite</returns>
    Task<FavoriteModel> Add(FavoriteCreateModel model, UserInfo user);

    /// <summary>
    /// Gets favorites matching the specified filter
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="user">Owner of the favorites</param>
    /// <returns>Matching favorites</returns>
    Task<FavoriteModel[]> GetFavorites(FavoriteFilterModel filter, UserInfo user);

    /// <summary>
    /// Gets a single favorite matching the specified criteria
    /// </summary>
    /// <param name="filter">Criteria to find the favorite</param>
    /// <param name="user">Owner of the favorite</param>
    /// <returns>The found favorite, or null if not found</returns>
    Task<FavoriteModel> GetFavorite(FavoriteFindModel filter, UserInfo user);

    /// <summary>
    /// Checks whether the specified item is in the user's favorites
    /// </summary>
    /// <param name="model">Criteria identifying the favorite</param>
    /// <param name="user">Owner of the favorite</param>
    /// <returns>True if the item is in favorites; otherwise false</returns>
    Task<bool> EnsureInFavorites(FavoriteFindModel model, UserInfo user);

    /// <summary>
    /// Removes a favorite by identifier
    /// </summary>
    /// <param name="id">Identifier of the favorite to remove</param>
    /// <param name="user">Owner of the favorite</param>
    Task Remove(string id, UserInfo user);
}

internal class FavoriteService : IFavoriteService
{
    private readonly IFavoritesManager _favorites;
    private readonly IUserManager _userManager;
    private readonly IPageService _pageService;
    private readonly ISpaceService _spaceService;
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly ISpaceDisplayService _spaceDisplayService;

    public FavoriteService(IFavoritesManager favorites,
        IUserManager userManager,
        IPageService pageService,
        ISpaceService spaceService,
        IPageManager pageManager,
        ISpaceManager spaceManager,
        ISpaceDisplayService spaceDisplayService)
    {
        _favorites = favorites;
        _userManager = userManager;
        _pageService = pageService;
        _spaceService = spaceService;
        _pageManager = pageManager;
        _spaceManager = spaceManager;
        _spaceDisplayService = spaceDisplayService;
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

    /// <summary>
    /// Maps the whole favorites list with one request per entity type instead of one request per favorite.
    /// </summary>
    public async Task<FavoriteModel[]> GetFavorites(FavoriteFilterModel filter, UserInfo user)
    {
        var favorites = (await _favorites.FindAllByUserEmail(user.Email, filter?.ToEntity())).ToArray();
        if (favorites.Length == 0)
            return [];

        var visibleSpaces = (await _spaceDisplayService.FindUserVisibleSpaces(user)).ToArray();
        var visibleSpacesById = visibleSpaces.ToDictionary(x => x.Id, x => x, StringComparer.Ordinal);
        var visibleSpaceKeys = visibleSpaces.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var usersByEmail = await LoadUsers(favorites);
        var pagesById = await LoadPages(favorites);
        var spacesByKey = await LoadSpaces(favorites);

        var result = new List<FavoriteModel>(favorites.Length);
        foreach (var favorite in favorites)
        {
            var favoriteModel = favorite.ToModel();
            switch (favorite)
            {
                case FavoriteUser favoriteUser:
                    if (!usersByEmail.TryGetValue(favoriteUser.UserEmail, out var favoriteTargetUser))
                        continue;
                    ((FavoriteUserModel)favoriteModel).User = favoriteTargetUser.ToModel();
                    break;
                case FavoritePage favoritePage:
                    if (!pagesById.TryGetValue(favoritePage.PageId, out var favoriteTargetPage)
                        || !visibleSpacesById.TryGetValue(favoriteTargetPage.SpaceId, out var pageSpace))
                        continue;
                    ((FavoritePageModel)favoriteModel).Page = favoriteTargetPage.ToModel(pageSpace.Key);
                    break;
                case FavoriteSpace favoriteSpace:
                    if (!spacesByKey.TryGetValue(favoriteSpace.SpaceKey, out var favoriteTargetSpace)
                        || !visibleSpaceKeys.Contains(favoriteTargetSpace.Key))
                        continue;
                    ((FavoriteSpaceModel)favoriteModel).Space = favoriteTargetSpace.ToModel();
                    break;
            }
            result.Add(favoriteModel);
        }

        return [.. result];
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

    private async Task<Dictionary<string, Users.User>> LoadUsers(Favorite[] favorites)
    {
        var emails = favorites.OfType<FavoriteUser>()
            .Select(x => x.UserEmail)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (emails.Length == 0)
            return new Dictionary<string, Users.User>(StringComparer.OrdinalIgnoreCase);

        var users = await _userManager.GetByEmails(emails);
        return users
            .Where(x => !string.IsNullOrEmpty(x.Email))
            .ToDictionary(x => x.Email, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, Page>> LoadPages(Favorite[] favorites)
    {
        var pageIds = favorites.OfType<FavoritePage>()
            .Select(x => x.PageId)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (pageIds.Length == 0)
            return new Dictionary<string, Page>(StringComparer.Ordinal);

        var pages = await _pageManager.GetByIds(pageIds);
        return pages
            .Where(x => !string.IsNullOrEmpty(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
    }

    private async Task<Dictionary<string, Space>> LoadSpaces(Favorite[] favorites)
    {
        var spaceKeys = favorites.OfType<FavoriteSpace>()
            .Select(x => x.SpaceKey)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (spaceKeys.Length == 0)
            return new Dictionary<string, Space>(StringComparer.OrdinalIgnoreCase);

        var spaces = await _spaceManager.GetByKeys(spaceKeys);
        return spaces
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
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
