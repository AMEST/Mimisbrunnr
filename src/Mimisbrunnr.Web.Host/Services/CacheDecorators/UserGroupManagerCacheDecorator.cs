using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Services;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

/// <summary>
/// Caches group membership of the user: it is read by every permission check but changes rarely.
/// </summary>
internal class UserGroupManagerCacheDecorator : IUserGroupManager
{
    private readonly TimeSpan _groupListCacheTime = TimeSpan.FromMinutes(5);
    private readonly IUserGroupManager _inner;
    private readonly IDistributedCache _cache;

    public UserGroupManagerCacheDecorator(IUserGroupManager inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<Users.Group[]> GetAll(int? offset = null)
    {
        return _inner.GetAll(offset);
    }

    public Task<Users.Group> FindByName(string name)
    {
        return _inner.FindByName(name);
    }

    public Task<Users.Group> Add(string name, string description, string ownerEmail)
    {
        return _inner.Add(name, description, ownerEmail);
    }

    public async Task Update(Users.Group userGroup)
    {
        await _inner.Update(userGroup);
        await ClearGroupMembersCache(userGroup);
    }

    public async Task Remove(Users.Group userGroup)
    {
        var members = await _inner.GetUsersInGroup(userGroup);
        await _inner.Remove(userGroup);
        await Task.WhenAll(members.Select(user => _cache.RemoveAsync(GetUserGroupsCacheKey(user.Id))));
    }

    public async Task AddToGroup(Users.Group userGroup, Users.User user)
    {
        await _inner.AddToGroup(userGroup, user);
        await _cache.RemoveAsync(GetUserGroupsCacheKey(user.Id));
    }

    public async Task RemoveFromGroup(Users.Group userGroup, Users.User user)
    {
        await _inner.RemoveFromGroup(userGroup, user);
        await _cache.RemoveAsync(GetUserGroupsCacheKey(user.Id));
    }

    public async Task<Users.Group[]> GetUserGroups(Users.User user)
    {
        if (user is null)
            return [];

        var cached = await _cache.GetAsync<Users.Group[]>(GetUserGroupsCacheKey(user.Id));
        if (cached is not null)
            return cached;

        cached = await _inner.GetUserGroups(user);
        await _cache.SetAsync(key: GetUserGroupsCacheKey(user.Id), entry: cached, options: new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _groupListCacheTime
        });
        return cached;
    }

    public Task<Users.User[]> GetUsersInGroup(Users.Group group)
    {
        return _inner.GetUsersInGroup(group);
    }

    private async Task ClearGroupMembersCache(Users.Group group)
    {
        var members = await _inner.GetUsersInGroup(group);
        await Task.WhenAll(members.Select(user => _cache.RemoveAsync(GetUserGroupsCacheKey(user.Id))));
    }

    private static string GetUserGroupsCacheKey(string userId) => $"user_groups_cache_id_{userId}";
}
