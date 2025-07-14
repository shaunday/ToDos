namespace ToDos.Repository.Sharding
{
    public interface IShardResolver
    {
        string GetConnectionString(int userId);
    }
} 