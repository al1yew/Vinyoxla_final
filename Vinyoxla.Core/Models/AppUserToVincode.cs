using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class AppUserToVincode : BaseModel
    {
        public bool DidRefundToBalance { get; set; }
        public bool IsApiError { get; set; }
        public bool IsFromApi { get; set; }
        public bool IsRenewedDueToExpire { get; set; }
        public bool IsRenewedDueToAbsence { get; set; }

        //relations
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
        public VinCode VinCode { get; set; }
        public int VinCodeId { get; set; }
    }
}
