using Mimisbrunnr.Users;
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
        var spaces = await _spaceManager.GetAll();
        if (requestedBy is null)
            return spaces.Where(x => x.Type == SpaceType.Public).Select(x => x.ToModel()).ToArray();

        var user = await _userManager.GetByEmail(requestedBy.Email);
        if (user.Role == UserRole.Admin)
            return spaces.ToList().Select(x => x.ToModel()).ToArray();

        var visibleSpaces = new List<SpaceModel>();
        var groups = await GetUserGroups(requestedBy);
        foreach (var space in spaces)
        {
            if (space.Type == SpaceType.Public)
            {
                visibleSpaces.Add(space.ToModel());
                continue;
            }
            var permission = FindPermission(space.Permissions.ToArray(), requestedBy, groups);
            if (permission is not null)
                visibleSpaces.Add(space.ToModel());
        }

        return visibleSpaces.ToArray();
    }

    public async Task<SpaceModel> GetByKey(string key, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        await _permissionService.EnsureViewPermission(key, requestedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        return space.ToModel();
    }

    public async Task<UserPermissionModel> GetPermission(string key, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        if (requestedBy == null)
            return new UserPermissionModel() { CanView = true };

        var user = await _userManager.GetByEmail(requestedBy.Email);
        if (user.Role == UserRole.Admin)
            return new UserPermissionModel() { CanView = true, CanEdit = true, CanRemove = true, IsAdmin = true };

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        var userGroups = await GetUserGroups(requestedBy);
        var userPermission = FindPermission(space.Permissions.ToArray(), requestedBy, userGroups);

        if (userPermission == null)
            return new UserPermissionModel() { CanView = space.Type == SpaceType.Public };

        return userPermission.ToModel();
    }

    public async Task<SpacePermissionModel[]> GetSpacePermissions(string key, UserInfo requestedBy)
    {
        EnsureIsNotAnonymous(requestedBy);
        await _permissionService.EnsureAdminPermission(key, requestedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        return space.Permissions.Select(x => x.ToSpacePermissionModel()).ToArray();
    }

    public async Task<SpacePermissionModel> AddPermission(string key, SpacePermissionModel model, UserInfo addedBy)
    {
        await _permissionService.EnsureAdminPermission(key, addedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        await _spaceManager.AddPermission(space, model.ToEntity());

        return model;
    }

    public async Task UpdatePermission(string key, SpacePermissionModel model, UserInfo updatedBy)
    {
        await _permissionService.EnsureAdminPermission(key, updatedBy);

        var space = await _spaceManager.GetByKey(key);
        if (space == null)
            throw new SpaceNotFoundException();

        await _spaceManager.UpdatePermission(space, model.ToEntity());
    }

    public async Task RemovePermission(string key, SpacePermissionModel model, UserInfo removedBy)
    {
        await _permissionService.EnsureAdminPermission(key, removedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        await _spaceManager.RemovePermission(space, model.ToEntity());
    }

    public async Task<SpaceModel> Create(SpaceCreateModel model, UserInfo createdBy)
    {
        EnsureIsNotAnonymous(createdBy);
        if (model.Type == SpaceTypeModel.Personal)
        {
            var personalSpace = await _spaceManager.FindPersonalSpace(createdBy);
            if (personalSpace != null)
                throw new InvalidOperationException("Only one personal space allowed");

            if (model.Key != createdBy.Email)
                model.Key = createdBy.Email;
            if (model.Name != createdBy.Name)
                model.Name = createdBy.Name;
        }

        var space = await _spaceManager.GetByKey(model.Key);
        if (space != null)
            throw new InvalidOperationException("Cannot create space because space with same key already exists");

        space = await _spaceManager.Create(model.Key.ToUpper(), model.Name, model.Description, (SpaceType)model.Type,
            createdBy);
        return space.ToModel();
    }

    public async Task Update(string key, SpaceUpdateModel model, UserInfo updatedBy)
    {
        EnsureIsNotAnonymous(updatedBy);
        await _permissionService.EnsureAdminPermission(key, updatedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        space.Name = model.Name;
        space.Description = model.Description;
        if (space.Type != SpaceType.Personal && model.Public is not null)
        {
            space.Type = model.Public.Value ? SpaceType.Public : SpaceType.Private;
        }

        await _spaceManager.Update(space);
    }

    public async Task Archive(string key, UserInfo archivedBy)
    {
        EnsureIsNotAnonymous(archivedBy);
        await _permissionService.EnsureAdminPermission(key, archivedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        await _spaceManager.Archive(space);
    }

    public async Task UnArchive(string key, UserInfo unArchivedBy)
    {
        EnsureIsNotAnonymous(unArchivedBy);
        await _permissionService.EnsureAdminPermission(key, unArchivedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        await _spaceManager.UnArchive(space);
    }

    public async Task Remove(string key, UserInfo removedBy)
    {
        EnsureIsNotAnonymous(removedBy);
        await _permissionService.EnsureAdminPermission(key, removedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        if (space.Status != SpaceStatus.Archived)
            throw new InvalidOperationException("Only archived spaces allowed for removing");

        await _spaceManager.Remove(space);
    }

    private static void EnsureIsNotAnonymous(UserInfo userInfo)
    {
        if (userInfo == null)
            throw new AnonymousNotAllowedException();
    }

    private static void EnsureSpaceExists(Space space)
    {
        if (space == null)
            throw new SpaceNotFoundException();
    }

    private async Task<Group[]> GetUserGroups(UserInfo userInfo)
    {
        var user = await _userManager.GetByEmail(userInfo?.Email);
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