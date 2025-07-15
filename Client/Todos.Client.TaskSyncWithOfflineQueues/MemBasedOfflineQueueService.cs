using System.Collections.Generic;
using System.Linq;
using ToDos.DotNet.Common;

namespace Todos.Client.TaskSyncWithOfflineQueues
{
    // For production, implement IOfflineQueueService with actual persistent storage (e.g., file or SQLite), not just in-memory.
    public class MemBasedOfflineQueueService : IOfflineQueueService
    {
        private readonly List<PendingAction> _queue = new List<PendingAction>();
        private readonly object _lock = new object();

        public void Enqueue(PendingAction action)
        {
            lock (_lock)
            {
                _queue.Add(action);
            }
        }

        public IEnumerable<PendingAction> GetAll()
        {
            lock (_lock)
            {
                return _queue.ToList();
            }
        }

        public void Remove(PendingAction action)
        {
            lock (_lock)
            {
                _queue.Remove(action);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }
    }
} 