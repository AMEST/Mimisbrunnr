using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Json;

namespace Mimisbrunnr.Web.Services
{
    /// <summary>
    /// Extension methods for <see cref="IDistributedCache"/>
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Gets a value from the distributed cache and deserializes it
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="cache">Distributed cache instance</param>
        /// <param name="key">Cache key</param>
        /// <returns>The deserialized cached value, or the default value if not found</returns>
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var cacheValue = await cache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cacheValue))
                return default;
            return JsonSerializer.Deserialize<T>(cacheValue, JsonSerializerOptionsFactory.Default);
        }

        /// <summary>
        /// Serializes a value and stores it in the distributed cache
        /// </summary>
        /// <param name="cache">Distributed cache instance</param>
        /// <param name="key">Cache key</param>
        /// <param name="entry">Value to store</param>
        /// <param name="options">Cache entry options</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public static Task SetAsync(this IDistributedCache cache, string key, object entry,
            DistributedCacheEntryOptions options)
        {
            var serializedEntry = JsonSerializer.Serialize(entry, JsonSerializerOptionsFactory.Default);
            return cache.SetAsync(key, Encoding.UTF8.GetBytes(serializedEntry), options);
        }
    }
}