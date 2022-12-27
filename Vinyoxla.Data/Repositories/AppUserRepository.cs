using Vinyoxla.Core.Models;
using Vinyoxla.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vinyoxla.Data.Repositories
{
    public class AppUserRepository : Repository<AppUser>, IAppUserRepository
    {
        public AppUserRepository(AppDbContext context) : base(context)
        {

        }
    }
}
