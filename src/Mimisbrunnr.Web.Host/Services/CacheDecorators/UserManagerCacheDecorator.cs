using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Services;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

internal class UserManagerCacheDecorator : IUserManager
{
    private readonly TimeSpan _maxCacheTime = TimeSpan.FromDays(1);
    private readonly TimeSpan _slidingCacheTime = TimeSpan.FromHours(8);
    private readonly IUserManager _inner;
    private readonly IDistributedCache _cache;

    public UserManagerCacheDecorator(IUserManager inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task Add(string email, string name, string avatarUrl, UserRole role)
    {
        await _inner.Add(email, name, avatarUrl, role);
        var user = await _inner.GetByEmail(email);
        await CacheUser(user, GetUserCacheKeyEmail(user.Email));
        await CacheUser(user, GetUserCacheKeyId(user.Id));
    }

    public async Task ChangeRole(Users.User user, UserRole role)
    {
        await _inner.ChangeRole(user, role);
        await ClearCache(user);
    }

    public async Task Disable(Users.User user)
    {
        await _inner.Disable(user);
        await ClearCache(user);
    }

    public async Task Enable(Users.User user)
    {
        await _inner.Enable(user);
        await ClearCache(user);
    }

    public async Task<Users.User> GetByEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var user = await _cache.GetAsync<Users.User>(GetUserCacheKeyEmail(email));
        if (user is not null)
            return user;

        user = await _inner.GetByEmail(email);
        if (user is null)
            return null;

        await CacheUser(user, GetUserCacheKeyEmail(user.Email));
        await CacheUser(user, GetUserCacheKeyId(user.Id));
        return user;
    }

    public async Task<Users.User> GetById(string id)
    {
        var user = await _cache.GetAsync<Users.User>(GetUserCacheKeyId(id));
        if (user is not null)
            return user;

        user = await _inner.GetById(id);
        if (user is null)
            return null;

        await CacheUser(user, GetUserCacheKeyEmail(user.Email));
        await CacheUser(user, GetUserCacheKeyId(user.Id));
        return user;
    }

    public Task<Users.User[]> GetUsers(int? offset = null)
    {
        return _inner.GetUsers(offset);
    }

    public async Task UpdateUserInfo(Users.User user)
    {
        await _inner.UpdateUserInfo(user);
        await ClearCache(user);
    }

    private async Task CacheUser(Users.User user, string cacheKey)
    {
        await _cache.SetAsync(key: cacheKey, entry: user, options: new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _maxCacheTime,
            SlidingExpiration = _slidingCacheTime
        });
    }

    private async Task ClearCache(Users.User user)
    {
        await _cache.RemoveAsync(GetUserCacheKeyEmail(user.Email));
        await _cache.RemoveAsync(GetUserCacheKeyId(user.Id));
    }

    private static string GetUserCacheKeyEmail(string email) => $"user_cache_email_{email.ToLower()}";

    private static string GetUserCacheKeyId(string id) => $"user_cache_id_{id.ToLower()}";
}