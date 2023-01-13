using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IPurchaseService
    {
        Task<PurchaseVM> GetViewModelForOrderPage(SelectedReportVM selectedReportVM);

        Task<bool> UserPurchase(OrderVM orderVM);

        Task<string> GetReport(string vinCode, string phoneno, bool isFromBalance);

        Task<bool> ReportIsExpired(string vin, string phoneno);

        Task<bool> ReplaceExpiredReport(string vin, string phoneno);

        Task<string> UserHasReportAndItIsAvailable(string vin, string phoneno);

        Task<bool> BuyReportAgain(string fileName);

        Task<bool> RefundDueToApiError(string phoneno, string vinCode);

        Task<bool> SubstractFromBalance();

        Task<int> GetUserBalance();

        Task<string> GetUserPhoneNumber();
    }
}
