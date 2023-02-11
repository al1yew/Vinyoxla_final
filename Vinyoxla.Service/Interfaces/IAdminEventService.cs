using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.EventVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IAdminEventService
    {
        Task<IQueryable<EventGetVM>> GetAllAsync(string vin, string phone);

        Task<EventGetVM> GetById(int? id);

        Task DeleteEventAsync(int? id);

        Task DeleteMessageAsync(int? id);
    }
}
