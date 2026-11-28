using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

internal class UserGroupManager : IUserGroupManager
{
    private const int DefaultMaxGroupsCount = 15;
    private readonly IRepository<Group> _groupRepository;
    private readonly IRepository<UserGroup> _userGroupsRepository;
    private readonly IUserManager _userManager;

    public UserGroupManager(IRepository<Group> groupRepository, IRepository<UserGroup> userGroupsRepository, IUserManager userManager)
    {
        _groupRepository = groupRepository;
        _userGroupsRepository = userGroupsRepository;
        _userManager = userManager;
    }

    public Task<Group[]> GetAll(int? offset = null)
    {
        if(offset is null)
            return _groupRepository.GetAll().ToArrayAsync();

        return _groupRepository.GetAll()
            .Skip(offset.Value)
            .Take(DefaultMaxGroupsCount)
            .ToArrayAsync();
    }
    
    public Task<Group> FindByName(string name)
    {
        return _groupRepository.GetAll().FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
    }

    public async Task<Group> Add(string name, string description, string ownerEmail)
    {
        var group = new Group()
        {
            Name = name,
            Description = description,
            OwnerEmails = [ownerEmail]
        };
        await _groupRepository.Create(group);
        return group;
    }

    public Task Update(Group userGroup)
    {
        return _groupRepository.Update(userGroup);
    }

    public async Task Remove(Group userGroup)
    {
        var groupExist = await _groupRepository.GetAll().AnyAsync(x => x.Id == userGroup.Id);
        if (!groupExist)
            return;

        await _userGroupsRepository.DeleteAll(x => x.GroupId == userGroup.Id);
        await _groupRepository.Delete(userGroup);
    }

    public async Task AddToGroup(Group userGroup, User user)
    {
        var userInGroup =
           await _userGroupsRepository.GetAll().AnyAsync(x => x.UserId == user.Id && x.GroupId == userGroup.Id);
        if (userInGroup)
            return;
        await _userGroupsRepository.Create(new UserGroup()
        {
            GroupId = userGroup.Id,
            UserId = user.Id
        });
    }

    public async Task RemoveFromGroup(Group userGroup, User user)
    {
        var userInGroup =
           await _userGroupsRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == user.Id && x.GroupId == userGroup.Id);
        if (userInGroup != null)
            await _userGroupsRepository.Delete(userInGroup);
    }

    public async Task<Group[]> GetUserGroups(User user)
    {
        if (user is null)
            return [];

        var groupIds = await _userGroupsRepository.GetAll()
            .Where(x => x.UserId == user.Id)
            .Select(x => x.GroupId)
            .ToArrayAsync();
        if (groupIds.Length == 0)
            return [];

        return await _groupRepository.GetAll().Where(x => groupIds.Contains(x.Id)).ToArrayAsync();
    }

    public async Task<User[]> GetUsersInGroup(Group group)
    {
        var userIds = await _userGroupsRepository.GetAll()
            .Where(x => x.GroupId == group.Id)
            .Select(x => x.UserId)
            .ToArrayAsync();
        if (userIds.Length == 0)
            return [];

        return await _userManager.GetByIds(userIds);
    }
}