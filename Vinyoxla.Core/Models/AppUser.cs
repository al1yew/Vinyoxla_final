using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class AppUser : IdentityUser
    {
        public int Balance { get; set; }
        public bool IsAdmin { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }

        //relations
        public List<AppUserToVincode> AppUserToVincodes { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<Event> Events { get; set; }
    }
}
