using System;
using System.Collections.Generic;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;
using Vinyoxla.Service.ViewModels.EventVMs;
using Vinyoxla.Service.ViewModels.TransactionVMs;

namespace Vinyoxla.Service.ViewModels.AccountVMs
{
    public class AppUserGetVM
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string UserName { get; set; }
        public int Balance { get; set; }
        public bool IsAdmin { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }


        //relations
        public List<AppUserToVincodeVM> AppUserToVincodes { get; set; }
        public List<TransactionGetVM> Transactions { get; set; }
        public List<EventGetVM> Events { get; set; }
    }
}
