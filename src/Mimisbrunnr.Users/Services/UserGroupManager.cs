using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

internal class UserGroupManager : IUserGroupManager
{
    private readonly IRepository<Group> _groupRepository;
    private readonly IRepository<UserGroup> _userGroupsRepository;

    public UserGroupManager(IRepository<Group> groupRepository, IRepository<UserGroup> userGroupsRepository)
    {
        _groupRepository = groupRepository;
        _userGroupsRepository = userGroupsRepository;
    }
    
    public Task<Group> FindByName(string name)
    {
        return Task.FromResult(_groupRepository.GetAll().FirstOrDefault(x => x.Name == name));
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
        var groupExist = _groupRepository.GetAll().Any(x => x.Id == userGroup.Id);
        if (!groupExist)
            return;

        var usersGroups = _userGroupsRepository.GetAll().Where(x => x.GroupId == userGroup.Id);
        foreach (var userInGroup in usersGroups)
        {
            await _userGroupsRepository.Delete(userInGroup);
        }

        await _groupRepository.Delete(userGroup);
    }

    public async Task AddToGroup(Group userGroup, User user)
    {
        var userInGroup =
            _userGroupsRepository.GetAll().Any(x => x.UserId == user.Id && x.GroupId == userGroup.Id);
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
            _userGroupsRepository.GetAll().FirstOrDefault(x => x.UserId == user.Id && x.GroupId == userGroup.Id);
        if (userInGroup != null)
            await _userGroupsRepository.Delete(userInGroup);
    }

    public Task<Group[]> GetUserGroups(User user)
    {
        var groups = new List<Group>();
        var userInGroups = _userGroupsRepository.GetAll().Where(x => x.UserId == user.Id).ToArray();
        foreach (var userInGroup in userInGroups)
        {
            var group = _groupRepository.GetAll().FirstOrDefault(x => x.Id == userInGroup.GroupId);
            if(group != null)
                groups.Add(group);
        }

        return Task.FromResult(groups.ToArray());
    }
}