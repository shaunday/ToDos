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

            //Server=localhost;Database=master;Trusted_Connection=True;
            string connectionString = $"Server={server};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;";

            return connectionString;
        }
    }
}
