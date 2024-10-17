using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OrderManagementSystem.util
{
    public static class DBConnUtil
    {
        public static SqlConnection GetDBConn()
        {
            // Build configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration config = builder.Build();

            // Get the connection string
            string connectionString = config.GetConnectionString("DefaultConnection");

            // Create and return the SqlConnection
            return new SqlConnection(connectionString);
        }
    }
}
