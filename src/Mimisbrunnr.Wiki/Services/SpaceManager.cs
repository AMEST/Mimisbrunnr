using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class SpaceManager : ISpaceManager, ISpaceSearcher
{
    private readonly IRepository<Space> _spaceRepository;
    private readonly IPageManager _pageManager;

    public SpaceManager(IRepository<Space> spaceRepository, IPageManager pageManager)
    {
        _spaceRepository = spaceRepository;
        _pageManager = pageManager;
    }

    public async Task<Space[]> GetAll()
    {
        var spaces = await _spaceRepository.GetAll().ToArrayAsync();
        return spaces;
    }

    public async Task<Space> GetById(string id)
    {
        var space = await _spaceRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
        return space;
    }

    public async Task<Space> GetByKey(string key)
    {
        var space = await _spaceRepository.GetAll().FirstOrDefaultAsync(x => x.Key == key.ToUpper());
        return space;
    }

    public async Task<Space> FindPersonalSpace(UserInfo user)
    {
         var personalSpace = await _spaceRepository.GetAll().FirstOrDefaultAsync(
            x => x.Type == SpaceType.Personal && x.Key == user.Email.ToUpper());
        return personalSpace;
    }

    public Task<Space[]> FindByName(string name)
    {
        return _spaceRepository.GetAll()
            .Where(x => x.Name.Contains(name)).ToArrayAsync();
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

    public async Task Update(Space space)
    {
        await _spaceRepository.Update(space);
    }

    public async Task AddPermission(Space space, Permission permission)
    {
        if (permission.Group != null && permission.User != null)
            throw new InvalidOperationException("Only one permission targer allowed. User or Group");

        if (space.Type == SpaceType.Personal && space.Permissions.Any(x => x.IsAdmin) && permission.IsAdmin)
            throw new InvalidOperationException("Cannot add more administrators to personal space");

        var newPermissions = new List<Permission>(space.Permissions) { permission };
        space.Permissions = newPermissions;
        await Update(space);
    }

    public async Task UpdatePermission(Space space, Permission permission)
    {
        if (permission.Group != null && permission.User != null)
            throw new InvalidOperationException("Only one permission targer allowed. User or Group");

        await RemovePermission(space, permission);
        await AddPermission(space, permission);
    }

    public async Task RemovePermission(Space space, Permission permission)
    {
        if (permission.Group != null && permission.User != null)
            throw new InvalidOperationException("Only one permission targer allowed. User or Group");

        if (space.Type == SpaceType.Personal && permission.IsAdmin)
            throw new InvalidOperationException("Cannot remove administrators from personal space");

        var newPermissions = space.Permissions.Where(x => 
            permission.User != null
            ? x.User is null || !x.User.Equals(permission.User)
            : x.Group is null || !x.Group.Equals(permission.Group));
        space.Permissions = newPermissions;
        await Update(space);
    }

    public async Task Archive(Space space)
    {
        space.Status = SpaceStatus.Archived;
        await Update(space);
    }

    public async Task UnArchive(Space space)
    {
        space.Status = SpaceStatus.Actual;
        await Update(space);
    }

    public async Task Remove(Space space)
    {
        var spaceHomePage = await _pageManager.GetById(space.HomePageId);
        await _pageManager.Remove(spaceHomePage, true);

        await _spaceRepository.Delete(space);
    }

    public async Task<IEnumerable<Space>> Search(string text)
    {
        var textLower = text.ToLower();
        var spaces = await _spaceRepository.GetAll()
            .Where(x => x.Name.ToLower().Contains(textLower)
                || x.Description.ToLower().Contains(textLower))
            .Take(100)
            .ToArrayAsync();
        return spaces;
    }
}