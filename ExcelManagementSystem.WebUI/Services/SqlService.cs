using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace ExcelManagementSystem.WebUI.Services
{
    public class SqlService
    {
        public static bool DoesTableExist(string connectionString, string databaseName, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);
                string sql = $@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = @TableName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool CheckDatabaseExists(string connectionString, string databaseName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = $@"
                    SELECT database_id 
                    FROM sys.databases 
                    WHERE Name = @DatabaseName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DatabaseName", databaseName);

                    object result = cmd.ExecuteScalar();

                    int databaseID = 0;

                    if (result != null)
                    {
                        int.TryParse(result.ToString(), out databaseID);
                    }

                    return databaseID > 0;
                }
            }
        }

        public static void CreateDatabase(string connectionString, string databaseName)
        {
            using(SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = $@"
                    CREATE DATABASE {databaseName}";

                using(SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}