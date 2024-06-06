using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelManagementSystem.WebUI.ExcelObjects
{
    public class Worksheet
    {
        public string Name { get; set; } = string.Empty;
        public string[][] Data { get; set; }
    }
}