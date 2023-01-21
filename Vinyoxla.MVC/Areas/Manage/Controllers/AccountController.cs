using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.AdminAccountVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
        private readonly IAdminAccountService _adminAccountService;

        public AccountController(IAdminAccountService adminAccountService)
        {
            _adminAccountService = adminAccountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginVM adminLoginVM)
        {
            if (!ModelState.IsValid) return View(adminLoginVM);

            List<string> errors = await _adminAccountService.Login(adminLoginVM);

            if (errors != null)
            {
                foreach (string error in errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(adminLoginVM);
            }

            return RedirectToAction("Index", "Home", new { area = "Manage" });
        }

        public async Task<IActionResult> Logout()
        {
            await _adminAccountService.Logout();

            return RedirectToAction("Login");
        }
    }
}
