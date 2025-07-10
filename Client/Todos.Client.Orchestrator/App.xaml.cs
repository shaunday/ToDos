using System.Configuration;
using System.Data;
using System.Windows;
using Serilog;

namespace Todos.Client.Orchestrator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string LogFileName = "Todos.Client.Orchestrator.log";
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFileName)
                .CreateLogger();
            Log.Information("Orchestrator started");
            base.OnStartup(e);
        }
    }

}