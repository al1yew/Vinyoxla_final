using System;
using System.Collections.Generic;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;
using Vinyoxla.Service.ViewModels.EventVMs;

namespace Vinyoxla.Service.ViewModels.VinCodeVMs
{
    public class VinCodeGetVM
    {
        public int Id { get; set; }
        public string Vin { get; set; }
        public string FileName { get; set; }
        public int PurchasedTimes { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }


        //relations
        public List<EventGetVM> Events { get; set; }
        public List<AppUserToVincodeVM> AppUserToVincodes { get; set; }
    }
}
