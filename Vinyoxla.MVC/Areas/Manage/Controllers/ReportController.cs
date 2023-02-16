using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Manage")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        public ReportController(IReportService reportService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _reportService = reportService;
            _env = env;
            Configuration = configuration;
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
