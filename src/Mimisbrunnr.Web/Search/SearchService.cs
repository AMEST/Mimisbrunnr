using Mimisbrunnr.Users;
using Mimisbrunnr.Users.Services;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Search;

internal class SearchService : ISearchService
{
    private readonly IPermissionService _permissionService;
    private readonly IPageSearcher _pageSearcher;
    private readonly ISpaceSearcher _spaceSearcher;
    private readonly IUserManager _userManager;
    private readonly IUserSearcher _userSearcher;
    private readonly ISpaceDisplayService _spaceDisplayService;
    private readonly ISpaceManager _spaceManager;

    public SearchService(
        IPermissionService permissionService,
        IPageSearcher pageSearcher,
        ISpaceSearcher spaceSearcher,
        IUserSearcher userSearcher,
        IUserManager userManager,
        ISpaceDisplayService spaceDisplayService,
        ISpaceManager spaceManager)
    {
        _permissionService = permissionService;
        _pageSearcher = pageSearcher;
        _spaceSearcher = spaceSearcher;
        _userManager = userManager;
        _userSearcher = userSearcher;
        _spaceDisplayService = spaceDisplayService;
        _spaceManager = spaceManager;
    }

    public async Task<IEnumerable<PageModel>> SearchPages(string text, UserInfo searchBy)
    {
        var result = await SearchPages(text, searchBy, 100, 0);
        return result.Pages;
    }

    public async Task<PageSearchResult> SearchPages(string text, UserInfo searchBy, int limit, int offset)
    {
        await _permissionService.EnsureAnonymousAllowed(searchBy);

        var user = await _userManager.GetByEmail(searchBy?.Email);
        Dictionary<string, string> spaceKeysById = null;
        if (user?.Role != UserRole.Admin)
        {
            var spaces = await _spaceDisplayService.FindUserVisibleSpaces(searchBy);
            spaceKeysById = spaces.ToDictionary(x => x.Id, x => x.Key, StringComparer.Ordinal);
            if (spaceKeysById.Count == 0)
                return new PageSearchResult([], null);
        }

        // Fetch one extra match to detect the next page without counting every match.
        var foundPages = await _pageSearcher.Search(text, spaceKeysById?.Keys.ToArray(), limit + 1, offset);
        if (foundPages.Length == 0)
            return new PageSearchResult([], null);

        var page = foundPages.Take(limit).ToArray();
        var nextOffset = foundPages.Length > limit ? offset + limit : (int?)null;
        if (spaceKeysById is null)
        {
            var spaces = await _spaceManager.GetByIds(page.Select(x => x.SpaceId).Distinct().ToArray());
            spaceKeysById = spaces
                .Where(x => !string.IsNullOrEmpty(x.Id))
                .ToDictionary(x => x.Id, x => x.Key, StringComparer.Ordinal);
        }

        var results = page
            .Where(x => x.SpaceId is not null && spaceKeysById.ContainsKey(x.SpaceId))
            .Select(x => x.ToModel(spaceKeysById[x.SpaceId]))
            .ToArray();
        return new PageSearchResult(results, nextOffset);
    }

    public async Task<IEnumerable<SpaceModel>> SearchSpaces(string text, UserInfo searchBy)
    {
        await _permissionService.EnsureAnonymousAllowed(searchBy);

        var spaces = await _spaceSearcher.Search(text);
        if (spaces.Length == 0)
            return [];
        var user = await _userManager.GetByEmail(searchBy?.Email);
        if (user?.Role == UserRole.Admin)
            return spaces.Select(x => x.ToModel());

        var userSpacesId = (await _spaceDisplayService.FindUserVisibleSpaces(searchBy))
            .Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);
        return spaces.Where(x => userSpacesId.Contains(x.Id)).Select(x => x.ToModel());
    }

    public async Task<IEnumerable<UserModel>> SearchUsers(string text, UserInfo searchBy)
    {
        await _permissionService.EnsureAnonymousAllowed(searchBy);

        var user = await _userManager.GetByEmail(searchBy?.Email);
        var users = await _userSearcher.Search(text);
        return user?.Role == UserRole.Admin
            ? users.Select(x => x.ToViewModel())
            : users.Select(x => x.ToModel());
    }
}