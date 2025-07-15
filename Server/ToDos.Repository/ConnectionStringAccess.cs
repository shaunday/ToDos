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
        public static string GetDbConnectionString(string dbName)
        {
            string server = Environment.GetEnvironmentVariable("DB_SERVER");
            string user = Environment.GetEnvironmentVariable("DB_USER");
            string pass = Environment.GetEnvironmentVariable("DB_PASS");

            string connectionString = $"Server={server};Database={dbName};User Id={user};Password={pass};TrustServerCertificate=True;";
            return connectionString;
        }
    }
}
