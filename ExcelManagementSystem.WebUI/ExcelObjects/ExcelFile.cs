using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelManagementSystem.WebUI.ExcelObjects
{
    public class ExcelFile
    {
        public string Name { get; set; } = string.Empty;
        public List<Worksheet> Worksheets { get; set; } = new List<Worksheet>();
    }
}