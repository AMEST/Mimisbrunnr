using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Feed;

internal class FeedService : IFeedService
{
    private readonly IFeedManager _feedManager;
    private readonly ISpaceDisplayService _spaceDisplayService;
    private readonly IPermissionService _permissionService;
    private readonly IUserManager _userManager;

    public FeedService(
        IUserManager userManager, 
        IPermissionService permissionService, 
        IFeedManager feedManager,
        ISpaceDisplayService spaceDisplayService
    )
    {
        _userManager = userManager;
        _permissionService = permissionService;
        _feedManager = feedManager;
        _spaceDisplayService = spaceDisplayService;
    }

    public async Task<PageUpdateEventModel[]> GetPageUpdates(UserInfo requestedBy, string updatedByEmail = null)
    {
        if (requestedBy is null)
        {
            await _permissionService.EnsureAnonymousAllowed(requestedBy);
            var publicUpdates = await _feedManager.GetPageUpdates(requestedBy);
            return publicUpdates.Select(x => x.ToModel()).ToArray();
        }

        if (!string.IsNullOrEmpty(updatedByEmail))
        {
            var visibleSpaces = await _spaceDisplayService.FindUserVisibleSpaces(requestedBy);
            var userUpdates = await _feedManager.GetPageUpdates(requestedBy, visibleSpaces, new UserInfo { Email = updatedByEmail.ToLower() });
            return userUpdates.Select(x => x.ToModel()).ToArray();
        }

        var user = await _userManager.GetByEmail(requestedBy.Email);
        if (user.Role == UserRole.Admin)
        {
            var allUpdates = await _feedManager.GetAllPageUpdates();
            return allUpdates.Select(x => x.ToModel()).ToArray();
        }

        var userSpaces = await _spaceDisplayService.FindUserVisibleSpaces(requestedBy);
        var updates = await _feedManager.GetPageUpdates(requestedBy, userSpaces);
        return updates.Select(x => x.ToModel()).ToArray();
    }
}