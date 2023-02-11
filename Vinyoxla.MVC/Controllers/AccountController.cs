using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.BankVMs;

namespace Vinyoxla.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;
        public AccountController(IAccountService accountService, UserManager<AppUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendCode([FromBody] LoginVM loginVM)
        {
            if (!await _accountService.CheckLogin(loginVM))
            {
                return StatusCode(404);
            }

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
            return View(await _accountService.Profile());
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.Logout();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Sort([FromQuery] string vin, int page, int sortbydate, int showcount)
        {
            return PartialView("_AccountReportsPartial", await _accountService.Sort(page, vin, sortbydate, showcount));
        }


        [HttpPost]
        public async Task<IActionResult> TopUp(string amount)
        {
            ReturnVM returnVM = await _accountService.Bank(amount);
            TempData["orderId"] = returnVM.OrderId;
            TempData["sessionId"] = returnVM.SessionId;
            TempData["amount"] = amount;

            return Redirect(returnVM.Url);
        }

        public async Task<IActionResult> UpdateBalance()
        {
            string orderId = TempData["orderId"].ToString();
            string sessionId = TempData["sessionId"].ToString();
            string amount = TempData["amount"].ToString();

            if (amount.Length > 0)
            {
                if (await _accountService.CheckOrder(amount, orderId, sessionId))
                {
                    await _accountService.UpdateBalance(amount, orderId, sessionId);

                    TempData["amount"] = "";
                    TempData["sessionId"] = "";
                    TempData["orderId"] = "";

                    return RedirectToAction("Profile");
                }
                else
                {
                    TempData["amount"] = "";
                    TempData["sessionId"] = "";
                    TempData["orderId"] = "";

                    return RedirectToAction("Error", new { errno = 10 });
                }
            }
            else
            {
                return RedirectToAction("Profile");
            }
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
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
        //        UserName = "+994505788901",
        //        PhoneNumber = "+994505788901",
        //        PhoneNumberConfirmed = true,
        //        Balance = 0,
        //        IsAdmin = true,
        //        CreatedAt = DateTime.UtcNow.AddHours(4),
        //    };

        //    appUser.IsAdmin = true;

        //    await _userManager.CreateAsync(appUser, "Admin123");

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
