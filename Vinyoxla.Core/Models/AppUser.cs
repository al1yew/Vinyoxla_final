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
        public List<VinCode> VinCodes { get; set; }
    }
}
