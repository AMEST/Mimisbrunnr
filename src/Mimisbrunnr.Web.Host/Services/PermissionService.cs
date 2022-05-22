using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using MongoDB.Driver;
using CacheExtensions = Mimisbrunnr.Web.Services.CacheExtensions;

namespace Mimisbrunnr.Web.Host.Services;

internal class PermissionService : IPermissionService
{
    private readonly IApplicationConfigurationService _applicationConfigurationService;
    private readonly ISpaceManager _spaceManager;
    private readonly IUserManager _userManager;
    private readonly IUserGroupManager _userGroupManager;
    private readonly IDistributedCache _distributedCache;

    public PermissionService(
        IApplicationConfigurationService applicationConfigurationService,
        ISpaceManager spaceManager,
        IUserManager userManager,
        IUserGroupManager userGroupManager,
        IDistributedCache distributedCache
    )
    {
        _applicationConfigurationService = applicationConfigurationService;
        _spaceManager = spaceManager;
        _userManager = userManager;
        _userGroupManager = userGroupManager;
        _distributedCache = distributedCache;
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

    public async Task<IEnumerable<Space>> FindUserVisibleSpaces(UserInfo userInfo)
    {
        var cacheKey = CreateUserVisibleSpacesCacheKey(userInfo.Email);
        IEnumerable<Space> spaces = await _distributedCache.GetAsync<Space[]>(cacheKey);
        if (spaces is not null) return spaces;

        var user = await _userManager.GetByEmail(userInfo.Email);
        spaces = await _spaceManager.GetAll();
        if (user.Role == UserRole.Admin)
        {
            await CacheExtensions.SetAsync(_distributedCache,
             cacheKey, 
             spaces, 
             new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
            );
            return spaces;
        }
        var userGroups = await GetUserGroups(user);
        spaces = spaces.Where( x => x.Type == SpaceType.Public || FindPermission(x.Permissions, userInfo, userGroups) != null);
        await CacheExtensions.SetAsync(_distributedCache,
             cacheKey, 
             spaces, 
             new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
        );
        return spaces;
    }


    private async Task<Users.Group[]> GetUserGroups(Mimisbrunnr.Users.User user)
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

    private static string CreateUserVisibleSpacesCacheKey(string email) => $"user_visible_spaces_cache_email_{email.ToLower()}";
}
