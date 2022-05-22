using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Search;

internal class SearchService : ISearchService
{
    private readonly IPermissionService _permissionService;
    private readonly IPageSearcher _pageSearcher;
    private readonly ISpaceSearcher _spaceSearcher;
    private readonly IUserManager _userManager;

    public SearchService(IPermissionService permissionService, IPageSearcher pageSearcher, ISpaceSearcher spaceSearcher, IUserManager userManager)
    {
        _permissionService = permissionService;
        _pageSearcher = pageSearcher;
        _spaceSearcher = spaceSearcher;
        _userManager = userManager;
    }

    public async Task<IEnumerable<PageModel>> SearchPages(string text, UserInfo searchBy)
    {
        var user = await _userManager.GetByEmail(searchBy.Email);
        var pages = await _pageSearcher.Search(text);
        if (user.Role == UserRole.Admin)
            return pages.Select(x => x.ToModel());

        var userSpaces = await _permissionService.FindUserVisibleSpaces(searchBy);
        var userSpacesId = userSpaces.Select(x => x.Id);
        return pages.Where(x => userSpacesId.Contains(x.SpaceId)).Select(x => x.ToModel(userSpaces.First(s => s.Id == x.SpaceId).Key));
    }

    public async Task<IEnumerable<SpaceModel>> SearchSpaces(string text, UserInfo searchBy)
    {
        var user = await _userManager.GetByEmail(searchBy.Email);
        var spaces = await _spaceSearcher.Search(text);
        if (user.Role == UserRole.Admin)
            return spaces.Select(x => x.ToModel());

        var userSpaces = (await _permissionService.FindUserVisibleSpaces(searchBy)).Select(x => x.Id);
        return spaces.Where(x => userSpaces.Contains(x.Id)).Select(x => x.ToModel());
    }
}