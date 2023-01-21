using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.AdminHomeVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminHomeService
    {
        Task<AdminHomeVM> GetData();
    }
}
