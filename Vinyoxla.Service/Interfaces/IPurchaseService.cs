using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.BankVMs;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IPurchaseService
    {
        Task<string> CheckEverything(string phone, string vin, bool refund);

        Task<bool> FileExists(string fileName);

        Task<bool> TryToFixAbsence(string vin, string fileName);

        Task<PurchaseVM> GetViewModelForOrderPage(SelectedReportVM selectedReportVM);

        Task<string> ReplaceOldReport(string phone, string vin, bool isFromBalance, string orderId, string sessionId);

        Task<string> GetReport(string phone, string vin, bool isFromBalance, string orderId, string sessionId);

        Task SubstractFromBalance();

        Task<int> GetUserBalance();

        Task<string> GetUserPhoneNumber();

        Task Refund(string phone);

        Task<string> Bank(string vin, string phoneno);

        Task<bool> CheckOrder(string orderId, string sessionId, string phone, string vin);
    }
}
