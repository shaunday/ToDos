using System;

namespace ToDos.Server.DbReplication
{
    public class SimulatedDbSyncService : IDbSyncService
    {
        public void Sync(string logicalDbName)
        {
            Console.WriteLine($"[SYNC] Syncing master to slave for DB: {logicalDbName} at {DateTime.Now}");
        }

        public void SyncAll()
        {
            Console.WriteLine($"[SYNC] Syncing all DBs (simulation) at {DateTime.Now}");
        }
    }
} 