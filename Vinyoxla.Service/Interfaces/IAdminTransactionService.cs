using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.TransactionVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminTransactionService
    {
        Task<IQueryable<TransactionGetVM>> GetAllAsync(string phone, string orderId, string sessionId);

        Task<TransactionGetVM> GetById(int? id);

        Task DeleteAsync(int? id);

        Task RefundAsync(int? id);
    }
}
