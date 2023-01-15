using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.AccountVMs;

namespace Vinyoxla.Service.ViewModels.TransactionVMs
{
    public class TransactionGetVM
    {
        public int Id { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public string Code { get; set; }
        public bool PaymentIsSuccessful { get; set; }
        public bool IsTopUp { get; set; }
        public int Amount { get; set; }
        public bool IsFromBalance { get; set; }


        //relation
        public AppUserGetVM AppUser { get; set; }
        public string AppUserId { get; set; }

    }
}
