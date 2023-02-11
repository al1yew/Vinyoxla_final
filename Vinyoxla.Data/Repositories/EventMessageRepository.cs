using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Core.Models;
using Vinyoxla.Core.Repositories;

namespace Vinyoxla.Data.Repositories
{
    public class EventMessageRepository : Repository<EventMessage>, IEventMessageRepository
    {
        public EventMessageRepository(AppDbContext context) : base(context)
        {

        }
    }
}
