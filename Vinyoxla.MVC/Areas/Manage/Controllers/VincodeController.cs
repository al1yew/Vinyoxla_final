using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Manage")]
    public class VincodeController : Controller
    {
        private readonly IAdminVincodeService _adminVincodeService;

        public VincodeController(IAdminVincodeService adminVincodeService)
        {
            _adminVincodeService = adminVincodeService;
        }

        public async Task<IActionResult> Index(int select, string vin, int page = 1)
        {
            IQueryable<VinCodeGetVM> vinCodes = await _adminVincodeService.GetAllAsync(vin);

            if (select <= 0)
            {
                select = 5;
            }

            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Vin = vin;
            ViewBag.WhereWeAre = "Vincodes";

            return View(PaginationList<VinCodeGetVM>.Create(vinCodes, page, select));
        }

        public async Task<IActionResult> Delete(int? id, string vin, int select, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.WhereWeAre = "Vincodes";

            await _adminVincodeService.DeleteAsync(id);

            IQueryable<VinCodeGetVM> vinCodes = await _adminVincodeService.GetAllAsync(vin);

            return PartialView("_VincodeIndexPartial", PaginationList<VinCodeGetVM>.Create(vinCodes, page, select));
        }
    }
}
