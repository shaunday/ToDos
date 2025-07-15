using System.Collections.Generic;
using ToDos.DotNet.Common;

namespace Todos.Client.TaskSyncWithOfflineQueues
{
    public interface IOfflineQueueService
    {
        void Enqueue(PendingAction action);
        IEnumerable<PendingAction> GetAll();
        void Remove(PendingAction action);
        void Clear();
    }

    public class PendingAction
    {
        public string ActionType { get; set; } // "Add", "Update", "Delete"
        public TaskDTO Task { get; set; }
        public int UserId { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
} 