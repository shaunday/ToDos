using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDos.Repository;

namespace ToDos.Repository.Tests
{
    [TestClass]
    public class TaskRepositoryTests
    {
        [TestMethod]
        public void CanInstantiate_TaskRepository_WithNulls()
        {
            var repo = new TaskRepository(null, null);
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
            var repo = new TaskRepository(null, null);
            list.Add(repo);
            Assert.AreEqual(1, list.Count);
            Assert.AreSame(repo, list[0]);
        }

        [TestMethod]
        public void TaskRepository_ReferenceEquality()
        {
            var a = new TaskRepository(null, null);
            var b = new TaskRepository(null, null);
            Assert.AreNotSame(a, b);
        }

        // No public methods to test with null input, so skipping for now.
    }
} 