using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.AdminAccountVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminAccountService
    {
        Task<List<string>> Login(AdminLoginVM adminLogin);

        Task Logout();
    }
}
