using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.AdminHomeVMs
{
    public class AdminHomeVM
    {
        public double AllReportsBalance { get; set; }
        public int UserCount { get; set; }
        public int TodayUsersCount { get; set; }
        public int TodayRelations { get; set; }
        public int Earned { get; set; }
        public int VinCount { get; set; }
    }
}
