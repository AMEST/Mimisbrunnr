using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Host.Services;

internal class PermissionService : IPermissionService
{
    private readonly IApplicationConfigurationService _applicationConfigurationService;
    private readonly ISpaceManager _spaceManager;
    private readonly IUserManager _userManager;
    private readonly IUserGroupManager _userGroupManager;

    public PermissionService(
        IApplicationConfigurationService applicationConfigurationService,
        ISpaceManager spaceManager,
        IUserManager userManager,
        IUserGroupManager userGroupManager
    )
    {
        _applicationConfigurationService = applicationConfigurationService;
        _spaceManager = spaceManager;
        _userManager = userManager;
        _userGroupManager = userGroupManager;
    }

    public async Task EnsureAnonymousAllowed(UserInfo userInfo)
    {
        if (userInfo != null)
            return;
        var configuration = await _applicationConfigurationService.Get();
        if (!configuration.AllowAnonymous)
            throw new AnonymousNotAllowedException();
    }

    public async Task EnsureViewPermission(string spaceKey, UserInfo userInfo)
    {
        var space = await _spaceManager.GetByKey(spaceKey);
        if (space.Type == SpaceType.Public)
            return;

        if (userInfo == null && space.Type != SpaceType.Public)
            throw new UserHasNotPermissionException();


        var user = await _userManager.FindByEmail(userInfo?.Email);
        var userGroups = await _userGroupManager.GetUserGroups(user);
        var userPermission = FindViewPermission(space, userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }

    public async Task EnsureEditPermission(string spaceKey, UserInfo userInfo)
    {
        var space = await _spaceManager.GetByKey(spaceKey);

        if (userInfo == null)
            throw new UserHasNotPermissionException();

        var user = await _userManager.FindByEmail(userInfo?.Email);
        var userGroups = await _userGroupManager.GetUserGroups(user);
        var userPermission = FindEditPermission(space, userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }
    
    public async Task EnsureRemovePermission(string spaceKey, UserInfo userInfo)
    {
        var space = await _spaceManager.GetByKey(spaceKey);

        if (userInfo == null)
            throw new UserHasNotPermissionException();

        var user = await _userManager.FindByEmail(userInfo?.Email);
        var userGroups = await _userGroupManager.GetUserGroups(user);
        var userPermission = FindRemovePermission(space, userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }
    
    private static Permission FindViewPermission(Space space, UserInfo user, Group[] groups)
    {
        var userPermission =
            space.Permissions.FirstOrDefault(x => (x.CanView || x.IsAdmin) && x.User != null && x.User.Equals(user));
        var groupPermission = space.Permissions.FirstOrDefault(x =>
            (x.CanView || x.IsAdmin) && x.Group != null && groups.Any(g => g.Name.Equals(x.Group.Name)));
        return userPermission ?? groupPermission;
    }

    private static Permission FindEditPermission(Space space, UserInfo user, Group[] groups)
    {
        var userPermission =
            space.Permissions.FirstOrDefault(x => (x.CanEdit || x.IsAdmin) && x.User != null && x.User.Equals(user));
        var groupPermission = space.Permissions.FirstOrDefault(x =>
            (x.CanEdit || x.IsAdmin) && x.Group != null && groups.Any(g => g.Name.Equals(x.Group.Name)));
        return userPermission ?? groupPermission;
    }
    
    private static Permission FindRemovePermission(Space space, UserInfo user, Group[] groups)
    {
        var userPermission =
            space.Permissions.FirstOrDefault(x => (x.CanRemove || x.IsAdmin) && x.User != null && x.User.Equals(user));
        var groupPermission = space.Permissions.FirstOrDefault(x =>
            (x.CanRemove || x.IsAdmin) && x.Group != null && groups.Any(g => g.Name.Equals(x.Group.Name)));
        return userPermission ?? groupPermission;
    }
}