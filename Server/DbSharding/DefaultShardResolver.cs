using System;

namespace ToDos.Server.DbSharding
{
    /// <summary>
    /// Example shard resolver. Returns a database name for the user's own database (one db per user).
    /// Replace this logic with a real sharding strategy for production use.
    /// </summary>
    public class DefaultShardResolver : IShardResolver
    {
        public DefaultShardResolver() { }

        public string GetDatabaseName(int userId)
        {
            var dbNameTemplate = Environment.GetEnvironmentVariable("DB_NAME") + "_{0}";
            return string.Format(dbNameTemplate, userId);
        }
    }
} 