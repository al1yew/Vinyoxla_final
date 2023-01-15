using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Core.Models
{
    public class EventMessage : BaseModel
    {
        public string Message { get; set; }

        public Event Event { get; set; }
        public int EventId { get; set; }
    }
}
