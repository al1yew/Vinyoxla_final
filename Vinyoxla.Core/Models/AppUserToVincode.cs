using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class AppUserToVincode : BaseModel
    {
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
        public VinCode VinCode { get; set; }
        public int VinCodeId { get; set; }
    }
}
