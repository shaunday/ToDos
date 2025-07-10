using System;
using Microsoft.Owin.Hosting;
using Serilog;
using dotenv.net;

namespace ToDos.TaskSyncServer
{
    class Program
    {
        private const string LogFileName = "ToDos.TaskSyncServer.log";
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFileName)
                .CreateLogger();
            Log.Information("TaskSyncServer started");

            // Load .env.Global (must be in the output directory)
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envPath }, probeForEnv: false));
            string url = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5000";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server running at " + url);
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
        }
    }
} 