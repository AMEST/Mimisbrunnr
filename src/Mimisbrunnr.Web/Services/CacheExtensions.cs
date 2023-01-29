using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Json;

namespace Mimisbrunnr.Web.Services
{
    public static class CacheExtensions
    {
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var cacheValue = await cache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cacheValue))
                return default;
            return JsonSerializer.Deserialize<T>(cacheValue, JsonSerializerOptionsFactory.Default);
        }

        public static Task SetAsync(this IDistributedCache cache, string key, object entry,
            DistributedCacheEntryOptions options)
        {
            var serializedEntry = JsonSerializer.Serialize(entry, JsonSerializerOptionsFactory.Default);
            return cache.SetAsync(key, Encoding.UTF8.GetBytes(serializedEntry), options);
        }
    }
}