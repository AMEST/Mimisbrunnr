using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Group;

public class GroupService : IGroupService
{
    private readonly IUserManager _userManager;

    private readonly IUserGroupManager _userGroupManager;

    public GroupService(IUserManager userManager, IUserGroupManager userGroupManager)
    {
        _userManager = userManager;
        _userGroupManager = userGroupManager;
    }

    public async Task AddUserToGroup(string name, UserInfo user, UserInfo addedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if(group is null) throw new GroupNotFoundException();
        var addedByUser = await _userManager.GetByEmail(addedBy.Email);
        if(!group.OwnerEmails.Contains(addedBy.Email) && addedByUser.Role != UserRole.Admin ) throw new UserHasNotPermissionException();

        var userForAdd = await _userManager.GetByEmail(user.Email);
        if(userForAdd is null) throw new ArgumentOutOfRangeException();

        await _userGroupManager.AddToGroup(group, userForAdd);
    }

    public async Task<GroupModel> Create(GroupCreateModel createModel, UserInfo createdBy)
    {
        var group = await _userGroupManager.Add(createModel.Name, createModel.Description, createdBy.Email);
        return group.ToModel(true);
    }

    public async Task<GroupModel[]> GetAll(UserInfo requestedBy)
    {
        var user = await _userManager.GetByEmail(requestedBy.Email);
        var groups = await _userGroupManager.GetAll();
        return groups.Select( x => x.ToModel(user.Role == UserRole.Admin)).ToArray();
    }

    public async Task<UserModel[]> GetUsers(string name, UserInfo requestedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if(group is null) throw new GroupNotFoundException();
        var requestedByUser = await _userManager.GetByEmail(requestedBy.Email);
        if(!group.OwnerEmails.Contains(requestedBy.Email) && requestedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        var users = await _userGroupManager.GetUsersInGroup(group);
        return users.Select(x => x.ToModel()).ToArray();
    }

    public async Task Remove(string name, UserInfo removedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if(group is null) throw new GroupNotFoundException();
        var deletedByUser = await _userManager.GetByEmail(removedBy.Email);
        if(!group.OwnerEmails.Contains(removedBy.Email) && deletedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        await _userGroupManager.Remove(group);
    }

    public async Task RemoveUserFromGroup(string name, UserInfo user, UserInfo removedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if(group is null) throw new GroupNotFoundException();
        var deletedByUser = await _userManager.GetByEmail(removedBy.Email);
        if(!group.OwnerEmails.Contains(removedBy.Email) && deletedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();
        
        var userForRemove = await _userManager.GetByEmail(user.Email);
        await _userGroupManager.RemoveFromGroup(group, userForRemove);
    }

    public async Task Update(string name, GroupUpdateModel model, UserInfo updatedBy)
    {
        var group = await _userGroupManager.FindByName(name);
        if(group is null) throw new GroupNotFoundException();
        var updatedByUser = await _userManager.GetByEmail(updatedBy.Email);
        if(!group.OwnerEmails.Contains(updatedBy.Email) && updatedByUser.Role != UserRole.Admin) throw new UserHasNotPermissionException();

        group.Description = model.Description;
        group.Name = model.Name;
        await _userGroupManager.Update(group);
    }
}