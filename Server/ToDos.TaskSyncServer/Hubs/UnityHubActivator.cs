using Microsoft.AspNet.SignalR.Hubs;
using Serilog;
using System;
using Unity;

namespace ToDos.TaskSyncServer.Hubs
{
    public class UnityHubActivator : IHubActivator
    {
        private readonly IUnityContainer _container;

        public UnityHubActivator(IUnityContainer container)
        {
            _container = container;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            try
            {
                var hub = (IHub)_container.Resolve(descriptor.HubType);
                Log.Logger.Information("Resolved hub {HubType} via Unity", descriptor.HubType.Name);
                return hub;
            }
            catch (Exception ex)
            {
                Log.Logger.Warning(ex, "Failed to resolve hub {HubType} via Unity, falling back to parameterless constructor", descriptor.HubType.Name);
                return (IHub)Activator.CreateInstance(descriptor.HubType);
            }
        }

    }
}
