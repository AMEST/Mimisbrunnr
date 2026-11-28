using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Mimisbrunnr.Web.Infrastructure
{
    public static class CacheExtensions
    {
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var cacheValue = await cache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cacheValue))
                return default;
            return JsonSerializer.Deserialize<T>(cacheValue);
        }

        /// <summary>
        /// Reads a cached value and reports whether the entry exists at all. Unlike <see cref="GetAsync{T}"/>
        /// this method can distinguish a missing entry from a cached <c>null</c>.
        /// </summary>
        public static async Task<(bool Found, T Value)> TryGetValueAsync<T>(this IDistributedCache cache, string key)
        {
            var cacheValue = await cache.GetStringAsync(key);
            if (cacheValue is null)
                return (false, default);
            return (true, JsonSerializer.Deserialize<T>(cacheValue));
        }

        public static Task SetAsync(this IDistributedCache cache, string key, object entry,
            DistributedCacheEntryOptions options)
        {
            var serializedEntry = JsonSerializer.Serialize(entry);
            return cache.SetAsync(key, Encoding.UTF8.GetBytes(serializedEntry), options);
        }

        public static Task SetValueAsync<T>(this IDistributedCache cache, string key, T entry, TimeSpan timeToLive)
        {
            var serializedEntry = JsonSerializer.Serialize(entry);
            return cache.SetAsync(key, Encoding.UTF8.GetBytes(serializedEntry), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            });
        }
    }
}
