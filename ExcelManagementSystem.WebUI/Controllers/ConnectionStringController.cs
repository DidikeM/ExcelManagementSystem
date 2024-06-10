using ExcelManagementSystem.WebUI.Helpers;
using ExcelManagementSystem.WebUI.Services;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExcelManagementSystem.WebUI.Controllers
{
    public class ConnectionStringController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string connectionString, string databaseName, bool continueDatabase = false, bool clearDatabase = false)
        {
            if (!ConnectionStringHelper.IsValidConnectionString(connectionString))
            {
                ViewBag.ErrorMessage = "Invalid ConnectionString.";
            }

            if (SqlService.DoesDatabaseExists(connectionString, databaseName))
            {
                if (!continueDatabase)
                {
                    ViewBag.DatabaseIsExist = true;
                    ViewBag.ErrorMessage = "Database Already Exists.";
                    return View();
                }
            }

            HttpContext.Session["ConnectionString"] = connectionString;
            HttpContext.Session["DatabaseName"] = databaseName;

            DbManager.CheckAndPrepareDatabase(connectionString, databaseName, clearDatabase);

            return RedirectToAction("Index", "Home");
        }
    }
}