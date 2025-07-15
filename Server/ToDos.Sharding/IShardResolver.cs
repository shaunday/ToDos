namespace ToDos.Sharding
{
    public interface IShardResolver
    {
        string GetDatabaseName(int userId);
    }
} 