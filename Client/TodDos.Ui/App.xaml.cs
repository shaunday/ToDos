using AutoMapper;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TodDos.Ui.Services.Mapping;
using Todos.Client.Common.Factories;
using Todos.Client.Common.Interfaces;
using Todos.Client.MockTaskSyncClient;
using Todos.Client.SignalRClient;
using Todos.Client.TaskSyncWithOfflineQueues;
using Todos.Client.UserService;
using Todos.Client.UserService.Interfaces;
using Todos.Ui.Services; // Add this if UserConnectionService is in this namespace
using Todos.Ui.Services.Navigation;
using Todos.Ui.ViewModels;
using Todos.UserService;
using ToDos.MockAuthService;
using Unity;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui
{
    public partial class App : Application
    {
        public static IUnityContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            SetupLogging();
            RegisterGlobalExceptionHandlers();
            ConfigureContainer();

            string autoUser = null;
            string autoPass = null;
            string autologinArg = e.Args.FirstOrDefault(arg => arg.Contains("user="));
            if (autologinArg != null)
            {
                var parts = autologinArg.Split(';');
                autoUser = parts.FirstOrDefault(p => p.StartsWith("user="))?.Split('=')[1];
                autoPass = parts.FirstOrDefault(p => p.StartsWith("pass="))?.Split('=')[1];
            }
            if (string.IsNullOrEmpty(autoUser) || string.IsNullOrEmpty(autoPass))
            {
                autoUser = "defaultuser";
                autoPass = "1234";
            }
            ShowMainWindow(autoUser, autoPass);
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
            Container = new UnityContainer();

            // Register both Serilog.ILogger and ILogger for DI
            Container.RegisterInstance<Serilog.ILogger>(Log.Logger);
            Container.RegisterInstance<ILogger>(Log.Logger);

            // Register services
            Container.RegisterSingleton<INavigationService, NavigationService>();
            Container.RegisterSingleton<IUserService, MockUserService>();
            Container.RegisterSingleton<IOfflineQueueService, MemBasedOfflineQueueService>();
            
            var signalRClient = new SignalRTaskSyncClient(Log.Logger);
            Container.RegisterInstance<ITaskSyncClient>(signalRClient);
            Container.RegisterSingleton<IAuthService, MockJwtAuthService>();
            Container.RegisterSingleton<UserConnectionService>();

            // Register AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ClientMappingProfile>();
            }, new SerilogLoggerFactory(Log.Logger));
            IMapper mapper = config.CreateMapper();
            Container.RegisterInstance<IMapper>(mapper);

            // Register ViewModels
            Container.RegisterType<MainViewModel>();
            Container.RegisterType<LoginViewModel>();
            Container.RegisterType<TasksViewModel>();
            Container.RegisterType<TopPanelViewModel>();
        }

        private void ShowMainWindow(string autoUser, string autoPass)
        {
            Process appProcess = Process.GetCurrentProcess();
            var mainVm = Container.Resolve<MainViewModel>();
            var window = new MainWindow
            {
                DataContext = mainVm,
                Title = $"ToDos Client - PID: {appProcess.Id}"
            };
            window.Show();
            // Trigger auto-login after window is shown
            _ = mainVm.AutoLogin(autoUser, autoPass);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application exiting.");
            Log.CloseAndFlush(); // Properly flush and close Serilog
            base.OnExit(e);
        }
    }
}
