using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.VinCodeVMs
{
    public class CheckVM
    {
        public List<VinCodeGetVM> OldVincodes { get; set; }
        public int OldCount { get; set; }
        public List<VinCodeGetVM> AbsentVincodes { get; set; }
        public int AbsentCount { get; set; }
        public List<string> DublicateVincodes { get; set; }
        public int Dublicates { get; set; }
    }
}
