using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodDos.Ui.Services.Mapping;
using ToDos.Ui.Services.Navigation;
using ToDos.Ui.ViewModels;
using Unity;
using Serilog;

namespace ToDos.Ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string LogFileName = "TodDos.Ui.log";
        public IUnityContainer container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFileName)
                .CreateLogger();
            Log.Information("TodDos.Ui started");
            base.OnStartup(e);

            container = new UnityContainer();

            //services
            container.RegisterSingleton<INavigationService, NavigationService>();

            // Create MapperConfiguration with your profiles
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ClientMappingProfile>();
            }, null); //todo logging

            // Create the IMapper instance
            IMapper mapper = config.CreateMapper();

            container.RegisterInstance<IMapper>(mapper);


            //ViewModels
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
