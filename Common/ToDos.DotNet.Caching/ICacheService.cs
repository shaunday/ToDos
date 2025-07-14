using System;
using System.Collections.Generic;

namespace ToDos.DotNet.Caching
{
    public interface ICacheService<TDto>
    {
        IEnumerable<TDto> Get(Guid userId, string key);
        void Set(Guid userId, string key, IEnumerable<TDto> items, TimeSpan? duration = null);
        void Invalidate(Guid userId, string key = null);
        int GetCachedCount(Guid userId, string key);
        void CleanupInactiveUsers(TimeSpan inactivityThreshold);
    }
} 