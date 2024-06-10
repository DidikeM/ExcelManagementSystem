using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ExcelManagementSystem.WebUI.Helpers
{
    public class ConnectionStringHelper
    {

        public static bool IsValidConnectionStringFromDatabase(string connectionString, string databaseName)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = databaseName
                };
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidConnectionString(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}