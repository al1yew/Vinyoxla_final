using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.MVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(string fileName)
        {
            ResultVM resultVM = await _reportService.GetUsersReport(fileName);

            if (resultVM == null)
            {
                return RedirectToAction("Error", new { errno = 6 });
            }

            return View(resultVM);
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
        }
    }
}
