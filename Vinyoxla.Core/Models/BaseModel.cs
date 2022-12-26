using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class BaseModel
    {
        public int Id { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
    }
}
