using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.BankVMs;
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

                string result = await _purchaseService.CheckEverything(phone, selectedReportVM.Vin, false);

                if (result == "0")
                {
                    return RedirectToAction("Error", new { errno = 0 });
                }
                else if (result == "1")
                {
                    if (userBalance >= 4)
                    {
                        await _purchaseService.SubstractFromBalance();

                        string fileName = await _purchaseService.ReplaceOldReport(phone, selectedReportVM.Vin, true, "balance", "balance");

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

                        string fileName = await _purchaseService.GetReport(phone, selectedReportVM.Vin, true, "balance", "balance");

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
            string url = await _purchaseService.Bank(orderVM.Vin, orderVM.PhoneNumber);

            if (url != null)
            {
                return Redirect(url);
            }

            return RedirectToAction("Error", new { errno = 3 });
        }

        public async Task<IActionResult> GetReport()
        {
            string cookieInfo = HttpContext.Request.Cookies["cookieInfo"];

            CookieInfoVM cookieInfoVM = new CookieInfoVM();

            if (string.IsNullOrWhiteSpace(cookieInfo))
            {
                return RedirectToAction("Index", "Home");
            }

            cookieInfoVM = JsonConvert.DeserializeObject<CookieInfoVM>(cookieInfo);

            string orderId = cookieInfoVM.OrderId;
            string sessionId = cookieInfoVM.SessionId;
            string phoneno = cookieInfoVM.PhoneNumber;
            string vin = cookieInfoVM.VinCode;

            HttpContext.Response.Cookies.Delete("cookieInfo");

            if (await _purchaseService.CheckOrder(orderId, sessionId, phoneno, vin))
            {
                if (User.Identity.IsAuthenticated)
                {
                    string fileName = await _purchaseService.GetReport(phoneno, vin, false, orderId, sessionId);

                    if (fileName == null)
                    {
                        return RedirectToAction("Error", new { errno = 2 });
                    }

                    return RedirectToAction("Index", "Report", new { fileName });
                }
                else
                {
                    string result = await _purchaseService.CheckEverything(phoneno, vin, true);

                    if (result == "0")
                    {
                        return RedirectToAction("Error", new { errno = 0 });
                    }
                    else if (result == "1")
                    {
                        string fileName = await _purchaseService.ReplaceOldReport(phoneno, vin, false, orderId, sessionId);

                        if (fileName == null)
                        {
                            return RedirectToAction("Error", new { errno = 1 });
                        }

                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                    else if (result == "2")
                    {
                        string fileName = await _purchaseService.GetReport(phoneno, vin, false, orderId, sessionId);

                        if (fileName == null)
                        {
                            return RedirectToAction("Error", new { errno = 2 });
                        }

                        return RedirectToAction("Index", "Report", new { fileName });
                    }

                    return RedirectToAction("Index", "Report", new { fileName = result });
                }
            }
            else
            {
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
