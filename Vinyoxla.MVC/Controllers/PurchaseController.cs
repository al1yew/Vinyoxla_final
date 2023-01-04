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

        public async Task<IActionResult> Index(SelectedReportVM selectedReportVM)
        {
            if (User.Identity.IsAuthenticated)
            {
                string phoneno = await _purchaseService.GetUserPhoneNumber();

                if (!await _purchaseService.UserHasReport(selectedReportVM.Vin, phoneno))
                {
                    if (await _purchaseService.GetUserBalance() < 5)
                    {
                        return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
                    }
                    else
                    {
                        if (await _purchaseService.SubstractFromBalance(selectedReportVM.Vin))
                        {
                            return RedirectToAction("Report", new { vinCode = selectedReportVM.Vin, phoneno });
                        }
                        else
                        {
                            return View("Result", false);
                        }
                    }
                }
                else
                {
                    string fileName = await _purchaseService.GetReportFileName(selectedReportVM.Vin);

                    return RedirectToAction("ViewReport", new { fileName });
                }
            }
            else
            {
                return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(OrderVM orderVM)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (await _purchaseService.UserPurchase(orderVM))
                {
                    return RedirectToAction("Report", new { vinCode = orderVM.Vin, phoneno = orderVM.PhoneNumber });
                }
                else
                {
                    return View("Result", false);
                }
            }
            else
            {
                if (!await _purchaseService.UserHasReport(orderVM.Vin, orderVM.PhoneNumber))
                {
                    if (await _purchaseService.UserPurchase(orderVM))
                    {
                        return RedirectToAction("Report", new { vinCode = orderVM.Vin, phoneno = orderVM.PhoneNumber });
                    }
                    else
                    {
                        return View("Result", false);
                    }
                }
                else
                {
                    string fileName = await _purchaseService.GetReportFileName(orderVM.Vin);

                    return RedirectToAction("ViewReport", new { fileName });
                }
            }
        }

        public async Task<IActionResult> Report(string vinCode, string phoneno)
        {
            string fileName = await _purchaseService.GetReport(vinCode, phoneno);

            if (fileName == null)
            {
                return View("Result", false);
            }

            return RedirectToAction("ViewReport", new { fileName });
        }

        public async Task<IActionResult> ViewReport(string fileName)
        {
            return View(await _purchaseService.GetUsersReport(fileName));
        }
    }
}
