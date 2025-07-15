namespace ToDos.Server.DbReplication
{
    public class SuffixReadWriteDbRouter : IReadWriteDbRouter
    {
        public string GetPhysicalDbName(string logicalDbName, bool isWriteOperation)
        {
            return logicalDbName; //mock for now

            //for production
            //return isWriteOperation ? $"{logicalDbName}_master" : $"{logicalDbName}_slave";
        }
    }
} 