using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IPurchaseService
    {
        PurchaseVM GetViewModel(SelectedReportVM selectedReportVM);

        Task<bool> UserPurchase(CardVM cardVM);

        Task<string> GetReport(string vinCode, int reportType);
    }
}
