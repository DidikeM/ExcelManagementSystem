using ExcelManagementSystem.WebUI.ExcelObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ExcelManagementSystem.WebUI.Services
{
    public class DbManager
    {
        public static void CheckAndCreateDatabase(string connectionString, string databaseName)
        {
            if (!SqlService.DoesDatabaseExists(connectionString, databaseName))
            {
                SqlService.CreateDatabase(connectionString, databaseName);
            }

            if (!SqlService.DoesTableExist(connectionString, databaseName, "ExcelFiles"))
            {
                var columns = new Dictionary<string, string>
                {
                    { "Name", "nvarchar(255)" }
                };
                SqlService.CreateTable(connectionString, databaseName, "ExcelFiles", columns);
            }

            if (!SqlService.DoesTableExist(connectionString, databaseName, "Worksheets"))
            {
                var columns = new Dictionary<string, string>
                {
                    { "ExcelFileId", "int" },
                    { "Name", "nvarchar(255)" },
                    { "TableName", "nvarchar(max)" }
                };
                SqlService.CreateTable(connectionString, databaseName, "Worksheets", columns);
            }
        }

        public static void CheckAndCreateExcelTables(string connectionString, string databaseName, ExcelFile excelFile)
        {
            if (!SqlService.WhereExists(connectionString, databaseName, "ExcelFiles", "Name", excelFile.Name))
            {
                SqlService.InsertData(connectionString, databaseName, "ExcelFiles", new string[] { "Name" }, new string[][] { new string[] { excelFile.Name } });
            }

            var excelFileId = SqlService.Where(connectionString, databaseName, "ExcelFiles", new string[] { "ID" }, "Name", excelFile.Name)[0][0];

            foreach (var worksheet in excelFile.Worksheets)
            {
                if (!SqlService.WhereExists(connectionString, databaseName, "Worksheets", new string[] { "ExcelFileId", "Name" }, new string[] { excelFileId, worksheet.Name }))
                {
                    SqlService.InsertData(connectionString, databaseName, "Worksheets", new string[] { "ExcelFileId", "Name", "TableName" }, new string[][] { new string[] { excelFileId, worksheet.Name, $"{excelFile.Name}_{worksheet.Name}" } });
                    
                }

                if (!SqlService.DoesTableExist(connectionString, databaseName, $"{excelFile.Name}_{worksheet.Name}"))
                {
                    SqlService.CreateTable(connectionString, databaseName, $"{excelFile.Name}_{worksheet.Name}", worksheet.Data[0].ToDictionary(x => x, x => "nvarchar(MAX)"));
                }
                else
                {
                    SqlService.ClearTable(connectionString, databaseName, $"{excelFile.Name}_{worksheet.Name}");
                }
                SqlService.InsertData(connectionString, databaseName, $"{excelFile.Name}_{worksheet.Name}", worksheet.Data[0], worksheet.Data.Skip(1).ToArray());
            }
        }

        public static ExcelFile GetExcelFile(string connectionString, string databaseName, string excelFileName)
        {
            var excelFileId = SqlService.Where(connectionString, databaseName, "ExcelFiles", new string[] { "ID" }, "Name", excelFileName)[0][0];

            var worksheets = SqlService.Where(connectionString, databaseName, "Worksheets", new string[] { "Name", "TableName" }, "ExcelFileId", excelFileId);

            ExcelFile excelFile = new ExcelFile
            {
                Name = excelFileName
            };

            foreach (var worksheet in worksheets)
            {
                var worksheetData = SqlService.ReadData(connectionString, databaseName, worksheet[1]);
                var worksheetColumns = SqlService.GetColumns(connectionString, databaseName, worksheet[1]);

                excelFile.Worksheets.Add(new Worksheet
                {
                    Name = worksheet[0],
                    Data = new string[][] { worksheetColumns }.Concat(worksheetData).ToArray()
                });
            }

            return excelFile;
        }
    }
}