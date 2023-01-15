using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Service.ViewModels.EventVMs;

namespace Vinyoxla.Service.ViewModels.EventMessageVMs
{
    public class EventMessageGetVM
    {
        public int Id { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public string Message { get; set; }

        //relations
        public EventGetVM Event { get; set; }
        public int EventId { get; set; }
    }
}
