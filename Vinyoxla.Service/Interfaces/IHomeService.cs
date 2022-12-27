using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.HomeVMs;

namespace Vinyoxla.Service.Interfaces
{
    public interface IHomeService
    {
        Task<HomeVM> Find(string vinCode);
    }
}
