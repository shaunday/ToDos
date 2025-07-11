using AutoMapper;
using dotenv.net;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodDos.Ui.Services.Mapping;
using Todos.Client.Common.Factories;
using ToDos.Ui.Services.Navigation;
using ToDos.Ui.ViewModels;
using Unity;
using static Todos.Client.Common.TypesGlobal;

namespace ToDos.Ui
{
    public partial class App : Application
    {
        public IUnityContainer container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Load .env.Global (must be in the output directory)
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            //DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envPath }, probeForEnv: false));
            //try catch here ^ todo .. handling = ??


            // ---- Logging
            Process appProcess = Process.GetCurrentProcess();
            string logFileName = LogFactory.GetLogFileName(appProcess.Id, ClientType.UiClient);
            string logFilePath = LogFactory.GetLogFilePath(logFileName);
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger();
            Log.Information($"TodDos.Client.Ui Process = {appProcess.Id} started");
            base.OnStartup(e);

            // ---- DI
            container = new UnityContainer();

            // services
            container.RegisterSingleton<INavigationService, NavigationService>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ClientMappingProfile>();
            }, new SerilogLoggerFactory(Log.Logger));
            IMapper mapper = config.CreateMapper();
            container.RegisterInstance(mapper);


            //ViewModels
            container.RegisterType<MainViewModel>(); //todo singleton?
            container.RegisterType<LoginViewModel>();
            container.RegisterType<TasksViewModel>();

            var window = new MainWindow
            {
                DataContext = container.Resolve<MainViewModel>(),
                Title = $"ToDos Client - PID: {appProcess.Id}"
            };

            window.Show();
        }
    }
}
