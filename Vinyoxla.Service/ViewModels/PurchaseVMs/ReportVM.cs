using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.PurchaseVMs
{
    public class ReportVM
    {
        public int Ur_Id { get; set; }
        public int Id { get; set; }
        public string Report { get; set; }
        public string Report_Hash { get; set; }
        public string Type { get; set; }
        public string Vin { get; set; }
    }
}
