using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace TodDos.Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IUnityContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Container = new UnityContainer();

            // Register services
            //Container.RegisterType<IMyService, MyService>();

            // Register ViewModels (using CommunityToolkit.Mvvm)
            Container.RegisterType<MainViewModel>();

            // Register Views
            Container.RegisterType<MainWindow>();

            // Resolve and show MainWindow
            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}
