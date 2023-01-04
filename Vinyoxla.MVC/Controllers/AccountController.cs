using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.AccountVMs;

namespace Vinyoxla.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendCode([FromBody] LoginVM loginVM)
        {
            //yoxlayirig
            if (!await _accountService.CheckLogin(loginVM))
            {
                return StatusCode(404);
            }

            //kodu gonderirik
            int response = await _accountService.SendCode(loginVM.PhoneNumber);

            if (response == 0)
            {
                return StatusCode(404);
            }

            TempData["UserConfirmationCode"] = response;

            return PartialView("_AccountCodePartial");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            foreach (string err in await _accountService.Login(loginVM, (int)TempData["UserConfirmationCode"]))
            {
                ModelState.AddModelError("", err);
                return View();
            }

            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> Profile()
        {
            AppUserVM appUser = await _accountService.Profile();

            return View(appUser);
        }

        public IActionResult TopUp()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.Logout();

            return RedirectToAction("Index", "Home");
        }


        #region Created Roles

        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "Member" });

        //    return Content("Salammmmm");
        //}

        #endregion

        #region Created Super Admin

        //public async Task<IActionResult> CreateSuperAdmin()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        UserName = "Admin",
        //        Email = "admin@admin",
        //        PhoneNumber = "+994505788901",
        //        PhoneNumberConfirmed = true,
        //        EmailConfirmed = true,
        //        Balance = 0,
        //        IsAdmin = true
        //    };

        //    appUser.IsAdmin = true;

        //    await _userManager.CreateAsync(appUser, "Admin@123");

        //    await _userManager.AddToRoleAsync(appUser, "Admin");

        //    return Content("Admin est ");
        //}

        #endregion

        #region Created users

        //public async Task<IActionResult> Createuser()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        UserName = "+994503923210",
        //        PhoneNumber = "+994505788901",
        //        PhoneNumberConfirmed = true,
        //        Balance = 0,
        //        IsAdmin = false
        //    };

        //    appUser.IsAdmin = false;

        //    IdentityResult result = await _userManager.CreateAsync(appUser, "Vasif123");

        //    await _userManager.AddToRoleAsync(appUser, "Member");

        //    return Content("member est");
        //}

        #endregion
    }
}
