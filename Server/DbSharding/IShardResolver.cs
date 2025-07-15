namespace ToDos.Server.DbSharding
{
    public interface IShardResolver
    {
        string GetDatabaseName(int userId);
    }
} 