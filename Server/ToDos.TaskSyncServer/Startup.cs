using AutoMapper;
using Microsoft.AspNet.SignalR;
using Owin;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Web.Http;
using ToDos.MockAuthService;
using ToDos.Repository;
using ToDos.Server.DbSharding;
using ToDos.Server.Common.Interfaces;
using ToDos.TaskSyncServer.Mapping;
using ToDos.TaskSyncServer.Services;
using Unity;
using Unity.Lifetime;
using Unity.Injection;
using System.Data.Entity;
using System.Threading;
using ToDos.Server.DbReplication;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace ToDos.TaskSyncServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // ThreadPool tuning for concurrency 
            ThreadPool.GetMinThreads(out int oldMinWorker, out int oldMinIOC);
            Log.Logger.Information("Default ThreadPool min worker threads: {MinWorker}, min IO threads: {MinIOC}", oldMinWorker, oldMinIOC);
            ThreadPool.SetMinThreads(100, oldMinIOC); // Set min worker threads to 100
            ThreadPool.GetMinThreads(out int newMinWorker, out int newMinIOC);
            Log.Logger.Information("ThreadPool min worker threads changed to: {MinWorker}, min IO threads: {MinIOC}", newMinWorker, newMinIOC);


            // For sharding: ensures that if a per-shard database does not exist, it will be auto-created
            // by Entity Framework Code First when first accessed. Remove or change for production if you want manual DB control.
            Database.SetInitializer(new CreateDatabaseIfNotExists<TaskDbContext>());

            // 1. Configure Unity container and register all dependencies
            var unityContainer = ConfigureUnityContainer();

            // 2. Set GlobalHost.DependencyResolver
            GlobalHost.DependencyResolver = new UnitySignalRDependencyResolver(unityContainer);

            // 3. Set GlobalHost.Configuration properties
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30); // Set DisconnectTimeout first
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10); // 10 <= 30/3
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(60); // (optional, can remain)

            // 4. Map SignalR
            var hubConfig = new HubConfiguration
            {
                EnableDetailedErrors = true
            };
            app.MapSignalR(hubConfig);

            // Enable SignalR trace logging for diagnostics
            // GlobalHost.TraceManager.Switch.Level = SourceLevels.All;

            // Enable SignalR detailed errors (per-hub)
            // var hubConfig = new HubConfiguration
            // {
            //     EnableDetailedErrors = true
            // };

            // Add global error handler for OWIN pipeline
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "[GLOBAL] Unhandled exception in OWIN pipeline");
                    throw;
                }
            });
        }

        private IUnityContainer ConfigureUnityContainer()
        {
            var container = new UnityContainer();

            // Register logging
            container.RegisterInstance(Log.Logger);
            container.RegisterInstance<ILogger>(Log.Logger); // Register ILogger interface as singleton

            // Register Auth service
            container.RegisterType<IAuthService, ToDos.MockAuthService.MockAuthService>();

            // Register repository
            container.RegisterType<IReadWriteDbRouter, SuffixReadWriteDbRouter>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITaskRepository, TaskRepository>();
            container.RegisterType<IDbSyncService, SimulatedDbSyncService>();

            container.RegisterType<ITaskService, TaskService>();

            // Register ShardResolver
            container.RegisterType<IShardResolver, DefaultShardResolver>(
                new ContainerControlledLifetimeManager()
            );

            // Register AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ServerMappingProfile>();
            }, new SerilogLoggerFactory(Log.Logger));
            IMapper mapper = config.CreateMapper();
            container.RegisterInstance<IMapper>(mapper);

            return container;
        }
    }

    // Unity SignalR dependency resolver
    public class UnitySignalRDependencyResolver : DefaultDependencyResolver
    {
        private readonly IUnityContainer _container;

        public UnitySignalRDependencyResolver(IUnityContainer container)
        {
            _container = container;
        }

        public override object GetService(System.Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (Exception ex)
            {
                return base.GetService(serviceType);
            }
        }

        public override System.Collections.Generic.IEnumerable<object> GetServices(System.Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch
            {
                return base.GetServices(serviceType);
            }
        }
    }
}