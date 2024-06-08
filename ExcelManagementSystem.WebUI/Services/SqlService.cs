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

        public static bool DoesDatabaseExists(string connectionString, string databaseName)
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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = $@"
                    CREATE DATABASE {databaseName}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
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

        public static void CreateTable(string connectionString, string databaseName, string tableName, Dictionary<string, string> columns)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                //{string.Join(", ", columns.Select(x => $"[{x}] nvarchar(MAX)"))}

                string sql = $@"
                    CREATE TABLE [{tableName}] (
                    ID INT PRIMARY KEY IDENTITY(1,1),
                    {string.Join(",", columns.Select(x => $"[{x.Key}] {x.Value}"))}
                    )";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    var asd = cmd.ExecuteNonQuery();
                }
            }
        }

        public static string[][] Where(string connectionString, string databaseName, string tableName, string[] columns, string[] whereColumns, string[] whereValues)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                string sql = $@"
                    SELECT {string.Join(", ", columns.Select(x => $"[{x}]"))} 
                    FROM {tableName} 
                    WHERE {string.Join(" AND ", whereColumns.Select((x, i) => $"[{x}] = @p{i}"))}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    for (int i = 0; i < whereValues.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@p{i}", whereValues[i]);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<string[]> data = new List<string[]>();

                        while (reader.Read())
                        {
                            string[] row = new string[columns.Length];

                            for (int i = 0; i < columns.Length; i++)
                            {
                                //row[i] = reader.GetString(i);
                                object columnValue = reader.GetValue(i);
                                row[i] = columnValue == null ? null : columnValue.ToString();
                            }

                            data.Add(row);
                        }

                        return data.ToArray();
                    }
                }
            }
        }

        public static string[][] Where(string connectionString, string databaseName, string tableName, string[] columns, string whereColumn, string whereValue)
        {
            return Where(connectionString, databaseName, tableName, columns, new string[] { whereColumn }, new string[] { whereValue });
        }

        public static bool WhereExists(string connectionString, string databaseName, string tableName, string[] whereColumns, string[] whereValues)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                string sql = $@"
                    SELECT COUNT(*) 
                    FROM {tableName} 
                    WHERE {string.Join(" AND ", whereColumns.Select((x, i) => $"[{x}] = @p{i}"))}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    for (int i = 0; i < whereValues.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@p{i}", whereValues[i]);
                    }

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool WhereExists(string connectionString, string databaseName, string tableName, string whereColumn, string whereValue)
        {
            return WhereExists(connectionString, databaseName, tableName, new string[] { whereColumn }, new string[] { whereValue });
        }

        public static string[] GetColumns(string connectionString, string databaseName, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                string sql = $@"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @TableName";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<string> columns = new List<string>();

                        while (reader.Read())
                        {
                            columns.Add(reader.GetString(0));
                        }

                        return columns.ToArray();
                    }
                }
            }
        }

        //public static void DoesColumnExist(string connectionString, string databaseName, string tableName, string columnName)
        //{
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        conn.ChangeDatabase(databaseName);

        //        string sql = $@"
        //            SELECT COUNT(*) 
        //            FROM INFORMATION_SCHEMA.COLUMNS 
        //            WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName";

        //        using (SqlCommand cmd = new SqlCommand(sql, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@TableName", tableName);
        //            cmd.Parameters.AddWithValue("@ColumnName", columnName);

        //            int count = (int)cmd.ExecuteScalar();
        //        }
        //    }
        //}

        public static void InsertData(string connectionString, string databaseName, string tableName, string[] columns, string[][] data)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                foreach (var row in data)
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = $@"
                            INSERT INTO [{tableName}] ({string.Join(", ", columns.Select(x => $"[{x}]"))}) 
                            VALUES ({string.Join(", ", columns.Select((x, i) => $"@p{i}"))})";

                        for (int i = 0; i < row.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@p{i}", row[i]);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            //using (SqlConnection conn = new SqlConnection(connectionString))
            //{
            //    conn.Open();
            //    conn.ChangeDatabase(databaseName);

            //    string sql = $@"
            //        INSERT INTO {tableName} ({string.Join(", ", columns.Select(x => $"[{x}]"))})
            //        VALUES ({string.Join(", ", columns.Select(x => $"@{x}"))})";

            //    using (SqlCommand cmd = new SqlCommand(sql, conn))
            //    {
            //        for (int i = 0; i < columns.Length; i++)
            //        {
            //            cmd.Parameters.Add($"@{columns[i]}", SqlDbType.NVarChar);
            //        }

            //        foreach (var row in data)
            //        {
            //            for (int i = 0; i < columns.Length; i++)
            //            {
            //                cmd.Parameters[$"@{columns[i]}"].Value = row[i];
            //            }

            //            cmd.ExecuteNonQuery();
            //        }
            //    }
            //}
        }

        public static void ClearTable(string connectionString, string databaseName, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                string sql = $@"
                    DELETE FROM [{tableName}];
                    DBCC CHECKIDENT('[{tableName}]', RESEED, 0)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string[][] ReadData(string connectionString, string databaseName, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(databaseName);

                string sql = $@"
                    SELECT * FROM [{tableName}]";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var data = new List<string[]>();
                        while (reader.Read())
                        {
                            var row = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[i] = reader.GetValue(i).ToString();
                            }
                            data.Add(row);
                        }
                        return data.ToArray();
                    }
                }
            }
        }

        //public static void DropDatabase(string connectionString, string databaseName)
        //{
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        conn.Open();

        //        string sql = $@"
        //            ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        //            DROP DATABASE {databaseName}";

        //        using (SqlCommand cmd = new SqlCommand(sql, conn))
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void DropTable(string connectionString, string databaseName, string tableName)
        //{
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        conn.ChangeDatabase(databaseName);

        //        string sql = $@"
        //            DROP TABLE {tableName}";

        //        using (SqlCommand cmd = new SqlCommand(sql, conn))
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}
    }
}