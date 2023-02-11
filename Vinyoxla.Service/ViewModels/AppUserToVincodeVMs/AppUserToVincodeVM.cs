using System;
using Vinyoxla.Service.ViewModels.UserVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.ViewModels.AppUserToVincodeVMs
{
    public class AppUserToVincodeVM
    {
        public int Id { get; set; }

        //relations
        public AppUserGetVM AppUser { get; set; }
        public string AppUserId { get; set; }
        public VinCodeGetVM VinCode { get; set; }
        public int VinCodeId { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
    }
}
