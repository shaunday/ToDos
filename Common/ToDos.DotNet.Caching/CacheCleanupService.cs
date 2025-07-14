using System;
using System.Threading;

namespace ToDos.DotNet.Caching
{
    public class CacheCleanupService<TDto> : IDisposable
    {
        private readonly ICacheService<TDto> _cacheService;
        private readonly Timer _timer;
        private readonly TimeSpan _interval;
        private readonly TimeSpan _inactivityThreshold;

        public CacheCleanupService(ICacheService<TDto> cacheService, TimeSpan interval, TimeSpan inactivityThreshold)
        {
            _cacheService = cacheService;
            _interval = interval;
            _inactivityThreshold = inactivityThreshold;
            _timer = new Timer(Cleanup, null, _interval, _interval);
        }

        private void Cleanup(object state)
        {
            _cacheService.CleanupInactiveUsers(_inactivityThreshold);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
} 