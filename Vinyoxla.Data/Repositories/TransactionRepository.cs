using System;
using System.Collections.Generic;
using System.Text;
using Vinyoxla.Core.Models;
using Vinyoxla.Core.Repositories;

namespace Vinyoxla.Data.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        {

        }
    }
}
