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
using dotenv.net;

namespace ToDos.Ui
{
    public partial class App : Application
    {
        private const string LogFileName = "TodDos.Ui.log";
        public IUnityContainer container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Load .env.Global (must be in the output directory)
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envPath }, probeForEnv: false));
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
