using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class VinCode : BaseModel
    {
        public string Vin { get; set; }
        public string FileName { get; set; }
        public int PurchasedTimes { get; set; }



        //relations
        public List<AppUserToVincode> AppUserToVincodes { get; set; }
    }
}
