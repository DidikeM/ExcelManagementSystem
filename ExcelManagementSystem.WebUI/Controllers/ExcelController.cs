using ExcelManagementSystem.WebUI.ExcelObjects;
using ExcelManagementSystem.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExcelManagementSystem.WebUI.Controllers
{
    public class ExcelController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.ExcelFileNames = DbManager.GetExcelFileNames();
            return View();
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var filePath = ExcelService.UploadExcelFile(file);

                var excelFile = ExcelService.ReadExcelFile(filePath);

                DbManager.CheckAndCreateExcelTables(excelFile);

                return RedirectToAction("ExcelFile", new { excelFileName = excelFile.Name });
            }
            else
            {
                //ViewBag.Message = "Please select a file to upload";
                return View();
            }
        }

        [HttpGet]
        public ActionResult Download(string excelFileName)
        {
            if (string.IsNullOrEmpty(excelFileName))
            {
                return RedirectToAction("Index");
            }
            var excelData = DbManager.GetExcelFile(excelFileName);
            var excelFile = ExcelService.CreateExcelFile(excelData);
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{excelFileName}");
        }

        public ActionResult ExcelFile(string excelFileName)
        {
            if (string.IsNullOrEmpty(excelFileName))
            {
                return RedirectToAction("Index");
            }
            var excelData = DbManager.GetExcelFile(excelFileName);
            ViewBag.ExcelData = excelData;
            return View();
        }

        //[HttpPost]
        //public ActionResult ExcelFileReader(HttpPostedFileBase file)
        //{
        //    //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        //    // Upload File
        //    if (file != null && file.ContentLength > 0)
        //    {

        //        var filePath = ExcelService.UploadExcelFile(file);

        //        var excelFile = ExcelService.ReadExcelFile(filePath);

        //        var connectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
        //        var databaseName = "ExcelManagementSystem";

        //        DbManager.CheckAndCreateDatabase(connectionString, databaseName);

        //        DbManager.CheckAndCreateExcelTables(connectionString, databaseName, excelFile);

        //        var excelData = DbManager.GetExcelFile(connectionString, databaseName, excelFile.Name);
        //        ViewBag.ExcelData = excelData;

        //    }
        //    return View();
        //}

        [HttpGet, Route("Edit/{id}")]
        public ActionResult Edit(int Id, string worksheetName, string excelName)
        {
            if (string.IsNullOrEmpty(excelName) || string.IsNullOrEmpty(worksheetName) || Id == 0)
            {
                return RedirectToAction("Index");
            }
            var sqlService = new SqlService();

            ViewBag.Row = sqlService.Where($"{excelName}_{worksheetName}", "ID", Id.ToString(), null)[0];
            ViewBag.Columns = sqlService.GetColumns($"{excelName}_{worksheetName}");
            ViewBag.ExcelName = excelName;
            return View();
        }

        [HttpPost, Route("Edit/{id}")]
        public ActionResult Edit(int Id, string worksheetName, string excelName, FormCollection formCollection)
        {
            if (string.IsNullOrEmpty(excelName) || string.IsNullOrEmpty(worksheetName) || Id == 0)
            {
                return RedirectToAction("Index");
            }

            var sqlService = new SqlService();

            var columns = sqlService.GetColumns($"{excelName}_{worksheetName}");

            var updateColumns = columns.Where(column => column != "ID").ToArray();

            var values = new List<string>();
            foreach (var column in updateColumns)
            {
                values.Add(formCollection[column]);
            }

            sqlService.UpdateData($"{excelName}_{worksheetName}", Id, columns, values);

            return RedirectToAction("ExcelFile", new { excelFileName = excelName });
        }

        [HttpGet, Route("Delete/{id}")]
        public ActionResult Delete(int Id, string worksheetName, string excelName)
        {
            if (string.IsNullOrEmpty(excelName) || string.IsNullOrEmpty(worksheetName) || Id == 0)
            {
                return RedirectToAction("Index");
            }
            var sqlService = new SqlService();

            sqlService.DeleteData($"{excelName}_{worksheetName}", Id);

            return RedirectToAction("ExcelFile", new { excelFileName = excelName });
        }
    }
}