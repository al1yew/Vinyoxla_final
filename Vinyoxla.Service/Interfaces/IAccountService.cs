using System.Collections.Generic;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;
using Vinyoxla.Service.ViewModels.BankVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAccountService
    {
        Task<bool> CheckLogin(LoginVM loginVM);

        Task<int> SendCode(string number);

        Task<List<string>> Login(LoginVM loginVM, int code);

        Task Logout();

        Task<AccountVM> Profile();

        Task<PaginationList<AppUserToVincodeVM>> Sort(string vin, int page = 1, int sortbydate = 1, int showcount = 10);

        Task<string> Bank(string amonut);

        Task<bool> CheckOrder(string amount, string orderId, string sessionId, string phone);

        Task UpdateBalance(string amount, string orderId, string sessionId, string phone);

    }
}
