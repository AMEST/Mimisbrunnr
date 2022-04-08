using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
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

        if (space == null)
            throw new SpaceNotFoundException();

        if (space.Type == SpaceType.Public)
            return;

        if (userInfo == null)
            throw new UserHasNotPermissionException();

        var userGroups = await GetUserGroups(userInfo);
        var userPermission = FindPermission(space.Permissions.Where(x => (x.CanView || x.IsAdmin)).ToArray(), userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }

    public async Task EnsureEditPermission(string spaceKey, UserInfo userInfo)
    {
        var space = await _spaceManager.GetByKey(spaceKey);

        if (space == null)
            throw new SpaceNotFoundException();

        if (userInfo == null)
            throw new UserHasNotPermissionException();

        var userGroups = await GetUserGroups(userInfo);
        var userPermission = FindPermission(space.Permissions.Where(x => (x.CanEdit || x.IsAdmin)).ToArray(), userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }

    public async Task EnsureRemovePermission(string spaceKey, UserInfo userInfo)
    {
        var space = await _spaceManager.GetByKey(spaceKey);

        if (space == null)
            throw new SpaceNotFoundException();

        if (userInfo == null)
            throw new UserHasNotPermissionException();

        var userGroups = await GetUserGroups(userInfo);
        var userPermission = FindPermission(space.Permissions.Where(x => (x.CanRemove || x.IsAdmin)).ToArray(), userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }

    public async Task EnsureAdminPermission(string spaceKey, UserInfo userInfo)
    {
        var space = await _spaceManager.GetByKey(spaceKey);

        if (space == null)
            throw new SpaceNotFoundException();

        if (userInfo == null)
            throw new UserHasNotPermissionException();

        var userGroups = await GetUserGroups(userInfo);
        var userPermission = FindPermission(space.Permissions.Where(x => x.IsAdmin).ToArray(), userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }

    private async Task<Group[]> GetUserGroups(UserInfo userInfo)
    {
        var user = await _userManager.FindByEmail(userInfo?.Email);
        var userGroups = await _userGroupManager.GetUserGroups(user);
        return userGroups;
    }

    private static Permission FindPermission(Permission[] permissions, UserInfo user, Group[] groups)
    {
        var userPermission =
            permissions.FirstOrDefault(x => x.User != null && x.User.Equals(user));
        var groupPermission = permissions.FirstOrDefault(x =>
            x.Group != null && groups.Any(g => g.Name.Equals(x.Group.Name)));
        return userPermission ?? groupPermission;
    }
}