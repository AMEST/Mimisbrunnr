using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Group;

internal class GroupService : IGroupService
{
    private readonly IUserManager _userManager;

    private readonly IUserGroupManager _userGroupManager;
    private readonly ISpaceManager _spaceManager;

    public GroupService(IUserManager userManager, IUserGroupManager userGroupManager, ISpaceManager spaceManager)
    {
        _spaceManager = spaceManager;
        _userManager = userManager;
        _userGroupManager = userGroupManager;
    }

    public async Task AddUserToGroup(string name, UserInfo user, UserInfo addedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if (group is null) throw new GroupNotFoundException();
        var addedByUser = await _userManager.GetByEmail(addedBy.Email);
        if (!group.OwnerEmails.Contains(addedBy.Email) && addedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        var userForAdd = await _userManager.GetByEmail(user.Email);
        if (userForAdd is null) throw new ArgumentOutOfRangeException();

        await _userGroupManager.AddToGroup(group, userForAdd);
    }

    public async Task<GroupModel> Create(GroupCreateModel createModel, UserInfo createdBy)
    {
        var group = await _userGroupManager.Add(createModel.Name, createModel.Description, createdBy.Email);
        return GroupMapper.Instance.ToModel(group);
    }

    public async Task<IEnumerable<GroupModel>> GetAll(GroupFilterModel filter, UserInfo requestedBy)
    {
        var user = await _userManager.GetByEmail(requestedBy.Email);
        if(!string.IsNullOrEmpty(filter?.OwnerEmail)
            && !filter.OwnerEmail.Equals(requestedBy.Email) 
            && user.Role != UserRole.Admin)
            return Array.Empty<GroupModel>();

        IEnumerable<Users.Group> groups = await _userGroupManager.GetAll(filter?.Offset);
        if(!string.IsNullOrEmpty(filter?.OwnerEmail))
            groups = groups.Where(x => x.OwnerEmails.Contains(filter.OwnerEmail));

        return groups.Select(GroupMapper.Instance.ToModel);
    }

    public async Task<GroupModel> Get(string name, UserInfo requestedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if (group is null) throw new GroupNotFoundException();
        return GroupMapper.Instance.ToModel(group);
    }
    
    public async Task<IEnumerable<UserModel>> GetUsers(string name, UserInfo requestedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if (group is null) throw new GroupNotFoundException();
        var requestedByUser = await _userManager.GetByEmail(requestedBy.Email);
        if (!group.OwnerEmails.Contains(requestedBy.Email) && requestedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        var users = await _userGroupManager.GetUsersInGroup(group);
        return users.Select(UserMapper.Instance.ToModel);
    }

    public async Task Remove(string name, UserInfo removedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if (group is null) throw new GroupNotFoundException();
        var deletedByUser = await _userManager.GetByEmail(removedBy.Email);
        if (!group.OwnerEmails.Contains(removedBy.Email) && deletedByUser.Role != UserRole.Admin ) throw new UserHasNotPermissionException();

        var removeGroupPermissionTasks = new List<Task>();
        var spaces = await _spaceManager.GetAll();
        foreach (var space in spaces)
            if (space.Permissions.Any(x => x.Group?.Name == name))
                removeGroupPermissionTasks.Add(_spaceManager.RemovePermission(space, new Permission() { Group = new GroupInfo() { Name = name } }));

        await Task.WhenAll(removeGroupPermissionTasks);

        await _userGroupManager.Remove(group);
    }

    public async Task RemoveUserFromGroup(string name, UserInfo user, UserInfo removedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if (group is null) throw new GroupNotFoundException();
        var deletedByUser = await _userManager.GetByEmail(removedBy.Email);
        if (!group.OwnerEmails.Contains(removedBy.Email) && deletedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        var userForRemove = await _userManager.GetByEmail(user.Email);
        await _userGroupManager.RemoveFromGroup(group, userForRemove);
    }

    public async Task Update(string name, GroupUpdateModel model, UserInfo updatedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if (group is null) throw new GroupNotFoundException();
        var updatedByUser = await _userManager.GetByEmail(updatedBy.Email);
        if (!group.OwnerEmails.Contains(updatedBy.Email) && updatedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        group.Description = model.Description;
        await _userGroupManager.Update(group);
    }
}