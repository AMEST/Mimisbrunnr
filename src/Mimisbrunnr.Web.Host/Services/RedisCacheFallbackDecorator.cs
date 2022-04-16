using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Mimisbrunnr.Web.Host.Services
{
    internal class RedisCacheFallbackDecorator : IDistributedCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheFallbackDecorator> _logger;
        private readonly TimeSpan _redisPauseTime = TimeSpan.FromMinutes(1);
        private DateTime _lastRedisFailTime = DateTime.MinValue;

        public RedisCacheFallbackDecorator(IDistributedCache distributedCache,
            ILogger<RedisCacheFallbackDecorator> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public byte[] Get(string key)
        {
            return ExecuteCatched(() => _distributedCache.Get(key.ToApplicationKey()));
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            return ExecuteCatchedAsync(() => _distributedCache.GetAsync(key.ToApplicationKey(), token));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            ExecuteCatched(() => _distributedCache.Set(key.ToApplicationKey(), value, options));
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            return ExecuteCatchedAsync(() => _distributedCache.SetAsync(key.ToApplicationKey(), value, options, token));
        }

        public void Refresh(string key)
        {
            ExecuteCatched(() => _distributedCache.Refresh(key.ToApplicationKey()));
        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            return ExecuteCatchedAsync(() => _distributedCache.RefreshAsync(key.ToApplicationKey(), token));
        }

        public void Remove(string key)
        {
            ExecuteCatched(() => _distributedCache.Remove(key.ToApplicationKey()));
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            return ExecuteCatchedAsync(() => _distributedCache.RemoveAsync(key.ToApplicationKey(), token));
        }

        private T ExecuteCatched<T>(Func<T> func) => ExecuteCatchedAsync(() => Task.Factory.StartNew(func)).Result;

        private async Task<T> ExecuteCatchedAsync<T>(Func<Task<T>> func)
        {
            if (IsRedisUnhealthy())
                return default;

            try
            {
                return await func();
            }
            catch (Exception e) when (e is RedisConnectionException || e is RedisTimeoutException)
            {
                _lastRedisFailTime = DateTime.UtcNow;
                _logger.LogError("Redis connection error. {Message}", e.Message);
                return default;
            }
        }

        private void ExecuteCatched(Action action) => ExecuteCatchedAsync(() => Task.Factory.StartNew(action)).Wait();

        private async Task ExecuteCatchedAsync(Func<Task> func)
        {
            if (IsRedisUnhealthy())
                return;
            try
            {
                await func();
            }
            catch (Exception e) when (e is RedisConnectionException || e is RedisTimeoutException)
            {
                _lastRedisFailTime = DateTime.UtcNow;
                _logger.LogError("Redis connection error. {Message}", e.Message);
            }
        }

        private bool IsRedisUnhealthy()
        {
            return _lastRedisFailTime.Add(_redisPauseTime) > DateTime.UtcNow;
        }
    }

    internal static class KeyExtensions
    {
        private const string KeyPrefix = "mimisbrunnr.";
        public static string ToApplicationKey(this string key)
        {
            return key.StartsWith(KeyPrefix)
                ? key
                : $"{KeyPrefix}{key}";
        }
    }
}