using ExcelManagementSystem.WebUI.ExcelObjects;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ExcelManagementSystem.WebUI.Services
{
    public class ExcelService
    {
        public string UploadExcelFile(HttpPostedFileBase file)
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

        public ExcelFile ReadExcelFile(string path)
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
    }
}