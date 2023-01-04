using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.Service.ViewModels.AccountVMs
{
    public class AppUserVM
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string UserName { get; set; }
        public int Balance { get; set; }
        public bool IsAdmin { get; set; }


        //relations
        public List<AppUserToVincodeVM> AppUserToVincodes { get; set; }
    }
}
