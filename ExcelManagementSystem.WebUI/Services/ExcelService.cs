using ExcelManagementSystem.WebUI.ExcelObjects;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ExcelManagementSystem.WebUI.Services
{
    public class ExcelService
    {
        public static string UploadExcelFile(HttpPostedFileBase file)
        {
            var uploadDirectory = HttpContext.Current.Server.MapPath("~/Uploads");

            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            var filePath = Path.Combine(uploadDirectory, file.FileName);

            file.SaveAs(filePath);

            return filePath;
        }

        public static ExcelFile ReadExcelFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            List<ExcelWorksheet> excelWorksheets = excelPackage.Workbook.Worksheets.ToList();

            ExcelFile excelFile = new ExcelFile
            {
                Name = fileInfo.Name
            };

            foreach (var excelWorksheet in excelWorksheets)
            {
                Worksheet worksheet = new Worksheet
                {
                    Name = excelWorksheet.Name
                };

                var row = excelWorksheet.Dimension.Rows;
                var column = excelWorksheet.Dimension.Columns;

                worksheet.Data = new string[row][];
                for (int i = 0; i < row; i++)
                {
                    worksheet.Data[i] = new string[column];
                    for (int j = 0; j < column; j++)
                    {
                        worksheet.Data[i][j] = excelWorksheet.Cells[i + 1, j + 1].Value.ToString();
                    }
                }
                excelFile.Worksheets.Add(worksheet);
            }

            return excelFile;
        }

        public static byte[] CreateExcelFile(ExcelFile excelFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excelPackage = new ExcelPackage();
            foreach (var worksheet in excelFile.Worksheets)
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets.Add(worksheet.Name);
                for (int i = 0; i < worksheet.Data.Length; i++)
                {
                    for (int j = 0; j < worksheet.Data[i].Length; j++)
                    {
                        excelWorksheet.Cells[i + 1, j + 1].Value = worksheet.Data[i][j];
                    }
                }
                //first row bold,
                excelWorksheet.Cells[1, 1, 1, worksheet.Data[0].Length].Style.Font.Bold = true;
            }

            return excelPackage.GetAsByteArray();
        }

    }
}