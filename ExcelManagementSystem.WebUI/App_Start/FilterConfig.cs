using ExcelManagementSystem.WebUI.ActionFilters;
using System.Web;
using System.Web.Mvc;

namespace ExcelManagementSystem.WebUI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ConnectionStringFilter());
        }
    }
}
