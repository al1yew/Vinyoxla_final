using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.PurchaseVMs
{
    public class CardVM
    {
        public string CardHolder { get; set; }
        public string CardNo { get; set; }
        public Nullable<int> Month { get; set; }
        public Nullable<int> CardYear { get; set; }
        public Nullable<int> CVV { get; set; }
        public string PhoneNumber { get; set; }
        public string Vin { get; set; }

    }
}
