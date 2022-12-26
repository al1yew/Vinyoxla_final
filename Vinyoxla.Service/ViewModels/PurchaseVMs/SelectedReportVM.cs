using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.PurchaseVMs
{
    public class SelectedReportVM
    {
        public int Type { get; set; }
        public int Records { get; set; }
        public string Vehicle { get; set; }
        public string Vin { get; set; }
        public int Year { get; set; }
    }
}
