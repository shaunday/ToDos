using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using Serilog;
using Serilog.Extensions.Logging;
using Sprache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading;
using ToDos.MockAuthService;
using ToDos.Repository;
using ToDos.Server.Common.Interfaces;
using ToDos.Server.DbReplication;
using ToDos.Server.DbSharding;
using ToDos.TaskSyncServer.Hubs;
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
            TuneThreadPool();
            Database.SetInitializer(new CreateDatabaseIfNotExists<TaskDbContext>());

            IUnityContainer container = ConfigureUnityContainer();

            var resolver = new HybridSignalRResolver(container);
            GlobalHost.DependencyResolver = resolver;

            var hubConfig = new HubConfiguration
            {
                EnableDetailedErrors = true,
                Resolver = resolver
            };

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

            app.MapSignalR("/signalr", hubConfig);
        }

        private void TuneThreadPool()
        {
            ThreadPool.GetMinThreads(out int oldMinWorker, out int oldMinIOC);
            ThreadPool.SetMinThreads(100, oldMinIOC);
            ThreadPool.GetMinThreads(out int newMinWorker, out int newMinIOC);

            Log.Logger.Information("ThreadPool min worker threads: {Old} → {New}", oldMinWorker, newMinWorker);
        }

        private IUnityContainer ConfigureUnityContainer()
        {
            var container = new UnityContainer();

            container.RegisterInstance<ILogger>(Log.Logger);

            container.RegisterSingleton<IAuthService, MockAuthService.MockAuthService>();
            container.RegisterType<IReadWriteDbRouter, SuffixReadWriteDbRouter>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITaskRepository, TaskRepository>();
            container.RegisterType<IDbSyncService, SimulatedDbSyncService>();
            container.RegisterType<IShardResolver, DefaultShardResolver>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITaskService, TaskService>();

            // Mapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ServerMappingProfile>();
            }, new SerilogLoggerFactory(Log.Logger));

            container.RegisterInstance<IMapper>(config.CreateMapper());
            container.RegisterType<IHub, TaskHub>("TaskHub");

            return container;
        }
    }

    // Hybrid fallback resolver
    public class HybridSignalRResolver : DefaultDependencyResolver
    {
        private readonly IUnityContainer _container;

        public HybridSignalRResolver(IUnityContainer container)
        {
            _container = container;
            this.Register(typeof(IHubActivator), () => new UnityHubActivator(_container));
        }

        public override object GetService(Type serviceType)
        {
            if (_container.IsRegistered(serviceType))
            {
                try
                {
                    var resolved = _container.Resolve(serviceType);
                    return resolved;
                }
                catch (ResolutionFailedException ex)
                {
                    Log.Logger.Warning(ex, "Failed to resolve {0} from Unity", serviceType.Name);
                }
            }

            return base.GetService(serviceType);
        }
    }
}
