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
            var connectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
            var databaseName = "ExcelManagementSystemm";
            var tableName = "TestTable";

            if (!SqlService.CheckDatabaseExists(connectionString, databaseName))
            {
                SqlService.CreateDatabase(connectionString, databaseName);
            }
            var asd = SqlService.DoesTableExist(connectionString, databaseName, tableName);
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
                ExcelService excelService = new ExcelService();

                var filePath = excelService.UploadExcelFile(file);

                var excelFile = excelService.ReadExcelFile(filePath);

                ViewBag.ExcelData = excelFile;
            }
            return View();
        }
    }
}