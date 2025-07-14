using AutoMapper;
using Microsoft.AspNet.SignalR;
using Owin;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Web.Http;
using ToDos.MockAuthService;
using ToDos.Repository;
using ToDos.Repository.Sharding;
using ToDos.Server.Common.Interfaces;
using ToDos.TaskSyncServer.Mapping;
using ToDos.TaskSyncServer.Services;
using Unity;
using Unity.Lifetime;
using Unity.Injection;

namespace ToDos.TaskSyncServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure SignalR with dependency injection
            var unityContainer = ConfigureUnityContainer();

            // Register SignalR with Unity container
            GlobalHost.DependencyResolver = new UnitySignalRDependencyResolver(unityContainer);

            // Configure SignalR with keep-alive and timeout settings
            // KeepAlive: Sends periodic "ping" messages to keep connections alive and detect disconnections
            // ConnectionTimeout: Maximum time to wait for a client to respond before considering it disconnected
            // These settings help with network stability, connection monitoring, and resource management
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(30);
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(60);

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
            container.RegisterType<ITaskRepository, TaskRepository>();

            container.RegisterType<ITaskService, TaskService>();

            // Register ShardResolver
            var connStrTemplate = ConnectionStringAccess.GetDbConnectionString();
            container.RegisterType<IShardResolver, DefaultShardResolver>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(connStrTemplate)
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