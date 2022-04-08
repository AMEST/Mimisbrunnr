using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class SpaceManager : ISpaceManager
{
    private readonly IRepository<Space> _spaceRepository;
    private readonly IPageManager _pageManager;

    public SpaceManager(IRepository<Space> spaceRepository, IPageManager pageManager)
    {
        _spaceRepository = spaceRepository;
        _pageManager = pageManager;
    }

    public Task<Space[]> GetAll()
    {
        return Task.FromResult(_spaceRepository.GetAll().ToArray());
    }

    public Task<Space> GetById(string id)
    {
        return Task.FromResult(_spaceRepository.GetAll().SingleOrDefault(x => x.Id == id));
    }

    public Task<Space> GetByKey(string key)
    {
        return Task.FromResult(_spaceRepository.GetAll().SingleOrDefault(x => x.Key == key.ToUpper()));
    }

    public Task<Space> FindPersonalSpace(UserInfo user)
    {
        var personalSpace = _spaceRepository.GetAll().FirstOrDefault(
            x => x.Type != SpaceType.Personal && x.Type != SpaceType.Public
                 && x.Permissions.Any(p => p.IsAdmin && p.User != null && p.User.Email == user.Email)
        );
        return Task.FromResult(personalSpace);
    }

    public Task<Space[]> FindByName(string name)
    {
        return Task.FromResult(_spaceRepository.GetAll()
            .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToArray());
    }

    public async Task<Space> Create(string key, string name, string description, SpaceType type, UserInfo owner)
    {
        var space = new Space()
        {
            Key = key,
            Name = name,
            Description = description,
            Type = type,
            Permissions = new[]
                { new Permission() { User = owner, IsAdmin = true, CanEdit = true, CanRemove = true, CanView = true } },
            Status = SpaceStatus.Actual
        };
        await _spaceRepository.Create(space);
        var homePage = await _pageManager.Create(space.Id, name, $"# {description}   ", owner);
        space.HomePageId = homePage.Id;
        await Update(space);
        return space;
    }

    public Task Update(Space space)
    {
        return _spaceRepository.Update(space);
    }

    public Task AddPermission(Space space, Permission permission)
    {
        if (permission.Group != null && permission.User != null)
            throw new InvalidOperationException("Only one permission targer allowed. User or Group");
        
        if (space.Type == SpaceType.Personal && space.Permissions.Any(x => x.IsAdmin) && permission.IsAdmin)
            throw new InvalidOperationException("Cannot add more administrators to personal space");
        
        var newPermissions = new List<Permission>(space.Permissions) { permission };
        space.Permissions = newPermissions;
        return Update(space);
    }

    public async Task UpdatePermission(Space space, Permission permission)
    {
        if (permission.Group != null && permission.User != null)
            throw new InvalidOperationException("Only one permission targer allowed. User or Group");

        await RemovePermission(space, permission);
        await AddPermission(space, permission);
    }

    public Task RemovePermission(Space space, Permission permission)
    {
        if (permission.Group != null && permission.User != null)
            throw new InvalidOperationException("Only one permission targer allowed. User or Group");
        
        if (space.Type == SpaceType.Personal && permission.IsAdmin)
            throw new InvalidOperationException("Cannot remove administrators from personal space");
        
        var newPermissions = space.Permissions.Where(x => permission.User != null 
            ? !x.User.Equals(permission.User)
            : !x.Group.Equals(permission.Group));
        space.Permissions = newPermissions;
        return Update(space);
    }

    public Task Archive(Space space)
    {
        space.Status = SpaceStatus.Archived;
        return Update(space);
    }

    public Task UnArchive(Space space)
    {
        space.Status = SpaceStatus.Actual;
        return Update(space);
    }

    public async Task Remove(Space space)
    {
        var spacePages = await _pageManager.GetAllOnSpace(space);
        foreach (var page in spacePages)
        {
            await _pageManager.Remove(page);
        }

        await _spaceRepository.Delete(space);
    }
}