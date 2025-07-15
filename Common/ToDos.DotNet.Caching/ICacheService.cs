using System;
using System.Collections.Generic;

namespace ToDos.DotNet.Caching
{
    public interface ICacheService<TUserId, TDto>
    {
        IEnumerable<TDto> Get(TUserId userId, string key);
        void Set(TUserId userId, string key, IEnumerable<TDto> items, TimeSpan? duration = null);
        void Invalidate(TUserId userId, string key = null);
        int GetCachedCount(TUserId userId, string key);
        void CleanupInactiveUsers(TimeSpan inactivityThreshold);
    }
} 