using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Serilog;

namespace ToDos.DotNet.Caching
{
    public class MemoryCacheService<TUserId, TDto> : ICacheService<TUserId, TDto>
    {
        private readonly MemoryCache _cache;
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly Dictionary<TUserId, DateTime> _lastAccess = new Dictionary<TUserId, DateTime>();
        private readonly TimeSpan _defaultDuration = TimeSpan.FromMinutes(10);

        public MemoryCacheService(string cacheName, ILogger logger)
        {
            _cache = new MemoryCache(cacheName);
            _logger = logger ?? Log.Logger;
        }

        private string GetCacheKey(TUserId userId, string key) => $"{userId}_{key}";

        public IEnumerable<TDto> Get(TUserId userId, string key)
        {
            var cacheKey = GetCacheKey(userId, key);
            var result = _cache.Get(cacheKey) as IEnumerable<TDto>;
            lock (_lock)
            {
                _lastAccess[userId] = DateTime.UtcNow;
            }
            return result;
        }

        public void Set(TUserId userId, string key, IEnumerable<TDto> items, TimeSpan? duration = null)
        {
            var cacheKey = GetCacheKey(userId, key);
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(duration ?? _defaultDuration) };
            _cache.Set(cacheKey, items, policy);
            lock (_lock)
            {
                _lastAccess[userId] = DateTime.UtcNow;
            }
            _logger.Information("Set cache for {CacheKey}", cacheKey);
        }

        public void Invalidate(TUserId userId, string key = null)
        {
            lock (_lock)
            {
                if (key == null)
                {
                    var prefix = $"{userId}_";
                    foreach (var k in _cache.Where(x => x.Key.StartsWith(prefix)).Select(x => x.Key).ToList())
                        _cache.Remove(k);
                }
                else
                {
                    _cache.Remove(GetCacheKey(userId, key));
                }
                _lastAccess.Remove(userId);
            }
            _logger.Information("Invalidated cache for user {UserId}, key {Key}", userId, key);
        }

        public int GetCachedCount(TUserId userId, string key)
        {
            var cacheKey = GetCacheKey(userId, key);
            var items = _cache.Get(cacheKey) as IEnumerable<TDto>;
            return items?.Count() ?? 0;
        }

        public void CleanupInactiveUsers(TimeSpan inactivityThreshold)
        {
            var cutoff = DateTime.UtcNow - inactivityThreshold;
            lock (_lock)
            {
                var toRemove = _lastAccess.Where(x => x.Value < cutoff).Select(x => x.Key).ToList();
                foreach (var userId in toRemove)
                {
                    Invalidate(userId);
                }
            }
            _logger.Information("Cleaned up inactive users older than {InactivityThreshold}", inactivityThreshold);
        }
    }
} 