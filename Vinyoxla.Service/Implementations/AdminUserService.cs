using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Exceptions;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.UserVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<AppUser> _signInManager;

        public AdminUserService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, SignInManager<AppUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
        }

        public async Task<IQueryable<AppUserGetVM>> GetAllAsync(string phone)
        {
            List<AppUserGetVM> dbList = _mapper.Map<List<AppUserGetVM>>
                (await _unitOfWork.AppUserRepository.GetAllByExAsync(x =>
                x.UserName != _httpContextAccessor.HttpContext.User.Identity.Name && x.UserName != "+994505788901",
                "Events", "Transactions", "AppUserToVincodes"));

            IQueryable<AppUserGetVM> query = dbList.AsQueryable();

            if (phone != null)
            {
                query = query.Where(x => x.UserName.Contains(phone.Trim().ToUpperInvariant()));
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<AppUserGetVM> GetById(string id)
        {
            if (id == null)
                throw new BadRequestException("Id is null!");

            AppUserGetVM appUser = _mapper.Map<AppUserGetVM>
                (await _unitOfWork.AppUserRepository.GetAsync(x =>
                x.Id == id, "Events", "Transactions", "AppUserToVincodes", "Events.EventMessages", "AppUserToVincodes.VinCode"));

            if (appUser == null)
                throw new NotFoundException("User cannot be found!");

            return appUser;
        }
        public async Task<AppUserGetVM> GetCurrentUser()
        {
            return _mapper.Map<AppUserGetVM>
                (await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name));
        }

        public async Task CreateAsync(AppUserCreateVM appUserCreateVM)
        {
            AppUser appUser = new AppUser
            {
                UserName = "+994" + appUserCreateVM.PhoneNumber,
                PhoneNumber = "+994" + appUserCreateVM.PhoneNumber,
                PhoneNumberConfirmed = true,
                Balance = appUserCreateVM.Balance,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow.AddHours(4),
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, appUserCreateVM.Password);

            if (!result.Succeeded)
                throw new BadRequestException("Can't create user!");

            result = await _userManager.AddToRoleAsync(appUser, "Admin");

            if (!result.Succeeded)
                throw new BadRequestException("Can't add to role!");
        }

        public async Task UpdateAsync(string id, AppUserUpdateVM appUserUpdateVM)
        {
            if (id == null)
                throw new BadRequestException($"Id is null!");

            if (id != appUserUpdateVM.Id)
                throw new BadRequestException($"Id's are not the same!");

            AppUser dbAppUser = await _unitOfWork.AppUserRepository.GetAsync(x => x.Id == id);

            if (dbAppUser == null)
                throw new NotFoundException($"User Cannot be found By id = {id}");

            dbAppUser.PhoneNumber = "+994" + appUserUpdateVM.PhoneNumber;
            dbAppUser.UserName = "+994" + appUserUpdateVM.PhoneNumber;
            dbAppUser.Balance = appUserUpdateVM.Balance;

            IdentityResult result = await _userManager.UpdateAsync(dbAppUser);

            if (!result.Succeeded)
                throw new BadRequestException("Can't update user!");

            if (appUserUpdateVM.Password != null)
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(dbAppUser);

                result = await _userManager.ResetPasswordAsync(dbAppUser, token, appUserUpdateVM.Password);

                if (!result.Succeeded)
                    throw new BadRequestException("Can't update user's password!");
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (id == null)
                throw new BadRequestException($"Id is null!");

            AppUser dbAppUser = await _unitOfWork.AppUserRepository.GetAsync(c => c.Id == id,
                "AppUserToVincodes", "Events", "Transactions");

            if (dbAppUser == null)
                throw new NotFoundException($"App User Cannot be found By id = {id}");

            IdentityResult result = await _userManager.DeleteAsync(dbAppUser);

            if (!result.Succeeded)
                throw new BadRequestException("Can't delete user!");

            List<AppUserToVincode> relations = dbAppUser.AppUserToVincodes;
            List<Transaction> transactions = dbAppUser.Transactions;
            List<Event> events = dbAppUser.Events;

            if (relations != null)
            {
                foreach (var relation in relations)
                {
                    _unitOfWork.AppUserToVincodeRepository.Remove(relation);
                }
            }

            if (transactions != null)
            {
                foreach (var transaction in transactions)
                {
                    _unitOfWork.TransactionRepository.Remove(transaction);
                }
            }

            if (events != null)
            {
                foreach (var eventik in events)
                {
                    _unitOfWork.EventRepository.Remove(eventik);
                }
            }

            await _unitOfWork.CommitAsync();
        }

        public async Task ChangeMyInfo(AppUserUpdateVM appUserUpdateVM)
        {
            AppUser dbAppUser = await _userManager.FindByNameAsync(_httpContextAccessor.HttpContext.User.Identity.Name);

            dbAppUser.PhoneNumber = "+994" + appUserUpdateVM.PhoneNumber;
            dbAppUser.UserName = "+994" + appUserUpdateVM.PhoneNumber;
            dbAppUser.Balance = appUserUpdateVM.Balance;

            IdentityResult result = await _userManager.UpdateAsync(dbAppUser);

            if (!result.Succeeded)
                throw new BadRequestException("Can't update your account!");

            if (appUserUpdateVM.Password != null)
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(dbAppUser);

                result = await _userManager.ResetPasswordAsync(dbAppUser, token, appUserUpdateVM.Password);

                if (!result.Succeeded)
                    throw new BadRequestException("Can't update your password!");
            }

            await _signInManager.SignOutAsync();
        }
    }
}
