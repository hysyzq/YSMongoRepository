using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class Cache : ICache
    {
        private readonly ILogger<Cache> _logger;
        private const int IndividualCacheEntry = 1;
        private readonly CacheOptions _cacheOptions;
        private readonly MemoryCache _cache;
        protected MemoryCacheEntryOptions DefaultCacheEntryOptions;

        public Cache(
            IOptionsMonitor<CacheOptions> cacheOptions,
            ILogger<Cache> logger
            )
        {
            _logger = logger;
            _cacheOptions = cacheOptions.CurrentValue;
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = _cacheOptions.MaxItemCount,
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(_cacheOptions.ExpirationScanFrequencyInMs)
            });

            DefaultCacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(IndividualCacheEntry)
                .SetSlidingExpiration(TimeSpan.FromSeconds(_cacheOptions.SlidingExpirationSeconds))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheOptions.AbsoluteExpirationSeconds));
            
        }

        public bool TryGet<T>(string key, out T val)
        {
            val = default;
            if (_cacheOptions.DisableCache)
            {
                _logger.LogDebug("== Cache ==> Get: cache disabled");
                return false;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = !string.IsNullOrWhiteSpace(key) && _cache.TryGetValue(key, out val);
            sw.Stop();
            _logger.LogDebug($"== Cache ==> cache hit: {result}: {sw.Elapsed.TotalMilliseconds}ms for {key}");
            return result;
        }

        public void Set<T>(string key, T val)
        {
            if (_cacheOptions.DisableCache)
            {
                _logger.LogDebug("== Cache ==> Set: cache disabled");
                return ;
            }
            if (_cacheOptions.TtlSeconds > 0)
            {
                var cacheEntryOption = new MemoryCacheEntryOptions()
                    .SetSize(IndividualCacheEntry)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(_cacheOptions.SlidingExpirationSeconds))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheOptions.AbsoluteExpirationSeconds))
                    .AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(TimeSpan.FromSeconds(_cacheOptions.TtlSeconds)).Token));

                Set(key,val, cacheEntryOption);
            }
            else
            {
                Set(key, val, DefaultCacheEntryOptions);
            }
        }

        public void Set<T>(string key, T val, MemoryCacheEntryOptions entryOptions)
        {
            if (_cacheOptions.DisableCache)
            {
                _logger.LogDebug("== Cache ==> Set: cache disabled");
                return;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _cache.Set(key, val, entryOptions);
            sw.Stop();
            _logger.LogDebug($"== Cache ==> cache set: {sw.Elapsed.TotalMilliseconds}ms for {key}");
        }

        public void Invalidate(string key)
        {
            _cache.Remove(key);
        }

        public void Invalidate(List<string> keys)
        {
            foreach (var key in keys)
            {
                Invalidate(key);
            }
        }
    }
}
