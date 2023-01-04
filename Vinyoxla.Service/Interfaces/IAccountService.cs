using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.AccountVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAccountService
    {
        Task<bool> CheckLogin(LoginVM loginVM);

        Task<int> SendCode(string number);

        Task<List<string>> Login(LoginVM loginVM, int code);

        Task Logout();

        Task<AppUserVM> Profile();
    }
}
