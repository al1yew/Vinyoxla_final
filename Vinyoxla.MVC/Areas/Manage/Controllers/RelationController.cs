using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Manage")]
    public class RelationController : Controller
    {
        private readonly IAdminRelationService _adminRelationService;
        private readonly IMapper _mapper;
        public RelationController(IAdminRelationService adminRelationService, IMapper mapper)
        {
            _adminRelationService = adminRelationService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int select, string vin, string phone, int page = 1)
        {
            IQueryable<AppUserToVincodeVM> appUserToVincodes = await _adminRelationService.GetAllAsync(vin, phone);

            if (select <= 0)
            {
                select = 5;
            }

            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.Vin = vin;
            ViewBag.WhereWeAre = "Relations";

            return View(PaginationList<AppUserToVincodeVM>.Create(appUserToVincodes, page, select));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.WhereWeAre = "Create";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppUserToVincodeCreateVM appUserToVincodeCreateVM)
        {
            ViewBag.WhereWeAre = "Relations";

            if (!ModelState.IsValid)
            {
                return View(appUserToVincodeCreateVM);
            }

            await _adminRelationService.CreateAsync(appUserToVincodeCreateVM);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id, int select, string vin, string phone, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.Vin = vin;
            ViewBag.WhereWeAre = "Relations";

            await _adminRelationService.DeleteAsync(id);

            IQueryable<AppUserToVincodeVM> appUserToVincodes = await _adminRelationService.GetAllAsync(vin, phone);

            return PartialView("_RelationIndexPartial", PaginationList<AppUserToVincodeVM>.Create(appUserToVincodes, page, select));
        }
    }
}
