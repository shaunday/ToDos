namespace ToDos.Server.DbReplication
{
    public class SuffixReadWriteDbRouter : IReadWriteDbRouter
    {
        public string GetPhysicalDbName(string logicalDbName, bool isWriteOperation)
        {
            return isWriteOperation ? $"{logicalDbName}_master" : $"{logicalDbName}_slave";
        }
    }
} 