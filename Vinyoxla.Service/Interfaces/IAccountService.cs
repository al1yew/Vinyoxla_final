using System.Collections.Generic;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAccountService
    {
        Task<bool> CheckLogin(LoginVM loginVM);

        Task<int> SendCode(string number);

        Task<List<string>> Login(LoginVM loginVM, int code);

        Task Logout();

        Task<AccountVM> Profile();

        Task<PaginationList<AppUserToVincodeVM>> Sort(int page, string vin, int sortbydate, int showcount);

    }
}
