using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Mimisbrunnr.Users.Services
{
    internal static class CacheExtensions
    {
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var cacheValue = await cache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cacheValue))
                return default;
            return JsonSerializer.Deserialize<T>(cacheValue);
        }

        public static Task SetAsync(this IDistributedCache cache, string key, object entry,
            DistributedCacheEntryOptions options)
        {
            var serializedEntry = JsonSerializer.Serialize(entry);
            return cache.SetAsync(key, Encoding.UTF8.GetBytes(serializedEntry), options);
        }
    }
}