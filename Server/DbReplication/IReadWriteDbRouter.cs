namespace ToDos.Server.DbReplication
{
    public interface IReadWriteDbRouter
    {
        string GetPhysicalDbName(string logicalDbName, bool isWriteOperation);
    }
} 