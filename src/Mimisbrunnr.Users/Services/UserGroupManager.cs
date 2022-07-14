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
        return _groupRepository.GetAll().FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<Group> Add(string name, string description, string ownerEmail)
    {
        var gorup = new Group()
        {
            Name = name,
            Description = description,
            OwnerEmails = new[] { ownerEmail }
        };
        await _groupRepository.Create(gorup);
        return gorup;
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

        var usersGroups = await _userGroupsRepository.GetAll().Where(x => x.GroupId == userGroup.Id).ToArrayAsync();
        foreach (var userInGroup in usersGroups)
        {
            await _userGroupsRepository.Delete(userInGroup);
        }

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
        var groups = new List<Group>();
        var userInGroups = await _userGroupsRepository.GetAll().Where(x => x.UserId == user.Id).ToArrayAsync();
        foreach (var userInGroup in userInGroups)
        {
            var group = await _groupRepository.GetAll().FirstOrDefaultAsync(x => x.Id == userInGroup.GroupId);
            if(group != null)
                groups.Add(group);
        }

        return groups.ToArray();
    }

    public async Task<User[]> GetUsersInGroup(Group group)
    {
        var usersInGroups = await _userGroupsRepository.GetAll().Where(x => x.GroupId == group.Id).ToArrayAsync();
        var users = new List<User>();
        foreach( var userGroup in usersInGroups)
        {
            var user = await _userManager.GetById(userGroup.UserId);
            if(user is not null)
                users.Add(user);
        }
        return users.ToArray();
    }
}