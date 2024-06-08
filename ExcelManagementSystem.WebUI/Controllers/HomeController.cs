using ExcelManagementSystem.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ExcelManagementSystem.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var connectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
            //var databaseName = "ExcelManagementSystem";
            //var tableName = "TestTable";

            //if (!SqlService.DoesDatabaseExists(connectionString, databaseName))
            //{
            //    SqlService.CreateDatabase(connectionString, databaseName);
            //}
            //var asd = SqlService.DoesTableExist(connectionString, databaseName, tableName);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ExcelFileReader()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExcelFileReader(HttpPostedFileBase file)
        {
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Upload File
            if (file != null && file.ContentLength > 0)
            {

                var filePath = ExcelService.UploadExcelFile(file);

                var excelFile = ExcelService.ReadExcelFile(filePath);

                //ViewBag.ExcelData = excelFile;


                var connectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
                var databaseName = "ExcelManagementSystem";
                //var tableName = "TestTable";

                DbManager.CheckAndCreateDatabase(connectionString, databaseName);

                DbManager.CheckAndCreateExcelTables(connectionString, databaseName, excelFile);

                var excelData = DbManager.GetExcelFile(connectionString, databaseName, excelFile.Name);
                ViewBag.ExcelData = excelData;

                //return ExcelService.DownloadExcelFile(excelFile);



                //var asd = SqlService.DoesTableExist(connectionString, databaseName, tableName);

                //foreach (var worksheet in excelFile.Worksheets)
                //{
                //    if (!SqlService.DoesTableExist(connectionString, databaseName, worksheet.Name))
                //    {
                //        SqlService.CreateTable(connectionString, databaseName, worksheet.Name, worksheet.Data[0].ToDictionary(x => x, x => "nvarchar(MAX)"));
                //    }
                //    else
                //    {
                //        SqlService.ClearTable(connectionString, databaseName, worksheet.Name);
                //    }

                //    SqlService.InsertData(connectionString, databaseName, worksheet.Name, worksheet.Data[0], worksheet.Data.Skip(1).ToArray());

                //    //var asd = SqlService.ReadData(connectionString, databaseName, worksheet.Name);
                //}
            }
            return View();
        }
    }
}