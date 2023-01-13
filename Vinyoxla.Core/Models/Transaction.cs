using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class Transaction : BaseModel
    {
        public string Code { get; set; }
        public bool PaymentIsSuccessful { get; set; }
        public bool IsTopUp { get; set; }
        public int Amount { get; set; }
        public bool IsFromBalance { get; set; }


        //relation
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
    }
}
