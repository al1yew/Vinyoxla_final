using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.Service.ViewModels.VinCodeVMs
{
    public class VinCodeVM
    {
        public int Id { get; set; }
        public string Vin { get; set; }
        public string FileName { get; set; }
        public int PurchasedTimes { get; set; }


        //relations
        public List<AppUserToVincodeVM> AppUserToVincodes { get; set; }
    }
}
