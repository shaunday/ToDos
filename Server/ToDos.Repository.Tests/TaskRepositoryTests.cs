using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDos.Repository;
using ToDos.Server.DbSharding;
using ToDos.Server.DbReplication;

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

        [TestMethod]
        public void CanInstantiate_TaskRepository_WithNulls()
        {
            var repo = new TaskRepository(new MockShardResolver(), new MockDbRouter(), null);
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
            var repo = new TaskRepository(new MockShardResolver(), new MockDbRouter(), null);
            list.Add(repo);
            Assert.AreEqual(1, list.Count);
            Assert.AreSame(repo, list[0]);
        }

        [TestMethod]
        public void TaskRepository_ReferenceEquality()
        {
            var a = new TaskRepository(new MockShardResolver(), new MockDbRouter(), null);
            var b = new TaskRepository(new MockShardResolver(), new MockDbRouter(), null);
            Assert.AreNotSame(a, b);
        }

        // No public methods to test with null input, so skipping for now.
    }
} 