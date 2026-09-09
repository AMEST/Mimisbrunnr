using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace Mimisbrunnr.Web.Tests.Host.CacheDecorators;

internal sealed class FakeDistributedCache : IDistributedCache
{
    private readonly Dictionary<string, byte[]> _store = new();

    public byte[] Get(string key) => _store.TryGetValue(key, out var value) ? value : null;

    public async Task<byte[]> GetAsync(string key, CancellationToken token = default) => Get(key);

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        _store[key] = value;
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key)
    {
        _store.Remove(key);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public bool Contains(string key) => _store.ContainsKey(key);
}