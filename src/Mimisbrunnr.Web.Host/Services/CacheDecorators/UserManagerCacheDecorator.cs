using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Services;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

internal class UserManagerCacheDecorator : IUserManager
{
    private readonly TimeSpan _maxCacheTime = TimeSpan.FromDays(1);
    private readonly TimeSpan _slidingCacheTime = TimeSpan.FromHours(8);
    // A missing user is cached for a shorter period so a freshly created user is picked up quickly.
    private readonly TimeSpan _missingUserCacheTime = TimeSpan.FromHours(8);
    private readonly IUserManager _inner;
    private readonly IDistributedCache _cache;

    public UserManagerCacheDecorator(IUserManager inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Users.User> Add(string email, string name, string avatarUrl, UserRole role)
    {
        await _inner.Add(email, name, avatarUrl, role);
        var user = await _inner.GetByEmail(email);
        await CacheUser(user);
        return user;
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

        if (await _cache.GetAsync<bool>(GetUserCacheKeyEmailMissing(email)))
            return null;

        user = await _inner.GetByEmail(email);
        if (user is null)
        {
            await CacheMissing(GetUserCacheKeyEmailMissing(email));
            return null;
        }

        await CacheUser(user);
        return user;
    }

    public async Task<Users.User> GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        var user = await _cache.GetAsync<Users.User>(GetUserCacheKeyId(id));
        if (user is not null)
            return user;

        if (await _cache.GetAsync<bool>(GetUserCacheKeyIdMissing(id)))
            return null;

        user = await _inner.GetById(id);
        if (user is null)
        {
            await CacheMissing(GetUserCacheKeyIdMissing(id));
            return null;
        }

        await CacheUser(user);
        return user;
    }

    public Task<Users.User[]> GetByIds(params string[] ids)
    {
        return GetMany(ids, GetUserCacheKeyId, missing => _inner.GetByIds(missing));
    }

    public Task<Users.User[]> GetByEmails(params string[] emails)
    {
        return GetMany(emails, GetUserCacheKeyEmail, missing => _inner.GetByEmails(missing));
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

    private async Task<Users.User[]> GetMany(string[] keys, Func<string, string> cacheKeyFactory, Func<string[], Task<Users.User[]>> loader)
    {
        var distinctKeys = (keys ?? []).Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (distinctKeys.Length == 0)
            return [];

        var cachedUsers = await Task.WhenAll(distinctKeys.Select(key => _cache.GetAsync<Users.User>(cacheKeyFactory(key))));
        var result = new List<Users.User>(distinctKeys.Length);
        var missingKeys = new List<string>();

        for (var i = 0; i < distinctKeys.Length; i++)
        {
            if (cachedUsers[i] is not null)
                result.Add(cachedUsers[i]);
            else
                missingKeys.Add(distinctKeys[i]);
        }

        if (missingKeys.Count > 0)
        {
            var loaded = await loader([.. missingKeys]);
            await Task.WhenAll(loaded.Select(CacheUser));
            result.AddRange(loaded);
        }

        return [.. result];
    }

    private Task CacheUser(Users.User user)
    {
        if (user is null)
            return Task.CompletedTask;

        return Task.WhenAll(
            _cache.SetAsync(key: GetUserCacheKeyEmail(user.Email), entry: user, options: CacheOptions()),
            _cache.SetAsync(key: GetUserCacheKeyId(user.Id), entry: user, options: CacheOptions()),
            _cache.RemoveAsync(GetUserCacheKeyEmailMissing(user.Email)),
            _cache.RemoveAsync(GetUserCacheKeyIdMissing(user.Id)));
    }

    private Task CacheMissing(string cacheKey)
    {
        return _cache.SetAsync(key: cacheKey, entry: true, options: new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = _missingUserCacheTime
        });
    }

    private async Task ClearCache(Users.User user)
    {
        await _cache.RemoveAsync(GetUserCacheKeyEmail(user.Email));
        await _cache.RemoveAsync(GetUserCacheKeyId(user.Id));
        await _cache.RemoveAsync(GetUserCacheKeyEmailMissing(user.Email));
        await _cache.RemoveAsync(GetUserCacheKeyIdMissing(user.Id));
    }

    private DistributedCacheEntryOptions CacheOptions() => new()
    {
        AbsoluteExpirationRelativeToNow = _maxCacheTime,
        SlidingExpiration = _slidingCacheTime
    };

    private static string GetUserCacheKeyEmail(string email) => $"user_cache_v2_email_{email.ToLower()}";

    private static string GetUserCacheKeyId(string id) => $"user_cache_v2_id_{id.ToLower()}";

    private static string GetUserCacheKeyEmailMissing(string email) => $"user_cache_v2_missing_email_{email.ToLower()}";

    private static string GetUserCacheKeyIdMissing(string id) => $"user_cache_v2_missing_id_{id.ToLower()}";
}
