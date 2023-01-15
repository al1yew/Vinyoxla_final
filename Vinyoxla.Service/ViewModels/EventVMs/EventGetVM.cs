using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.EventMessageVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.ViewModels.EventVMs
{
    public class EventGetVM
    {
        public int Id { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<DateTime> UpdatedAt { get; set; }
        public bool DidRefundToBalance { get; set; }

        public bool IsApiError { get; set; }
        public bool FileExists { get; set; }
        public bool IsFromApi { get; set; }
        public bool IsRenewedDueToExpire { get; set; }
        public bool IsRenewedDueToAbsence { get; set; }
        public bool ErrorWhileRenew { get; set; }
        public bool ErrorWhileReplace { get; set; }

        public string Vin { get; set; }

        //relation
        public AppUserGetVM AppUser { get; set; }
        public string AppUserId { get; set; }
      
        public List<EventMessageGetVM> EventMessages { get; set; }
    }
}
