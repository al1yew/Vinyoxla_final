using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Vinyoxla.Service.ViewModels.HomeVMs;

namespace Vinyoxla.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Find(string vinCode)
        {
            HttpResponseMessage response = null;

            HomeVM homeVM;

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync($"https://api.allreports.tools/wp-json/v1/get_report_check/{vinCode}");
            }

            if (response.IsSuccessStatusCode)
            {
                homeVM = JsonConvert.DeserializeObject<HomeVM>(await response.Content.ReadAsStringAsync());

                //etot api uebanskiy, on vmesto year pri oshibke usera vozvrashayet string, takje pri nevernom vin
                //pocemu to status kod 200 OK, kakoy naxren OK esli ti nixuya ne nashel da :/ koroche tupo lovlu 
                //error v axiose i vivoju v toastr

                homeVM.PhotoCount = 15;
                //posle togo kak otpravim request v autoastat polucim photocount, potom tut eshe fotki
                //nado soxranit v assets, krc pizdes

                if (homeVM.Carfax.Records <= 0 || homeVM.Autocheck.Records <= 0)
                {
                    return StatusCode(404);
                }
                //fakticeski etot IF ne nujen daje
            }
            else
            {
                return StatusCode(404);
                //eto toje ne nado ved ix status kod vecno 200 blat
            }

            return PartialView("_ResultsContainerPartial", homeVM);
        }
    }
}
