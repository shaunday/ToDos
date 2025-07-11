using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todos.Client.Common
{
    public class TypesGlobal
    {
        public enum ClientType { UiClient, UiSimulator }

        public enum ConnectionStatus
        {
            Connecting,
            Connected,
            Reconnecting,
            Disconnected,
            Failed
        }

    }
}
