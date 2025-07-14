using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ToDos.DotNet.Caching;

namespace ToDos.DotNet.Caching.Tests
{
    [TestClass]
    public class MemoryCacheServiceTests
    {
        private MemoryCacheService<string> _cache;
        private List<string> _log;

        [TestInitialize]
        public void Setup()
        {
            _log = new List<string>();
            _cache = new MemoryCacheService<string>("TestCache", msg => _log.Add(msg));
        }

        [TestMethod]
        public void SetAndGet_ReturnsCachedItems()
        {
            var userId = Guid.NewGuid();
            var key = "test";
            var items = new[] { "a", "b", "c" };
            _cache.Set(userId, key, items);
            var result = _cache.Get(userId, key);
            CollectionAssert.AreEqual(items, result.ToArray());
        }

        [TestMethod]
        public void Invalidate_RemovesCachedItems()
        {
            var userId = Guid.NewGuid();
            var key = "test";
            _cache.Set(userId, key, new[] { "x" });
            _cache.Invalidate(userId, key);
            var result = _cache.Get(userId, key);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void CleanupInactiveUsers_RemovesOldEntries()
        {
            var userId = Guid.NewGuid();
            _cache.Set(userId, "test", new[] { "1" });
            // Simulate inactivity by manipulating _lastAccess (reflection for test only)
            var lastAccessField = typeof(MemoryCacheService<string>).GetField("_lastAccess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lastAccess = (Dictionary<Guid, DateTime>)lastAccessField.GetValue(_cache);
            lastAccess[userId] = DateTime.UtcNow.AddHours(-2);
            _cache.CleanupInactiveUsers(TimeSpan.FromHours(1));
            var result = _cache.Get(userId, "test");
            Assert.IsNull(result);
        }
    }
}
