using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.UserVMs
{
    public class AppUserListVM
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string UserName { get; set; }
        public int Balance { get; set; }
        public bool IsAdmin { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
    }
}
