using System.Threading.Tasks;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.ViewModels.PurchaseVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IPurchaseService
    {
        Task<string> CheckEverything(string phone, string vin);

        Task<bool> UserHasReport(string phone, string vin);

        Task<bool> FileExists(string vin);

        Task<bool> TryToFixAbsence(string vin, string fileName);

        Task<bool> UserPurchase(OrderVM orderVM);

        Task<PurchaseVM> GetViewModelForOrderPage(SelectedReportVM selectedReportVM);

        Task<string> ReplaceOldReport(string phone, string vin, bool isFromBalance);

        Task<string> GetReport(string phone, string vin, bool isFromBalance);

        Task SubstractFromBalance();

        Task<int> GetUserBalance();

        Task<string> GetUserPhoneNumber();

        Task Refund(string phone);
    }
}
