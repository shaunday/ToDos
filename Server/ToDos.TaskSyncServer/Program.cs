using System;
using Microsoft.Owin.Hosting;
using Serilog;
using DotNetEnv;
using ToDos.DotNet.Common;
using ToDos.DotNet.Common.SignalR;

namespace ToDos.TaskSyncServer
{
    class Program
    {
        private const string LogFileName = "ToDos.TaskSyncServer.log";
        static void Main()
        {
            var logFilePath = System.IO.Path.Combine(AppContext.BaseDirectory, LogFileName);
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger();
            Log.Information("TaskSyncServer started");

            // Add process-level unhandled exception logging
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Logger.Fatal(args.ExceptionObject as Exception, "[FATAL] Unhandled exception in AppDomain");
            };

            // Load .env.Global (must be in the output directory)
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Global");
            Env.Load(envPath);
            string url = Environment.GetEnvironmentVariable(SignalRGlobals.URL_String_Identifier);

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server running at " + url);
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
        }
    }
} 