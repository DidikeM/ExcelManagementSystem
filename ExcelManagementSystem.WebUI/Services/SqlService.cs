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
        private readonly string _connectionString;
        private readonly string _databaseName;

        public SqlService()
        {
            _connectionString = HttpContext.Current.Session["ConnectionString"].ToString();
            _databaseName = HttpContext.Current.Session["DatabaseName"].ToString();
        }

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

        public void CreateDatabase()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string sql = $@"
                    CREATE DATABASE {_databaseName}";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ClearDatabase()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

                string sql = $@"
                    EXEC sp_MSforeachtable 'DISABLE TRIGGER ALL ON ?';
                    EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
                    EXEC sp_MSforeachtable 'DELETE FROM ?';
                    EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
                    EXEC sp_MSforeachtable 'ENABLE TRIGGER ALL ON ?'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool DoesTableExist(string tableName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);
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

        public void CreateTable(string tableName, Dictionary<string, string> columns)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

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

        public string[][] Where(string tableName, string[] whereColumns, string[] whereValues, string[] columns = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

                string sql;

                if (columns == null)
                {
                    sql = $@"
                    SELECT * 
                    FROM [{tableName}] 
                    WHERE {string.Join(" AND ", whereColumns.Select((x, i) => $"[{x}] = @p{i}"))}";
                }
                else
                {
                    sql = $@"
                    SELECT {string.Join(", ", columns.Select(x => $"[{x}]"))} 
                    FROM [{tableName}] 
                    WHERE {string.Join(" AND ", whereColumns.Select((x, i) => $"[{x}] = @p{i}"))}";
                }
                

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
                            string[] row = new string[reader.FieldCount];

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                //row[i] = reader.GetString(i);
                                object columnValue = reader.GetValue(i);
                                row[i] = columnValue?.ToString();
                            }

                            data.Add(row);
                        }

                        return data.ToArray();
                    }
                }
            }
        }

        public string[][] Where(string tableName, string whereColumn, string whereValue, string[] columns = null)
        {
            return Where(tableName, new string[] { whereColumn }, new string[] { whereValue }, columns);
        }


        public bool WhereExists(string tableName, string[] whereColumns, string[] whereValues)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

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

        public bool WhereExists(string tableName, string whereColumn, string whereValue)
        {
            return WhereExists(tableName, new string[] { whereColumn }, new string[] { whereValue });
        }

        public string[] GetColumns(string tableName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

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

        public void InsertData(string tableName, string[] columns, string[][] data)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

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
        }

        public void ClearTable(string tableName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

                string sql = $@"
                    DELETE FROM [{tableName}];
                    DBCC CHECKIDENT('[{tableName}]', RESEED, 0)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public string[][] ReadData(string tableName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

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

        public void UpdateData(string tableName, int Id, string[] columns, List<string> values)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

                var updateColumns = columns.Where(column => column != "ID").ToArray();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $@"
                        UPDATE [{tableName}] 
                        SET {string.Join(", ", updateColumns.Select((x, i) => $"[{x}] = @p{i}"))}
                        WHERE ID = @Id";

                    for (int i = 0; i < values.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@p{i}", values[i]);
                    }
                    cmd.Parameters.AddWithValue("@Id", Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteData(string tableName, int Id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(_databaseName);

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $@"
                        DELETE FROM [{tableName}] 
                        WHERE ID = @Id";

                    cmd.Parameters.AddWithValue("@Id", Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}