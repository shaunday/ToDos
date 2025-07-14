using Microsoft.VisualStudio.TestTools.UnitTesting;
using Todos.Ui;

namespace TodDos.Ui.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestMethod]
        public void MainViewModel_Type_Should_Exist()
        {
            Assert.IsNotNull(typeof(MainViewModel));
        }
    }
} 