using System;
using Vinyoxla.Service.ViewModels.UserVMs;

namespace Vinyoxla.Service.ViewModels.TransactionVMs
{
    public class TransactionGetVM
    {
        public int Id { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public bool PaymentIsSuccessful { get; set; }
        public bool IsTopUp { get; set; }
        public int Amount { get; set; }
        public bool IsFromBalance { get; set; }
        public string SessionId { get; set; }
        public string OrderId { get; set; }


        //relation
        public AppUserGetVM AppUser { get; set; }
        public string AppUserId { get; set; }

    }
}
