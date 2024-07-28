using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

internal class SpaceManagerCacheDecorator : ISpaceManager
{
    private readonly TimeSpan _maxCacheTime = TimeSpan.FromDays(1);
    private readonly TimeSpan _slidingCacheTime = TimeSpan.FromHours(8);
    private readonly ISpaceManager _inner;
    private readonly IDistributedCache _cache;

    public SpaceManagerCacheDecorator(ISpaceManager inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task AddPermission(Space space, Permission permission)
    {
        await _inner.AddPermission(space, permission);
        await DeleteSpaceFromCache(space);
    }

    public async Task Archive(Space space)
    {
        await _inner.Archive(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task<Space> Create(string key, string name, string description, SpaceType type, UserInfo owner)
    {
        var space = await _inner.Create(key, name, description, type, owner);
        await AddSpaceToCache(space);
        return space;
    }

    public Task<Space[]> FindByName(string name)
    {
        return _inner.FindByName(name);
    }

    public Task<Space> FindPersonalSpace(UserInfo user)
    {
        return _inner.FindPersonalSpace(user);
    }

    public Task<Space[]> GetAll(int? take = null, int? skip = null)
    {
        return _inner.GetAll(take, skip);
    }

    public Task<Space[]> GetAllWithPermissions(UserInfo user = null, string[] userGroups = null, int? take = null, int? skip = null)
    {
        return _inner.GetAllWithPermissions(user, userGroups, take, skip);
    }

    public Task<Space[]> GetPublicSpaces(int? take = null, int? skip = null)
    {
        return _inner.GetPublicSpaces(take, skip);
    }

    public async Task<Space> GetById(string id)
    {
        var cached = await _cache.GetAsync<Space>(GetSpaceCacheKeyById(id));
        if (cached is not null)
            return cached;
        cached = await _inner.GetById(id);
        await AddSpaceToCache(cached);
        return cached;
    }

    public async Task<Space> GetByKey(string key)
    {
        var cached = await _cache.GetAsync<Space>(GetSpaceCacheKey(key));
        if (cached is not null)
            return cached;

        cached = await _inner.GetByKey(key);
        await AddSpaceToCache(cached);
        return cached;
    }

    public async Task Remove(Space space)
    {
        await _inner.Remove(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task RemovePermission(Space space, Permission permission)
    {
        await _inner.RemovePermission(space, permission);
        await DeleteSpaceFromCache(space);
    }

    public async Task UnArchive(Space space)
    {
        await _inner.UnArchive(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task Update(Space space)
    {
        await _inner.Update(space);
        await DeleteSpaceFromCache(space);
    }

    public async Task UpdatePermission(Space space, Permission permission)
    {
        await DeleteSpaceFromCache(space);
        await _inner.UpdatePermission(space, permission);
        await DeleteSpaceFromCache(space);
    }


    private async Task AddSpaceToCache(Space space)
    {
        if (space is null) return;
        await _cache.SetAsync(key: GetSpaceCacheKey(space.Key), entry: space, options: new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _maxCacheTime,
            SlidingExpiration = _slidingCacheTime
        });
        await _cache.SetAsync(key: GetSpaceCacheKeyById(space.Id), entry: space, options: new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _maxCacheTime,
            SlidingExpiration = _slidingCacheTime
        });
    }

    private async Task DeleteSpaceFromCache(Space space)
    {
        if (space is null) return;
        await _cache.RemoveAsync(GetSpaceCacheKey(space.Key));
        await _cache.RemoveAsync(GetSpaceCacheKeyById(space.Id));
    }

    private static string GetSpaceCacheKey(string spaceKey) => $"space_cache_key_{spaceKey}";

    private static string GetSpaceCacheKeyById(string id) => $"space_cache_id_{id}";
}