﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.AdminAccountVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AdminAccountService : IAdminAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AdminAccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<List<string>> Login(AdminLoginVM adminLoginVM)
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u =>
            u.UserName == "+994" + adminLoginVM.Login && u.IsAdmin);

            List<string> errors = new List<string>();

            if (appUser == null)
            {
                errors.Add("Login or password is wrong!");
                return errors;
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(appUser, adminLoginVM.Password, true, false);

            if (!signInResult.Succeeded)
            {
                errors.Add("Login or password is wrong!");
                return errors;
            }

            return null;
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
