using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class SpaceManager : ISpaceManager
{
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromHours(12);
    private readonly IRepository<Space> _spaceRepository;
    private readonly IPageManager _pageManager;
    private readonly IDistributedCache _distributedCache;

    public SpaceManager(IRepository<Space> spaceRepository, IPageManager pageManager, IDistributedCache distributedCache)
    {
        _spaceRepository = spaceRepository;
        _pageManager = pageManager;
        _distributedCache = distributedCache;
    }

    public Task<Space[]> GetAll()
    {
        return _spaceRepository.GetAll().ToArrayAsync();
    }

    public async Task<Space> GetById(string id)
    {
        var space = await _distributedCache.GetAsync<Space>(GetSpaceCacheKeyById(id));
        if (space is not null)
            return space;
        space = await _spaceRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
        await AddSpaceToCache(space);
        return space;
    }

    public async Task<Space> GetByKey(string key)
    {
        var space = await _distributedCache.GetAsync<Space>(GetSpaceCacheKey(key));
        if (space is not null)
            return space;

        space = await _spaceRepository.GetAll().FirstOrDefaultAsync(x => x.Key == key.ToUpper());
        await AddSpaceToCache(space);
        return space;
    }

    public async Task<Space> FindPersonalSpace(UserInfo user)
    {
        var personalSpace = await _spaceRepository.GetAll().FirstOrDefaultAsync(
            x => x.Type != SpaceType.Personal && x.Type != SpaceType.Public
                 && x.Permissions.Any(p => p.IsAdmin && p.User != null && p.User.Email == user.Email)
        );
        return personalSpace;
    }

    public Task<Space[]> FindByName(string name)
    {
        return _spaceRepository.GetAll()
            .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToArrayAsync();
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
        await AddSpaceToCache(space);
        return space;
    }

    public async Task Update(Space space)
    {
        await _spaceRepository.Update(space);
        await DeleteSpaceFromCache(space);
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
        await DeleteSpaceFromCache(space);
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

        var newPermissions = space.Permissions.Where(x => permission.User != null
            ? !x.User.Equals(permission.User)
            : !x.Group.Equals(permission.Group));
        space.Permissions = newPermissions;
        await Update(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task Archive(Space space)
    {
        space.Status = SpaceStatus.Archived;
        await Update(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task UnArchive(Space space)
    {
        space.Status = SpaceStatus.Actual;
        await Update(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task Remove(Space space)
    {
        var spacePages = await _pageManager.GetAllOnSpace(space);
        var pageRemoveTasks = new List<Task>();
        foreach (var page in spacePages)
            pageRemoveTasks.Add(_pageManager.Remove(page));

        await Task.WhenAll(pageRemoveTasks);
        await _spaceRepository.Delete(space);
        await DeleteSpaceFromCache(space);
    }

    private async Task AddSpaceToCache(Space space)
    {
        if(space is null) return;
        await _distributedCache.SetAsync(GetSpaceCacheKey(space.Key), space, new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _defaultCacheTime
        });
        await _distributedCache.SetAsync(GetSpaceCacheKeyById(space.Id), space, new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _defaultCacheTime
        });
    }

    private async Task DeleteSpaceFromCache(Space space)
    {
        if(space is null) return;
        await _distributedCache.RemoveAsync(GetSpaceCacheKey(space.Key));
        await _distributedCache.RemoveAsync(GetSpaceCacheKeyById(space.Id));
    }

    private static string GetSpaceCacheKey(string spaceKey) => $"space_cache_key_{spaceKey}";

    private static string GetSpaceCacheKeyById(string id) => $"space_cache_id_{id}";
}