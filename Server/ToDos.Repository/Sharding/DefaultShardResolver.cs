namespace ToDos.Repository.Sharding
{
    /// <summary>
    /// Example shard resolver. Returns a connection string for the user's own database (one db per user).
    /// Replace this logic with a real sharding strategy for production use.
    /// </summary>
    public class DefaultShardResolver : IShardResolver
    {
        private readonly string _connectionStringTemplate;

        public DefaultShardResolver(string connectionStringTemplate)
        {
            _connectionStringTemplate = connectionStringTemplate;
        }

        public string GetConnectionString(int userId)
        {
            // Each user gets their own database (not real sharding, just an example)
            return string.Format(_connectionStringTemplate, userId);
        }
    }
} 