using System;
using System.Collections.Generic;

namespace Vinyoxla.Core.Models
{
    public class Event : BaseModel
    {
        public Nullable<DateTime> UpdatedAt { get; set; }
        public bool DidRefundToBalance { get; set; }

        public bool IsApiError { get; set; }
        public bool FileExists { get; set; }
        public bool IsFromApi { get; set; }
        public bool IsRenewedDueToExpire { get; set; }
        public bool IsRenewedDueToAbsence { get; set; }
        public bool ErrorWhileRenew { get; set; }
        public bool ErrorWhileReplace { get; set; }

        //relation
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
        public VinCode VinCode { get; set; }
        public int VinCodeId { get; set; }


        public List<EventMessage> EventMessages { get; set; }
    }
}
