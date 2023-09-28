using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.UserVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IMapper _mapper;
        public UserController(IAdminUserService adminUserService, IMapper mapper)
        {
            _adminUserService = adminUserService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int select, string phone, int page = 1)
        {
            IQueryable<AppUserListVM> users = await _adminUserService.GetAllAsync(phone);

            if (select <= 0)
            {
                select = 10;
            }

            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.WhereWeAre = "Users";

            return View(PaginationList<AppUserListVM>.Create(users, page, select));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            ViewBag.WhereWeAre = "Details";

            return View(await _adminUserService.GetById(id));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.WhereWeAre = "Create Admin";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppUserCreateVM appUserCreateVM)
        {
            ViewBag.WhereWeAre = "Users";

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "");
                return View(appUserCreateVM);
            }

            await _adminUserService.CreateAsync(appUserCreateVM);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            ViewBag.WhereWeAre = "Update User";

            return View(_mapper.Map<AppUserUpdateVM>(await _adminUserService.GetByIdForUpdate(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, AppUserUpdateVM appUserUpdateVM)
        {
            ViewBag.WhereWeAre = "Users";

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "");
                return View(appUserUpdateVM);
            }

            await _adminUserService.UpdateAsync(id, appUserUpdateVM);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeMyInfo()
        {
            ViewBag.WhereWeAre = "Change Info";

            return View(_mapper.Map<AppUserUpdateVM>(await _adminUserService.GetCurrentUser()));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeMyInfo(AppUserUpdateVM appUserUpdateVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "");
                return View();
            }

            await _adminUserService.ChangeMyInfo(appUserUpdateVM);

            return RedirectToAction("Login", "Account", new { area = "Manage" });
        }

        public async Task<IActionResult> Delete(string id, string phone, int select, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.WhereWeAre = "Users";

            await _adminUserService.DeleteAsync(id);

            IQueryable<AppUserListVM> users = await _adminUserService.GetAllAsync(phone);

            return PartialView("_UserIndexPartial", PaginationList<AppUserListVM>.Create(users, page, select));
        }
    }
}
