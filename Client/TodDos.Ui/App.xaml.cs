using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ToDos.Ui.Services.Navigation;
using ToDos.Ui.ViewModels;
using Unity;

namespace ToDos.Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IUnityContainer container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            container = new UnityContainer();
            container.RegisterSingleton<INavigationService, NavigationService>();

            container.RegisterType<MainViewModel>();
            container.RegisterType<LoginViewModel>();
            container.RegisterType<TasksViewModel>();

            var window = new MainWindow
            {
                DataContext = container.Resolve<MainViewModel>()
            };

            window.Show();
        }
    }
}
