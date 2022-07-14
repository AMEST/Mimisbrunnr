namespace Mimisbrunnr.Users;

public interface IUserGroupManager
{
    Task<Group[]> GetAll(int? offset = null);
    
    Task<Group> FindByName(string name);
    
    Task<Group> Add(string name, string description, string ownerEmail);

    Task Update(Group userGroup);

    Task Remove(Group userGroup);

    Task AddToGroup(Group userGroup, User user);

    Task RemoveFromGroup(Group userGroup, User user);

    Task<Group[]> GetUserGroups(User user);

    Task<User[]> GetUsersInGroup(Group group);
}