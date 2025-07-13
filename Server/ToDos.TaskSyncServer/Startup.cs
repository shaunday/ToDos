using AutoMapper;
using Microsoft.AspNet.SignalR;
using Owin;
using Serilog;
using Serilog.Extensions.Logging;
using System.Web.Http;
using Todos.Server.MockTaskService;
using ToDos.Repository;
using ToDos.Server.Common.Interfaces;
using ToDos.TaskSyncServer.Mapping;
using ToDos.TaskSyncServer.Services;
using Unity;
using Unity.Lifetime;

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
            
            // Register database context
            container.RegisterType<TaskDbContext>(new ContainerControlledLifetimeManager());
            
            // Register repository
            container.RegisterType<ITaskRepository, TaskRepository>();
            
            // Register services - choose between real and mock
            var useMockService = System.Configuration.ConfigurationManager.AppSettings["UseMockService"] == "true";
            
            if (useMockService)
            {
                container.RegisterType<ITaskService, MockTaskService>();
                Log.Information("Using MockTaskService for development/testing");
            }
            else
            {
                container.RegisterType<ITaskService, TaskService>();
                Log.Information("Using TaskService with real database");
            }

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