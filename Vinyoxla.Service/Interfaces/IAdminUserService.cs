using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.UserVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminUserService
    {
        Task<IQueryable<AppUserGetVM>> GetAllAsync(string phone);

        Task<AppUserGetVM> GetById(string id);

        Task CreateAsync(AppUserCreateVM appUserCreateVM);

        Task<AppUserGetVM> GetCurrentUser();

        Task UpdateAsync(string id, AppUserUpdateVM appUserCreateVM);

        Task DeleteAsync(string id);

        Task ChangeMyInfo(AppUserUpdateVM appUserUpdateVM);
    }
}
