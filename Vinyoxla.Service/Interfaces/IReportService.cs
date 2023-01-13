using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IReportService
    {
        Task<ResultVM> GetUsersReport(string fileName);
    }
}
