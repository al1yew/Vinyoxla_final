using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.BankVMs
{
    public class TopUpVM
    {
        public string OrderId { get; set; }
        public string SessionId { get; set; }
        public string Amount { get; set; }
        public string Phone { get; set; }
    }
}
