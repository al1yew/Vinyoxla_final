using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Vinyoxla.Service.Exceptions;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.HomeVMs;

namespace Vinyoxla.Service.Implementations
{
    public class HomeService : IHomeService
    {
        public async Task<HomeVM> Find(string vinCode)
        {
            HomeVM homeVM = new HomeVM();

            #region Report

            HttpResponseMessage response = null;

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync($"https://api.allreports.tools/wp-json/v1/get_report_check/{vinCode}");
            }

            if (response.IsSuccessStatusCode)
            {
                homeVM = JsonConvert.DeserializeObject<HomeVM>(await response.Content.ReadAsStringAsync());
            }

            #endregion

            #region Images
            //request image api
            homeVM.ImageVM = new ImageVM()
            {
                AuctionCount = 2,
                ImageCount = 33,
                Vehicle = homeVM.Carfax.Vehicle,
                Vin = homeVM.Carfax.Vin
            };

            #endregion

            return homeVM;
        }
    }
}
