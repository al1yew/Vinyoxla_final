using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Service.ViewModels.HomeVMs
{
    public class HomeVM
    {
        public int PhotoCount { get; set; }
        public int AuctionCount { get; set; }
        public CarfaxVM Carfax { get; set; }
        public AutoCheckVM Autocheck { get; set; }
    }
}
