using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using MongoDB.Driver;

namespace Mimisbrunnr.Web.Host.Services;

internal class PermissionService : IPermissionService
{
    private readonly IApplicationConfigurationManager _applicationConfigurationService;
    private readonly ISpaceManager _spaceManager;
    private readonly IUserManager _userManager;
    private readonly IUserGroupManager _userGroupManager;

    public PermissionService(
        IApplicationConfigurationManager applicationConfigurationService,
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

        var user = await _userManager.GetByEmail(userInfo.Email);
        if (user.Role == UserRole.Admin)
            return;

        var userGroups = await GetUserGroups(user);
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

        var user = await _userManager.GetByEmail(userInfo.Email);
        if (user.Role == UserRole.Admin)
            return;

        var userGroups = await GetUserGroups(user);
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

        var user = await _userManager.GetByEmail(userInfo.Email);
        if (user.Role == UserRole.Admin)
            return;

        var userGroups = await GetUserGroups(user);
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

        var user = await _userManager.GetByEmail(userInfo.Email);
        if (user.Role == UserRole.Admin)
            return;

        var userGroups = await GetUserGroups(user);
        var userPermission = FindPermission(space.Permissions.Where(x => x.IsAdmin).ToArray(), userInfo, userGroups);

        if (userPermission == null)
            throw new UserHasNotPermissionException();
    }

    private async Task<Users.Group[]> GetUserGroups(Users.User user)
    {
        var userGroups = await _userGroupManager.GetUserGroups(user);
        return userGroups;
    }

    private static Permission FindPermission(IEnumerable<Permission> permissions, UserInfo user, Users.Group[] groups)
    {
        var userPermission =
            permissions.FirstOrDefault(x => x.User != null && x.User.Equals(user));
        var groupPermission = permissions.FirstOrDefault(x =>
            x.Group != null && groups.Any(g => g.Name.Equals(x.Group.Name)));
        return userPermission ?? groupPermission;
    }
}
