using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Web.Favorites;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

internal class FavoriteServiceCacheDecorator : IFavoriteService
{
    private readonly IFavoriteService _inner;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheTime = TimeSpan.FromDays(1);

    public FavoriteServiceCacheDecorator(IFavoriteService inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }
    public async Task<FavoriteModel> Add(FavoriteCreateModel model, UserInfo user)
    {
        var result = await _inner.Add(model, user);
        await _cache.RemoveAsync(GetCacheKey(user));
        return result;
    }

    public Task<bool> EnsureInFavorites(FavoriteCreateModel model, UserInfo user)
    {
        return _inner.EnsureInFavorites(model, user);
    }

    public async Task<FavoriteModel[]> GetFavorites(UserInfo user)
    {
        var cacheKey = GetCacheKey(user);
        var cached = await _cache.GetAsync<FavoriteModel[]>(cacheKey);
        if (cached is not null)
            return cached;
        var result = await _inner.GetFavorites(user);
        await _cache.SetAsync(cacheKey, result, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = _cacheTime });
        return result;
    }

    public async Task Remove(string id, UserInfo user)
    {
        await _inner.Remove(id, user);
        await _cache.RemoveAsync(GetCacheKey(user));
    }


    private static string GetCacheKey(UserInfo user) => $"favorite_cache_{user.Email}";
}