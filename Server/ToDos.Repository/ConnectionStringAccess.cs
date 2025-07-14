using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDos.Repository
{
    public static class ConnectionStringAccess
    {
        public static string GetDbConnectionString()
        {
            var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Repository");
            Env.Load(envPath);

            string server = Environment.GetEnvironmentVariable("DB_SERVER");
            string db = Environment.GetEnvironmentVariable("DB_NAME");
            string user = Environment.GetEnvironmentVariable("DB_USER");
            string pass = Environment.GetEnvironmentVariable("DB_PASS");

            //// Return a template with a {0} placeholder for userId/shardId
            //string connectionString = $"Server={server};Database={db}_{{0}};User Id={user};Password={pass};TrustServerCertificate=True;";

            // just 1 db for now
            string connectionString = $"Server={server};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;";

            return connectionString;
        }
    }
}
