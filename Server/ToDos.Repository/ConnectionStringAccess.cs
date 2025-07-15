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
        public static string GetDbConnectionString(string dbName ="")
        {
            if (string.IsNullOrEmpty(dbName)) //dev-mode
            {
                var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, ".env.Repository");
                Env.Load(envPath);
                dbName = Environment.GetEnvironmentVariable("DB_NAME");
            }

            string server = Environment.GetEnvironmentVariable("DB_SERVER");
            string user = Environment.GetEnvironmentVariable("DB_USER");
            string pass = Environment.GetEnvironmentVariable("DB_PASS");

            string connectionString = $"Server={server};Database={dbName};User Id={user};Password={pass};TrustServerCertificate=True;";


            string connectionOverride = Environment.GetEnvironmentVariable("MSSQL_TRUSTED_CONNECTION_STRING");

            return connectionOverride;
        }
    }
}
