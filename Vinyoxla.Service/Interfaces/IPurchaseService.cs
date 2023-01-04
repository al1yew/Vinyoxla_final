using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IPurchaseService
    {
        Task<PurchaseVM> GetViewModelForOrderPage(SelectedReportVM selectedReportVM);

        Task<bool> UserPurchase(OrderVM orderVM);

        Task<string> GetReport(string vinCode, string phoneno);

        Task<int> GetUserBalance();

        Task<string> GetUserPhoneNumber();

        Task<bool> SubstractFromBalance(string vin);

        Task<bool> UserHasReport(string vin, string phoneno);

        Task<ResultVM> GetUsersReport(string vinCode);

        Task<string> GetReportFileName(string vin);
    }
}
