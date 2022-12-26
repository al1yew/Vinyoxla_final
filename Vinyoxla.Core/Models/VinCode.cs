using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class VinCode : BaseModel
    {
        public string Vin { get; set; }
        public int FileName { get; set; }
        public bool IsCarfax { get; set; }
        public bool IsAutoCheck { get; set; }


        //relations
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
    }
}
