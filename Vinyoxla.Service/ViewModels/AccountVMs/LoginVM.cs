using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.AccountVMs
{
    public class LoginVM
    {
        public string PhoneNumber { get; set; }
        public Nullable<int> Code { get; set; }
    }
}
