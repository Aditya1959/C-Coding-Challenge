using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace OrderManagementSystem.util
{
    public class DBPropertyUtil
    {
        public static string GetConnectionString(string propertyFileName)
        {
            return ConfigurationManager.ConnectionStrings[propertyFileName].ConnectionString;
        }
    }
}
