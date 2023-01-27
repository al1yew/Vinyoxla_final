using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminVincodeService
    {
        Task<IQueryable<VinCodeGetVM>> GetAllAsync(string vin);

        Task DeleteAsync(int? id);
    }
}
