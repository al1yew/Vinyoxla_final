using Microsoft.AspNetCore.Mvc;
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

                        string fileName = await _purchaseService.ReplaceOldReport(phone, selectedReportVM.Vin, true, "balance", "nalbalanceance");

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
            ReturnVM returnVM = await _purchaseService.Bank(orderVM.Vin, orderVM.PhoneNumber);
            TempData["orderId"] = returnVM.OrderId;
            TempData["sessionId"] = returnVM.SessionId;
            TempData["vin"] = orderVM.Vin;
            TempData["phone"] = orderVM.PhoneNumber;

            return Redirect(returnVM.Url);
        }

        public async Task<IActionResult> GetReport()
        {
            string orderId = TempData["orderId"].ToString();
            string sessionId = TempData["sessionId"].ToString();
            string vin = TempData["vin"].ToString();
            string phoneno = TempData["phone"].ToString();

            if (orderId.Length > 0)
            {
                if (await _purchaseService.CheckOrder(orderId, sessionId, phoneno, vin))
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        string fileName = await _purchaseService.GetReport(phoneno, vin, false, orderId, sessionId);

                        if (fileName == null)
                        {
                            TempData["orderId"] = "";
                            TempData["sessionId"] = "";
                            TempData["vin"] = "";
                            TempData["phone"] = "";

                            return RedirectToAction("Error", new { errno = 2 });
                        }

                        TempData["orderId"] = "";
                        TempData["sessionId"] = "";
                        TempData["vin"] = "";
                        TempData["phone"] = "";

                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                    else
                    {
                        string result = await _purchaseService.CheckEverything(phoneno, vin, true);

                        if (result == "0")
                        {
                            TempData["orderId"] = "";
                            TempData["sessionId"] = "";
                            TempData["vin"] = "";
                            TempData["phone"] = "";

                            return RedirectToAction("Error", new { errno = 0 });
                        }
                        else if (result == "1")
                        {
                            string fileName = await _purchaseService.ReplaceOldReport(phoneno, vin, false, orderId, sessionId);

                            if (fileName == null)
                            {
                                TempData["orderId"] = "";
                                TempData["sessionId"] = "";
                                TempData["vin"] = "";
                                TempData["phone"] = "";

                                return RedirectToAction("Error", new { errno = 1 });
                            }

                            TempData["orderId"] = "";
                            TempData["sessionId"] = "";
                            TempData["vin"] = "";
                            TempData["phone"] = "";

                            return RedirectToAction("Index", "Report", new { fileName });
                        }
                        else if (result == "2")
                        {
                            string fileName = await _purchaseService.GetReport(phoneno, vin, false, orderId, sessionId);

                            if (fileName == null)
                            {
                                TempData["orderId"] = "";
                                TempData["sessionId"] = "";
                                TempData["vin"] = "";
                                TempData["phone"] = "";

                                return RedirectToAction("Error", new { errno = 2 });
                            }

                            TempData["orderId"] = "";
                            TempData["sessionId"] = "";
                            TempData["vin"] = "";
                            TempData["phone"] = "";

                            return RedirectToAction("Index", "Report", new { fileName });
                        }

                        TempData["orderId"] = "";
                        TempData["sessionId"] = "";
                        TempData["vin"] = "";
                        TempData["phone"] = "";

                        return RedirectToAction("Index", "Report", new { fileName = result });
                    }
                }
                else
                {
                    TempData["orderId"] = "";
                    TempData["sessionId"] = "";
                    TempData["vin"] = "";
                    TempData["phone"] = "";

                    return RedirectToAction("Error", new { errno = 10 });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
        }
    }
}

//rusum az esli elektrik < 3 let, idxal = 0
