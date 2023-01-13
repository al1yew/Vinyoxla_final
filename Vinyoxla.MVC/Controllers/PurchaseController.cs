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
                string fileName = await _purchaseService.UserHasReportAndItIsAvailable(selectedReportVM.Vin, phoneno);
                int userBalance = await _purchaseService.GetUserBalance();
                TempData["phoneno"] = phoneno;

                //est li relation

                if (fileName == null)
                {
                    if (userBalance < 4)
                    {
                        return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
                    }
                    else
                    {
                        if (await _purchaseService.SubstractFromBalance())
                        {
                            return RedirectToAction("GetReport", new { vinCode = selectedReportVM.Vin, phoneno, isFromBalance = true });
                        }
                        else
                        {
                            return RedirectToAction("Error", new { errno = 1 });
                        }
                    }
                }
                else if (fileName == "error")
                {
                    if (!await _purchaseService.RefundDueToApiError(phoneno, selectedReportVM.Vin))
                    {
                        return RedirectToAction("Error", new { errno = 5 });
                    }

                    return RedirectToAction("Error", new { errno = 2 });
                    //tut bilo sporno delat refund ili net, no ya sdelal v hasreport metode !isAPiReport, teper on ne smojet 500 raz refund delat
                }

                //esli relation est, proverim starost, esli star, zaplatit babok, kupim i vernem, a esli error, vernem denqi i error 2

                if (await _purchaseService.ReportIsExpired(selectedReportVM.Vin, phoneno))
                {
                    if (userBalance < 4)
                    {
                        return View(await _purchaseService.GetViewModelForOrderPage(selectedReportVM));
                    }
                    else
                    {
                        if (await _purchaseService.SubstractFromBalance())
                        {
                            if (await _purchaseService.ReplaceExpiredReport(selectedReportVM.Vin, phoneno))
                            {
                                return RedirectToAction("Index", "Report", new { fileName });
                            }
                            else
                            {
                                if (!await _purchaseService.RefundDueToApiError(phoneno, selectedReportVM.Vin))
                                {
                                    return RedirectToAction("Error", new { errno = 5 });
                                }

                                return RedirectToAction("Error", new { errno = 2 });
                            }
                        }
                        else
                        {
                            return RedirectToAction("Error", new { errno = 1 });
                        }
                    }
                }
                //esli relation est i ne stariy, tupo pokaju report useru
                return RedirectToAction("Index", "Report", new { fileName });
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
            {
                if (await _purchaseService.UserPurchase(orderVM))
                {
                    return RedirectToAction("GetReport", new { vinCode = orderVM.Vin, phoneno = orderVM.PhoneNumber, isFromBalance = false });
                }
                else
                {
                    return RedirectToAction("Error", new { errno = 1 });
                }
            }
            else
            {
                string fileName = await _purchaseService.UserHasReportAndItIsAvailable(orderVM.Vin, orderVM.PhoneNumber);

                //est li relation

                if (fileName == null)
                {
                    if (await _purchaseService.UserPurchase(orderVM))
                    {
                        return RedirectToAction("GetReport", new { vinCode = orderVM.Vin, phoneno = orderVM.PhoneNumber, isFromBalance = false });
                    }
                    else
                    {
                        return RedirectToAction("Error", new { errno = 1 });
                    }
                }
                else if (fileName == "error")
                {
                    if (!await _purchaseService.RefundDueToApiError(orderVM.PhoneNumber, orderVM.Vin))
                    {
                        return RedirectToAction("Error", new { errno = 5 });
                    }

                    return RedirectToAction("Error", new { errno = 2 });
                }

                //esli est relation, ne star li on, esli star, kuplu noviy, esli error api, vernu denqi i error 2

                if (await _purchaseService.ReportIsExpired(orderVM.Vin, orderVM.PhoneNumber))
                {
                    if (await _purchaseService.ReplaceExpiredReport(orderVM.Vin, orderVM.PhoneNumber))
                    {
                        return RedirectToAction("Index", "Report", new { fileName });
                    }
                    else
                    {
                        if (!await _purchaseService.RefundDueToApiError(orderVM.PhoneNumber, orderVM.Vin))
                        {
                            return RedirectToAction("Error", new { errno = 5 });
                        }

                        return RedirectToAction("Error", new { errno = 2 });
                    }
                }

                //esli relation est i ne stariy, uje tupo vernu report

                return RedirectToAction("Index", "Report", new { fileName });
            }
        }

        public async Task<IActionResult> GetReport(string vinCode, string phoneno, bool isFromBalance)
        {
            string fileName = await _purchaseService.GetReport(vinCode, phoneno, isFromBalance);

            if (fileName == null)
            {
                if (!await _purchaseService.RefundDueToApiError(phoneno, vinCode))
                {
                    return RedirectToAction("Error", new { errno = 4 });
                }
                // burda polubomu refund olmalidi cunki bura elebele adam gelmir, pul odeyen gelir ancaq
                return RedirectToAction("Error", new { errno = 2 });
            }

            return RedirectToAction("Index", "Report", new { fileName });
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
        }
    }
}
//rusum az esli elektrik < 3 let, idxal = 0
