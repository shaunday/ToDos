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
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFileName)
                .CreateLogger();
            Log.Information("TaskSyncServer started");

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