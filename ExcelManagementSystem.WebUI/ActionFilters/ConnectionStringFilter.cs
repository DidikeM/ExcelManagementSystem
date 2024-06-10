using ExcelManagementSystem.WebUI.Helpers;
using ExcelManagementSystem.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExcelManagementSystem.WebUI.ActionFilters
{
    public class ConnectionStringFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = filterContext.ActionDescriptor.ActionName;

            if (controllerName == "ConnectionString" && actionName == "Index")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var connectionString = HttpContext.Current.Session["ConnectionString"] as string;
            var databaseName = HttpContext.Current.Session["DatabaseName"] as string;

            bool isValid = !string.IsNullOrEmpty(connectionString) &&
                           ConnectionStringHelper.IsValidConnectionStringFromDatabase(connectionString, databaseName);

            if (!isValid)
            {
                connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
                databaseName = ConfigurationManager.AppSettings["DatabaseName"];

                isValid = !string.IsNullOrEmpty(connectionString) &&
                            !string.IsNullOrEmpty(databaseName) &&
                            ConnectionStringHelper.IsValidConnectionString(connectionString);

                if (isValid)
                {
                    HttpContext.Current.Session["ConnectionString"] = connectionString;
                    HttpContext.Current.Session["DatabaseName"] = databaseName;

                    DbManager.CheckAndPrepareDatabase(connectionString, databaseName, false);
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/ConnectionString");
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}