﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Integration.Group;

namespace Mimisbrunnr.Web.Wiki;

internal class SpaceService : ISpaceService, ISpaceDisplayService
{
    private readonly IPermissionService _permissionService;
    private readonly ISpaceManager _spaceManager;
    private readonly IUserManager _userManager;
    private readonly IUserGroupManager _userGroupManager;
    private readonly IApplicationConfigurationManager _applicationConfigurationManager;
    private readonly ILogger<SpaceService> _logger;

    public SpaceService(IPermissionService permissionService,
        ISpaceManager spaceManager,
        IUserManager userManager,
        IUserGroupManager userGroupManager,
        IApplicationConfigurationManager applicationConfigurationManager,
        ILogger<SpaceService> logger
    )
    {
        _permissionService = permissionService;
        _spaceManager = spaceManager;
        _userManager = userManager;
        _userGroupManager = userGroupManager;
        _applicationConfigurationManager = applicationConfigurationManager;
        _logger = logger;
    }

    public async Task<SpaceModel[]> GetAll(UserInfo requestedBy, int? take = null, int? skip = null)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        var visibleSpaces = await FindUserVisibleSpaces(requestedBy, take, skip);
        return visibleSpaces.Select(x => x.ToModel()).ToArray();
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

        return userPermission.ToUserPermissions();
    }

    public async Task<SpacePermissionModel[]> GetSpacePermissions(string key, UserInfo requestedBy)
    {
        EnsureIsNotAnonymous(requestedBy);
        await _permissionService.EnsureAdminPermission(key, requestedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        return space.Permissions.Select(x => x.ToSpacePermissions()).ToArray();
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

        if (model.Type == SpaceTypeModel.Personal)
        {
            space.AvatarUrl = createdBy.AvatarUrl;
            await _spaceManager.Update(space);
        }

        return space.ToModel();
    }

    public async Task Update(string key, SpaceUpdateModel model, UserInfo updatedBy)
    {
        EnsureIsNotAnonymous(updatedBy);
        await _permissionService.EnsureAdminPermission(key, updatedBy);

        var space = await _spaceManager.GetByKey(key);
        EnsureSpaceExists(space);

        var appConfig = await _applicationConfigurationManager.Get();
        if (appConfig.CustomHomepageEnabled
            && appConfig.CustomHomepageSpaceKey.Equals(key, StringComparison.OrdinalIgnoreCase)
            && model.Public.HasValue
            && !model.Public.Value
            && space.Type == SpaceType.Public)
            throw new InvalidOperationException(
                "Cannot change space visible type to private, because space homepage used for wiki homepage");

        space.Name = model.Name;
        space.Description = model.Description;
        if (space.Type != SpaceType.Personal
            && !string.IsNullOrEmpty(model.AvatarUrl)
            && model.AvatarUrl.StartsWith("/api/attachment"))
            space.AvatarUrl = model.AvatarUrl;

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

        var appConfig = await _applicationConfigurationManager.Get();
        if (appConfig.CustomHomepageEnabled
            && appConfig.CustomHomepageSpaceKey.Equals(key, StringComparison.OrdinalIgnoreCase)
            && space.Type == SpaceType.Public)
            throw new InvalidOperationException("Cannot remove space, because space homepage used for wiki homepage");

        if (space.Status != SpaceStatus.Archived)
            throw new InvalidOperationException("Only archived spaces allowed for removing");

        await _spaceManager.Remove(space);
        _logger.LogInformation("User `{User}` remove space `{SpaceKey}`", removedBy.Email, key);
    }

    public async Task<IEnumerable<Space>> FindUserVisibleSpaces(UserInfo userInfo, int? take = null, int? skip = null)
    {
        if (userInfo is null)
            return await _spaceManager.GetPublicSpaces(take, skip);

        var user = await _userManager.GetByEmail(userInfo.Email);
        if (user.Role == UserRole.Admin)
            return await _spaceManager.GetAll(take, skip);

        var userGroups = await _userGroupManager.GetUserGroups(user);
        return await _spaceManager.GetAllWithPermissions(userInfo, userGroups.Select(x => x.Name).ToArray(), take, skip);
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

    private async Task<Users.Group[]> GetUserGroups(UserInfo userInfo)
    {
        var user = await _userManager.GetByEmail(userInfo?.Email);
        var userGroups = await _userGroupManager.GetUserGroups(user);
        return userGroups;
    }

    private static Permission FindPermission(Permission[] permissions, UserInfo user, Users.Group[] groups)
    {
        var userPermission =
            permissions.FirstOrDefault(x => x.User != null && x.User.Equals(user));
        var groupPermission = permissions.FirstOrDefault(x =>
            x.Group != null && groups.Any(g => g.Name.Equals(x.Group.Name)));
        return userPermission ?? groupPermission;
    }
}