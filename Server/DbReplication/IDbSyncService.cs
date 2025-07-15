namespace ToDos.Server.DbReplication
{
    public interface IDbSyncService
    {
        /// <summary>
        /// Triggers a sync from master to slave for the given logical DB.
        /// </summary>
        void Sync(string logicalDbName);

        /// <summary>
        /// Triggers a sync for all known logical DBs.
        /// </summary>
        void SyncAll();
    }
} 