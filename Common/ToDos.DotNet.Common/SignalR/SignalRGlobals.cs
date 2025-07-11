using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDos.DotNet.Common
{
    public static class SignalRGlobals
    {
        public const string URL_String_Identifier = "SERVER_URL";

        // From server to clients (invoked via Clients.All / Clients.User etc.)
        public const string TaskAdded = "TaskAdded";
        public const string TaskUpdated = "TaskUpdated";
        public const string TaskDeleted = "TaskDeleted";
        public const string TaskLocked = "TaskLocked";
        public const string TaskUnlocked = "TaskUnlocked";

        // From clients to server (invoked via connection.InvokeAsync)
        public const string GetAllTasks = "GetAllTasks";
        public const string AddTask = "AddTask";
        public const string UpdateTask = "UpdateTask";
        public const string DeleteTask = "DeleteTask";
        public const string SetTaskCompletion = "SetTaskCompletion";
        public const string LockTask = "LockTask";
        public const string UnlockTask = "UnlockTask";
    }
}
