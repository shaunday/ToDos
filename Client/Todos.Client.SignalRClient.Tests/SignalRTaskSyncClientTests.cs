using Microsoft.VisualStudio.TestTools.UnitTesting;
using Todos.Client.SignalRClient;

namespace Todos.Client.SignalRClient.Tests
{
    [TestClass]
    public class SignalRTaskSyncClientTests
    {
        [TestMethod]
        public void CanInstantiate_SignalRTaskSyncClient()
        {
            var client = new SignalRTaskSyncClient(null);
            Assert.IsNotNull(client);
        }
    }
} 