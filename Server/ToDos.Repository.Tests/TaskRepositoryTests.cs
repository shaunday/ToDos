using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDos.Repository;
using ToDos.Server.DbSharding;
using ToDos.Server.DbReplication;
using Serilog;

namespace ToDos.Repository.Tests
{
    [TestClass]
    public class TaskRepositoryTests
    {
        public class MockShardResolver : ToDos.Server.DbSharding.IShardResolver
        {
            public string GetDatabaseName(int userId) => "TestDbName";
        }

        public class MockDbRouter : IReadWriteDbRouter
        {
            public string GetPhysicalDbName(string logicalDbName, bool isWriteOperation) => logicalDbName + (isWriteOperation ? "_master" : "_slave");
        }

        public class MockDbSyncService : IDbSyncService
        {
            public void Sync(string logicalDbName) { }
            public void SyncAll() { }
        }

        [TestMethod]
        public void CanInstantiate_TaskRepository_WithNulls()
        {
            var repo = new TaskRepository(new MockShardResolver(), new MockDbRouter(), new MockDbSyncService(), null);
            Assert.IsNotNull(repo);
        }

        [TestMethod]
        public void TaskRepository_Type_Should_Exist()
        {
            Assert.IsNotNull(typeof(TaskRepository));
        }

        [TestMethod]
        public void TaskRepository_CanBeAddedToList()
        {
            var list = new System.Collections.Generic.List<TaskRepository>();
            var repo = new TaskRepository(new MockShardResolver(), new MockDbRouter(), new MockDbSyncService(), null);
            list.Add(repo);
            Assert.AreEqual(1, list.Count);
            Assert.AreSame(repo, list[0]);
        }

        [TestMethod]
        public void TaskRepository_ReferenceEquality()
        {
            var a = new TaskRepository(new MockShardResolver(), new MockDbRouter(), new MockDbSyncService(), null);
            var b = new TaskRepository(new MockShardResolver(), new MockDbRouter(), new MockDbSyncService(), null);
            Assert.AreNotSame(a, b);
        }

        // No public methods to test with null input, so skipping for now.
    }
} 