using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDos.TaskSyncServer.Hubs;

namespace ToDos.TaskSyncServer.Tests
{
    [TestClass]
    public class TaskHubTests
    {
        [TestMethod]
        public void TaskHub_Type_Should_Exist()
        {
            Assert.IsNotNull(typeof(TaskHub));
        }
    }
} 