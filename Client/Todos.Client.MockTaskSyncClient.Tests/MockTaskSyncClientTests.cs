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

        [TestMethod]
        public void MockTaskSyncClient_Type_Should_Exist()
        {
            Assert.IsNotNull(typeof(MockTaskSyncClient));
        }

        [TestMethod]
        public void MockTaskSyncClient_CanBeAddedToList()
        {
            var list = new System.Collections.Generic.List<MockTaskSyncClient>();
            var client = new MockTaskSyncClient();
            list.Add(client);
            Assert.AreEqual(1, list.Count);
            Assert.AreSame(client, list[0]);
        }

        [TestMethod]
        public void MockTaskSyncClient_ReferenceEquality()
        {
            var a = new MockTaskSyncClient();
            var b = new MockTaskSyncClient();
            Assert.AreNotSame(a, b);
        }

        [TestMethod]
        public void MockTaskSyncClient_CanBeAddedToHashSet()
        {
            var set = new System.Collections.Generic.HashSet<MockTaskSyncClient>();
            var client = new MockTaskSyncClient();
            set.Add(client);
            Assert.IsTrue(set.Contains(client));
        }
    }
} 