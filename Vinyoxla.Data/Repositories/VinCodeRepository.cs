using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Core.Models;
using Vinyoxla.Core.Repositories;

namespace Vinyoxla.Data.Repositories
{
    public class VinCodeRepository : Repository<VinCode>, IVinCodeRepository
    {
        public VinCodeRepository(AppDbContext context) : base(context)
        {

        }
    }
}
