using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Core.Models;
using Vinyoxla.Core.Repositories;

namespace Vinyoxla.Data.Repositories
{
    public class AppUserToVincodeRepository : Repository<AppUserToVincode>, IAppUserToVincodeRepository
    {
        public AppUserToVincodeRepository(AppDbContext context) : base(context)
        {

        }
    }
}
