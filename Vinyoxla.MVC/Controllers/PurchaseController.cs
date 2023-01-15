using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

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
                string phone = await _purchaseService.GetUserPhoneNumber();
                int userBalance = await _purchaseService.GetUserBalance();
                TempData["phoneno"] = phone;

                string result = await _purchaseService.CheckEverything(phone, selectedReportVM.Vin);

                if (result == "0")
                {
                    return RedirectToAction("Error", new { errno = 0 });
                }
                else if (result == "1")
                {
                    if (userBalance >= 4)
                    {
                        await _purchaseService.SubstractFromBalance();

                        string fileName = await _purchaseService.ReplaceOldReport(phone, selectedReportVM.Vin, true);

                        if (fileName == null)
                        {
                            return RedirectToAction("Error", new { errno = 1 });
                        }

                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                    else
                    {
                        return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
                    }
                }
                else if (result == "2")
                {
                    if (userBalance >= 4)
                    {
                        await _purchaseService.SubstractFromBalance();

                        string fileName = await _purchaseService.GetReport(phone, selectedReportVM.Vin, true);

                        if (fileName == null)
                        {
                            return RedirectToAction("Error", new { errno = 2 });
                        }

                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                    else
                    {
                        return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
                    }
                }

                return RedirectToAction("Index", "Report", new { fileName = result });
            }
            else
            {
                return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(OrderVM orderVM)
        {
            TempData["phoneno"] = orderVM.PhoneNumber;

            if (User.Identity.IsAuthenticated)
            {//on pridet tolko esli relationa net ili je report stariy
                if (await _purchaseService.UserPurchase(orderVM))
                {
                    return RedirectToAction("GetReport", new { vinCode = orderVM.Vin, phoneno = orderVM.PhoneNumber, isFromBalance = false });
                }
                else
                {
                    return RedirectToAction("Error", new { errno = 10 });
                }
            }
            else
            {
                if (await _purchaseService.UserPurchase(orderVM))
                {
                    string result = await _purchaseService.CheckEverything(orderVM.PhoneNumber, orderVM.Vin);

                    if (result == "0")
                    {
                        return RedirectToAction("Error", new { errno = 0 });
                    }
                    else if (result == "1")
                    {
                        string fileName = await _purchaseService.ReplaceOldReport(orderVM.PhoneNumber, orderVM.Vin, false);

                        if (fileName == null)
                        {
                            return RedirectToAction("Error", new { errno = 1 });
                        }

                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                    else
                    {
                        string fileName = await _purchaseService.GetReport(orderVM.PhoneNumber, orderVM.Vin, false);

                        if (fileName == null)
                        {
                            return RedirectToAction("Error", new { errno = 2 });
                        }

                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                }

                return RedirectToAction("Error", new { errno = 10 });
            }
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
        }
    }
}

//rusum az esli elektrik < 3 let, idxal = 0
