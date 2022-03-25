using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Wiki;

internal class SpaceService : ISpaceService
{
    private readonly IPermissionService _permissionService;
    private readonly ISpaceManager _spaceManager;
    private readonly IUserManager _userManager;
    private readonly IUserGroupManager _userGroupManager;

    public SpaceService(IPermissionService permissionService,
        ISpaceManager spaceManager,
        IUserManager userManager,
        IUserGroupManager userGroupManager
    )
    {
        _permissionService = permissionService;
        _spaceManager = spaceManager;
        _userManager = userManager;
        _userGroupManager = userGroupManager;
    }

    public async Task<SpaceModel[]> GetAll(UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        return (await _spaceManager.GetAll()).Select(x => x.ToModel()).ToArray();
    }

    public async Task<SpaceModel> GetByKey(string key, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        return (await _spaceManager.GetByKey(key)).ToModel();
    }

    public async Task<UserPermissionModel> GetPermission(string key, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        if (requestedBy == null)
            return new UserPermissionModel() { CanView = true };

        var space = await _spaceManager.GetByKey(key);
        var userGroups = await GetUserGroups(requestedBy);
        var userPermission = FindPermission(space.Permissions.ToArray(), requestedBy, userGroups);

        return userPermission?.ToModel();
    }

    public async Task<SpacePermissionModel[]> GetSpacePermissions(string key, UserInfo requestedBy)
    {
        EnsureIsNotAnonymous(requestedBy);
        await _permissionService.EnsureAdminPermission(key, requestedBy);

        var space = await _spaceManager.GetByKey(key);

        return space.Permissions.Select(x => x.ToSpacePermissionModel()).ToArray();
    }

    public async Task<SpaceModel> Create(SpaceCreateModel model, UserInfo createdBy)
    {
        EnsureIsNotAnonymous(createdBy);
        if (model.Type == SpaceTypeModel.Personal)
        {
            var personalSpace = await _spaceManager.FindPersonalSpace(createdBy);
            if (personalSpace != null)
                throw new InvalidOperationException("Only one personal space allowed");
        }

        var space = await _spaceManager.Create(model.Key, model.Name, model.Description, (SpaceType)model.Type,
            createdBy);
        return space.ToModel();
    }

    public async Task Update(string key, SpaceUpdateModel model, UserInfo updatedBy)
    {
        EnsureIsNotAnonymous(updatedBy);
        await _permissionService.EnsureAdminPermission(key, updatedBy);

        var space = await _spaceManager.GetByKey(key);

        space.Name = model.Name;
        space.Description = model.Description;
        await _spaceManager.Update(space);
    }

    public async Task Archive(string key, UserInfo archivedBy)
    {
        EnsureIsNotAnonymous(archivedBy);
        await _permissionService.EnsureAdminPermission(key, archivedBy);
        
        var space = await _spaceManager.GetByKey(key);

        await _spaceManager.Archieve(space);

    }

    public async Task UnArchive(string key, UserInfo unArchivedBy)
    {
        EnsureIsNotAnonymous(unArchivedBy);
        await _permissionService.EnsureAdminPermission(key, unArchivedBy);
        
        
        var space = await _spaceManager.GetByKey(key);

        await _spaceManager.UnArchieve(space);
    }

    public async Task Remove(string key, UserInfo removedBy)
    {
        EnsureIsNotAnonymous(removedBy);
        await _permissionService.EnsureAdminPermission(key, removedBy);
        
        var space = await _spaceManager.GetByKey(key);

        if (space.Status != SpaceStatus.Archived)
            throw new InvalidOperationException("Only archived spaces allowed for removing");

        await _spaceManager.Remove(space);
    }

    private static void EnsureIsNotAnonymous(UserInfo userInfo)
    {
        if (userInfo == null)
            throw new AnonymousNotAllowedException();
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