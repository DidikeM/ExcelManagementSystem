using ExcelManagementSystem.WebUI.ExcelObjects;
using ExcelManagementSystem.WebUI.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Xml.Linq;

namespace ExcelManagementSystem.WebUI.Services
{
    public class DbManager
    {
        public static void CheckAndPrepareDatabase(string connectionString, string databaseName, bool clearDatabase)
        {
            var sqlService = new SqlService();
            if (!SqlService.DoesDatabaseExists(connectionString, databaseName))
            {
                sqlService.CreateDatabase();
            }
            else if (clearDatabase)
            {
                sqlService.ClearDatabase();
            }

            if (!sqlService.DoesTableExist("ExcelFiles"))
            {
                var columns = new Dictionary<string, string>
                {
                    { "Name", "nvarchar(255)" }
                };
                sqlService.CreateTable("ExcelFiles", columns);
            }

            if (!sqlService.DoesTableExist("Worksheets"))
            {
                var columns = new Dictionary<string, string>
                {
                    { "ExcelFileId", "int" },
                    { "Name", "nvarchar(255)" },
                    { "TableName", "nvarchar(max)" }
                };
                sqlService.CreateTable("Worksheets", columns);
            }
        }

        public static void CheckAndCreateExcelTables(ExcelFile excelFile)
        {
            var sqlService = new SqlService();

            if (!sqlService.WhereExists("ExcelFiles", "Name", excelFile.Name))
            {
                sqlService.InsertData("ExcelFiles", new string[] { "Name" }, new string[][] { new string[] { excelFile.Name } });
            }

            var excelFileId = sqlService.Where("ExcelFiles", "Name", excelFile.Name, new string[] { "ID" })[0][0];

            foreach (var worksheet in excelFile.Worksheets)
            {
                if (!sqlService.WhereExists("Worksheets", new string[] { "ExcelFileId", "Name" }, new string[] { excelFileId, worksheet.Name }))
                {
                    sqlService.InsertData("Worksheets", new string[] { "ExcelFileId", "Name", "TableName" }, new string[][] { new string[] { excelFileId, worksheet.Name, $"{excelFile.Name}_{worksheet.Name}" } });
                    
                }

                if (!sqlService.DoesTableExist($"{excelFile.Name}_{worksheet.Name}"))
                {
                    sqlService.CreateTable($"{excelFile.Name}_{worksheet.Name}", worksheet.Data[0].ToDictionary(x => x, x => "nvarchar(MAX)"));
                }
                else
                {
                    sqlService.ClearTable($"{excelFile.Name}_{worksheet.Name}");
                }
                sqlService.InsertData($"{excelFile.Name}_{worksheet.Name}", worksheet.Data[0], worksheet.Data.Skip(1).ToArray());
            }
        }

        public static ExcelFile GetExcelFile(string excelFileName)
        {
            var sqlService = new SqlService();

            var excelFileId = sqlService.Where("ExcelFiles", "Name", excelFileName, new string[] { "ID" })[0][0];

            var worksheets = sqlService.Where("Worksheets", "ExcelFileId", excelFileId, new string[] { "Name", "TableName" });

            ExcelFile excelFile = new ExcelFile
            {
                Name = excelFileName
            };

            foreach (var worksheet in worksheets)
            {
                var worksheetData = sqlService.ReadData(worksheet[1]);
                var worksheetColumns = sqlService.GetColumns(worksheet[1]);

                excelFile.Worksheets.Add(new Worksheet
                {
                    Name = worksheet[0],
                    Data = new string[][] { worksheetColumns }.Concat(worksheetData).ToArray()
                });
            }

            return excelFile;
        }

        public static string[] GetExcelFileNames()
        {
            var sqlService = new SqlService();
            return sqlService.ReadData("ExcelFiles").Select(x => x[1]).ToArray();
        }
    }
}