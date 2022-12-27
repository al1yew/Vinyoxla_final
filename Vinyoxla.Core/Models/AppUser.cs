using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class AppUser : IdentityUser
    {
        public int Balance { get; set; }

        //relations
        public List<AppUserToVincode> AppUserToVincodes { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
