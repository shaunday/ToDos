using Serilog;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using Unity;

namespace Todos.Client.Orchestrator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string LogFileName = "Todos.Client.Orchestrator.log";

        public static IUnityContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFileName)
                .CreateLogger();
            Log.Information("Orchestrator started");
            Container = new UnityContainer();

            Container.RegisterInstance<Serilog.ILogger>(Log.Logger);

            base.OnStartup(e);
        }
    }

}