using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.ViewModels.AppUserToVincodeVMs
{
    public class AppUserToVincodeVM
    {
        public int Id { get; set; }

        //relations
        public AppUserVM AppUser { get; set; }
        public string AppUserId { get; set; }
        public VinCodeGetVM VinCode { get; set; }
        public int VinCodeId { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
    }
}
