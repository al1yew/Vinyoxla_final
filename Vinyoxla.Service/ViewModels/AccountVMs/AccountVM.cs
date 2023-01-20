using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.Service.ViewModels.AccountVMs
{
    public class AccountVM
    {
        public PaginationList<AppUserToVincodeVM> AppUserToVincodes { get; set; }
        public int Balance { get; set; }
    }
}
