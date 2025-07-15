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
            // Configure SignalR with dependency injection
            var unityContainer = ConfigureUnityContainer();

            // Register SignalR with Unity container
            GlobalHost.DependencyResolver = new UnitySignalRDependencyResolver(unityContainer);

            // Configure SignalR with keep-alive and timeout settings
            // KeepAlive: Sends periodic "ping" messages to keep connections alive and detect disconnections
            // DisconnectTimeout: Maximum time to wait for a client to respond before considering it disconnected
            // These settings help with network stability, connection monitoring, and resource management
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30); // Set DisconnectTimeout first
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10); // 10 <= 30/3
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(60); // (optional, can remain)

            app.MapSignalR();

            var config = new HttpConfiguration();
            // Attribute routing
            config.MapHttpAttributeRoutes();
            // Default route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            app.UseWebApi(config);
        }

        private IUnityContainer ConfigureUnityContainer()
        {
            var container = new UnityContainer();

            // Register logging
            container.RegisterInstance(Log.Logger);

            // Register Auth service
            container.RegisterType<IAuthService, ToDos.MockAuthService.MockAuthService>();

            // Register repository
            container.RegisterType<IReadWriteDbRouter, SuffixReadWriteDbRouter>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITaskRepository, TaskRepository>();

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
            catch
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