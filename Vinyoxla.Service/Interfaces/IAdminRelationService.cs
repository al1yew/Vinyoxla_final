using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminRelationService
    {
        Task<IQueryable<AppUserToVincodeVM>> GetAllAsync(string vin, string phone);

        Task<AppUserToVincodeVM> GetById(int? id);

        Task CreateAsync(AppUserToVincodeCreateVM appUserToVincodeCreateVM);

        Task DeleteAsync(int? id);





        //help

        Task<bool> FileExists(string fileName);

        Task<bool> TryToFixAbsence(string vin, string fileName);

        Task<bool> BuyReport(string vin, string fileName);
    }
}
