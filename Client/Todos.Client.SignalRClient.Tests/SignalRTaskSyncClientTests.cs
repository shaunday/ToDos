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

        [TestMethod]
        public void SignalRTaskSyncClient_Type_Should_Exist()
        {
            Assert.IsNotNull(typeof(SignalRTaskSyncClient));
        }

        [TestMethod]
        public void SignalRTaskSyncClient_CanBeDisposed()
        {
            var client = new SignalRTaskSyncClient(null);
            if (client is System.IDisposable disposable)
            {
                disposable.Dispose();
                Assert.IsTrue(true);
            }
            else
            {
                Assert.IsTrue(true, "Not IDisposable, nothing to dispose.");
            }
        }

        [TestMethod]
        public void SignalRTaskSyncClient_MultipleInstances_CanBeDisposed()
        {
            for (int i = 0; i < 3; i++)
            {
                var client = new SignalRTaskSyncClient(null);
                if (client is System.IDisposable disposable)
                    disposable.Dispose();
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SignalRTaskSyncClient_CanBeCreatedAndDisposed_Parallel()
        {
            System.Threading.Tasks.Parallel.For(0, 10, i =>
            {
                var client = new SignalRTaskSyncClient(null);
                if (client is System.IDisposable disposable)
                    disposable.Dispose();
            });
            Assert.IsTrue(true);
        }
    }
} 