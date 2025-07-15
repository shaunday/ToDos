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

        [TestMethod]
        public void TopPanelViewModel_Type_Should_Exist()
        {
            Assert.IsNotNull(typeof(Todos.Ui.ViewModels.TopPanelViewModel));
        }

        [TestMethod]
        public void TopPanelViewModel_ConnectionStatus_PropertyChanged_RaisesEvent()
        {
            var type = typeof(Todos.Ui.ViewModels.TopPanelViewModel);
            var vm = (Todos.Ui.ViewModels.TopPanelViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
            bool raised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == "ConnectionStatus") raised = true; };
            var prop = type.GetProperty("ConnectionStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            prop.SetValue(vm, Todos.Client.Common.TypesGlobal.ConnectionStatus.Connected);
            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void TopPanelViewModel_ConnectionStatusText_PropertyChanged_RaisesEvent()
        {
            var type = typeof(Todos.Ui.ViewModels.TopPanelViewModel);
            var vm = (Todos.Ui.ViewModels.TopPanelViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
            bool raised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == "ConnectionStatusText") raised = true; };
            var prop = type.GetProperty("ConnectionStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            prop.SetValue(vm, Todos.Client.Common.TypesGlobal.ConnectionStatus.Connected);
            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void TopPanelViewModel_ConnectionStatusText_AllEnumValues()
        {
            var type = typeof(Todos.Ui.ViewModels.TopPanelViewModel);
            var vm = (Todos.Ui.ViewModels.TopPanelViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
            var prop = type.GetProperty("ConnectionStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var val in Enum.GetValues(typeof(Todos.Client.Common.TypesGlobal.ConnectionStatus)))
            {
                prop.SetValue(vm, val);
                var text = vm.ConnectionStatusText;
                Assert.IsFalse(string.IsNullOrEmpty(text));
            }
        }

        [TestMethod]
        public void TopPanelViewModel_CanSet_CurrentUser()
        {
            var type = typeof(Todos.Ui.ViewModels.TopPanelViewModel);
            var vm = (Todos.Ui.ViewModels.TopPanelViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
            var user = new Todos.Ui.Models.UserModel();
            var prop = type.GetProperty("CurrentUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            prop.SetValue(vm, user);
            Assert.AreSame(user, vm.CurrentUser);
        }

        [TestMethod]
        public void MainViewModel_TopPanelViewModel_IsSameInstance()
        {
            Assert.AreSame(typeof(MainViewModel).GetProperty("TopPanelViewModel"), typeof(MainViewModel).GetProperty("TopPanelViewModel"));
        }
    }
} 