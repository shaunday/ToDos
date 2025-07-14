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
    }
} 