using AutoMapper;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Windows;
using Todos.Ui.Services.Mapping;
using Todos.Client.Common.Factories;
using Todos.Client.Common.Interfaces;
using Todos.Client.MockTaskSyncClient;
using Todos.Client.SignalRClient;
using Todos.Client.UserService;
using Todos.Client.UserService.Interfaces;
using Todos.Ui.Services.Navigation;
using Todos.Ui.ViewModels;
using Unity;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui
{
    public partial class App : Application
    {
        public IUnityContainer container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            SetupLogging();

            RegisterGlobalExceptionHandlers();

            ConfigureContainer();

            ShowMainWindow();

            base.OnStartup(e);
        }

        private void SetupLogging()
        {
            Process appProcess = Process.GetCurrentProcess();
            string logFileName = LogFactory.GetLogFileName(appProcess.Id, ClientType.UiClient);
            string logFilePath = LogFactory.GetLogFilePath(logFileName);
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger();
            Log.Information($"TodDos.Client.Ui Process = {appProcess.Id} started");
        }

        private void RegisterGlobalExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            };

            DispatcherUnhandledException += (s, e) =>
            {
                Log.Fatal(e.Exception, "UI thread unhandled exception");
                e.Handled = true;
            };
        }

        private void ConfigureContainer()
        {
            container = new UnityContainer();

            // Register services
            container.RegisterSingleton<INavigationService, NavigationService>();
            container.RegisterSingleton<IUserService, MockUserService>();
            container.RegisterSingleton<ITaskSyncClient, MockTaskSyncClient>();
            container.RegisterInstance(Log.Logger);

            // Register AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ClientMappingProfile>();
            }, new SerilogLoggerFactory(Log.Logger));
            IMapper mapper = config.CreateMapper();
            container.RegisterInstance<IMapper>(mapper);

            // Register ViewModels
            container.RegisterType<MainViewModel>();
            container.RegisterType<LoginViewModel>();
            container.RegisterType<TasksViewModel>();
        }

        private void ShowMainWindow()
        {
            Process appProcess = Process.GetCurrentProcess();

            var window = new MainWindow
            {
                DataContext = container.Resolve<MainViewModel>(),
                Title = $"ToDos Client - PID: {appProcess.Id}"
            };

            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application exiting.");
            Log.CloseAndFlush(); // Properly flush and close Serilog
            base.OnExit(e);
        }
    }
}
