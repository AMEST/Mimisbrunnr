namespace Mimisbrunner.Users;

public interface IUserGroupManager
{
    Task<Group> FindByName(string name);
    
    Task<Group> Add(string name, string description, string ownerEmail);

    Task Update(Group userGroup);

    Task Remove(Group userGroup);

    Task AddToGroup(Group userGroup, User user);

    Task RemoveFromGroup(Group userGroup, User user);

    Task<Group[]> GetUserGroups(User user);
}