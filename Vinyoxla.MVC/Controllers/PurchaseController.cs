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

        public async Task<IActionResult> PrePurchase(SelectedReportVM selectedReportVM)
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
            //return StatusCode(200); // make xml request to kapital bank, return status code, assign this code to tempdata
            //ili je xz, tam je est approveurl, declineurl, vnutri xml. Mogu li ya ix izbejat? esli net, sgenerirovat
            //url iz route values, v GetReport prinat ne ordervm, a string vin, string phone i brat ix s linka.
            return RedirectToAction("GetReport", orderVM);
        }

        public async Task<IActionResult> GetReport(OrderVM orderVM)
        {
            if (User.Identity.IsAuthenticated)
            {//on pridet tolko esli relationa net ili je report stariy
                return RedirectToAction("GetReport", new { vinCode = orderVM.Vin, phoneno = orderVM.PhoneNumber, isFromBalance = false });
            }
            else
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
                else if (result == "2")
                {
                    string fileName = await _purchaseService.GetReport(orderVM.PhoneNumber, orderVM.Vin, false);

                    if (fileName == null)
                    {
                        return RedirectToAction("Error", new { errno = 2 });
                    }

                    return RedirectToAction("Index", "Report", new { fileName });
                }

                return RedirectToAction("Index", "Report", new { fileName = result });
            }
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
        }
    }
}

//rusum az esli elektrik < 3 let, idxal = 0
