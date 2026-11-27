using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Host.Services.CacheDecorators;

internal class PageManagerCacheDecorator : IPageManager
{
    private readonly TimeSpan _maxCacheTime = TimeSpan.FromDays(1);
    private readonly TimeSpan _slidingCacheTime = TimeSpan.FromMinutes(30);
    private readonly IPageManager _inner;
    private readonly IDistributedCache _cache;

    public PageManagerCacheDecorator(IPageManager inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Page> Copy(Page source, Page destinationParentPage)
    {
        var page = await _inner.Copy(source, destinationParentPage);
        await ClearCache(page);
        return page;
    }

    public async Task<Page> Create(string spaceId, string name, string content, UserInfo createdBy, string parentPageId = null)
    {
        var page = await _inner.Create(spaceId, name, content, createdBy, parentPageId);
        await ClearCache(page);
        return page;
    }

    public Task<Page[]> FindByName(string name)
    {
        return _inner.FindByName(name);
    }

    public Task<Page[]> GetAllChilds(Page page, bool lightContract = true)
    {
        return _inner.GetAllChilds(page, lightContract);
    }

    public Task<Page[]> GetAllOnSpace(Space space)
    {
        return _inner.GetAllOnSpace(space);
    }

    public async Task<Page> GetById(string id)
    {
        var cached = await _cache.GetAsync<Page>(GetPageCacheName(id));
        if (cached is not null)
            return cached;
        cached = await _inner.GetById(id);
        await StoreInCache(cached);
        return cached;
    }

    public async Task<Page[]> GetByIds(params string[] ids)
    {
        if (ids is null || ids.Length == 0)
            return [];

        var distinctIds = ids.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.Ordinal).ToArray();
        if (distinctIds.Length == 0)
            return [];

        var cachedPages = await Task.WhenAll(distinctIds.Select(id => _cache.GetAsync<Page>(GetPageCacheName(id))));
        var result = new List<Page>(distinctIds.Length);
        var missingIds = new List<string>();

        for (var i = 0; i < distinctIds.Length; i++)
        {
            if (cachedPages[i] is not null)
                result.Add(cachedPages[i]);
            else
                missingIds.Add(distinctIds[i]);
        }

        if (missingIds.Count > 0)
        {
            var loaded = await _inner.GetByIds([.. missingIds]);
            await Task.WhenAll(loaded.Select(StoreInCache));
            result.AddRange(loaded);
        }

        return [.. result];
    }

    public async Task<Page> Move(Page source, Page destinationParentPage, bool withChilds = true)
    {
        var copiedPage = await _inner.Move(source, destinationParentPage, withChilds);
        await ClearCache(copiedPage);
        return copiedPage;
    }

    public async Task Remove(Page page, bool deleteChild = false)
    {
        await _inner.Remove(page, deleteChild);
        await ClearCache(page);
    }

    public async Task Update(Page page, string name, string content, UserInfo updatedBy)
    {
        await _inner.Update(page, name, content, updatedBy);
        await ClearCache(page);
    }

    public Task<HistoricalPage[]> GetAllVersions(Page page)
    {
        return _inner.GetAllVersions(page);
    }

    public Task<HistoricalPage> GetVersionByPageId(string id, long version)
    {
        return _inner.GetVersionByPageId(id, version);
    }

    public async Task<Page> RestoreVersion(Page page, long version, UserInfo restoredBy)
    {
        var result = await _inner.RestoreVersion(page, version, restoredBy);
        await ClearCache(page);
        return result;
    }

    public Task RemoveVersion(Page page, long version)
    {
        return _inner.RemoveVersion(page, version);
    }

    private Task StoreInCache(Page page)
    {
        if (page is null)
            return Task.CompletedTask;
        return _cache.SetAsync(key: GetPageCacheName(page.Id), entry: page, options: new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = _maxCacheTime,
                SlidingExpiration = _slidingCacheTime
            });
    }

    private async Task ClearCache(Page page)
    {
        if (page is null)
            return;
        await _cache.RemoveAsync(GetPageCacheName(page.Id));
    }

    private static string GetPageCacheName(string id) => $"page_manager_cache_id_{id}";
}
