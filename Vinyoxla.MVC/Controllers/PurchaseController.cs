using Microsoft.AspNetCore.Cors;
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
            ResultVM resultVM = await _purchaseService.GetReport(cardVM.Vin);

            if (resultVM.FileName == null || resultVM.HTML == null || resultVM.Vin == null || resultVM == null)
            {
                return View("Result", false);
            }

            //snachala nado kupit, i esli smogli kupit, toqda uje prinat platu usera i vidat emu report
            //sdelat iframe i loader

            if (!await _purchaseService.UserPurchase(cardVM))
            {
                return View("Result", false);
            }

            return View("Report", resultVM);
        }
    }
}
