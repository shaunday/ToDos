using Microsoft.VisualStudio.TestTools.UnitTesting;
using Todos.Client.MockTaskSyncClient;

namespace Todos.Client.MockTaskSyncClient.Tests
{
    [TestClass]
    public class MockTaskSyncClientTests
    {
        [TestMethod]
        public void CanInstantiate_MockTaskSyncClient()
        {
            var client = new MockTaskSyncClient();
            Assert.IsNotNull(client);
        }
    }
} 