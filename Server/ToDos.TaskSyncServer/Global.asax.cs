using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using ToDos.TaskSyncServer.Mapping;
using Serilog;

namespace ToDos.TaskSyncServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private const string LogFileName = "ToDos.TaskSyncServer.log";
        protected void Application_Start()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(LogFileName)
                .CreateLogger();
            Log.Information("TaskSyncServer started");

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
