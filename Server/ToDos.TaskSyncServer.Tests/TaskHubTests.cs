using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDos.TaskSyncServer.Hubs;

namespace ToDos.TaskSyncServer.Tests
{
    [TestClass]
    public class TaskHubTests
    {
        [TestMethod]
        public void CanInstantiate_TaskHub_WithNulls()
        {
            var hub = new TaskHub(null, null);
            Assert.IsNotNull(hub);
        }
    }
} 