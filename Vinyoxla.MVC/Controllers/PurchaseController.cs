using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;


namespace Vinyoxla.MVC.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        public IActionResult Index(SelectedReportVM selectedReportVM)
        {
            return View(_purchaseService.GetViewModel(selectedReportVM));
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(CardVM cardVM)
        {
            string response = await _purchaseService.GetReport(cardVM.Vin, cardVM.Type);

            if (response == null)
            {
                return View("Result", false);
            }
            //snachala nado kupit, i esli smogli kupit, toqda uje prinat platu usera i vidat emu report
            if (!await _purchaseService.UserPurchase(cardVM))
            {
                return View("Result", false);
            }

            return View("Report", response);
        }
    }
}
