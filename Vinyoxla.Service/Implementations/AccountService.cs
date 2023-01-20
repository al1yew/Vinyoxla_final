using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AccountService : IAccountService
    {
        private IConfiguration Configuration { get; }
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CheckLogin(LoginVM loginVM)
        {
            Regex regexNumber = new Regex("^[0-9]+$");

            if (regexNumber.IsMatch(loginVM.PhoneNumber.ToString()))
            {
                if (!(loginVM.PhoneNumber.StartsWith("50") ||
                    loginVM.PhoneNumber.StartsWith("10") ||
                    loginVM.PhoneNumber.StartsWith("51") ||
                    loginVM.PhoneNumber.StartsWith("70") ||
                    loginVM.PhoneNumber.StartsWith("77") ||
                    loginVM.PhoneNumber.StartsWith("99") ||
                    loginVM.PhoneNumber.StartsWith("55")))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public async Task<int> SendCode(string number)
        {
            string phone = "+994" + number;

            Random random = new Random();
            int generatedcode = random.Next(1000, 9999);

            string url = $"http://api.msm.az/sendsms?user={Configuration.GetSection("MSM:Username").Value}&password={Configuration.GetSection("MSM:Apikey").Value}&gsm={phone}&from=MSM&text={generatedcode}";

            HttpResponseMessage response = null;

            string responsedecoded = "";

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync(url);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (response.IsSuccessStatusCode)
            {
                responsedecoded = await response.Content.ReadAsStringAsync();

                if (responsedecoded.Contains("errno=100"))
                {
                    return generatedcode;
                }
            }

            return 0;
        }

        public async Task<List<string>> Login(LoginVM loginVM, int code)
        {
            AppUser appUser = await _userManager.Users.Include(u => u.AppUserToVincodes).ThenInclude(x => x.VinCode).FirstOrDefaultAsync(u => u.UserName == "+994" + loginVM.PhoneNumber);

            List<string> errors = new List<string>();

            if (loginVM.Code == code)
            {
                if (appUser == null)
                {
                    AppUser newUser = new AppUser()
                    {
                        UserName = "+994" + loginVM.PhoneNumber,
                        PhoneNumber = "+994" + loginVM.PhoneNumber,
                        PhoneNumberConfirmed = false,
                        Balance = 0,
                        IsAdmin = false
                    };

                    IdentityResult result = await _userManager.CreateAsync(newUser);

                    if (!result.Succeeded)
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            errors.Add(error.Description);
                        }

                        return errors;
                    }

                    result = await _userManager.AddToRoleAsync(newUser, "Member");

                    await _signInManager.SignInAsync(newUser, true);
                }
                else
                {
                    appUser.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(appUser);

                    await _signInManager.SignInAsync(appUser, true);
                }
            }
            else
            {
                errors.Add("Kodu səhv daxil etmisiniz.");
            }

            return errors;
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<AccountVM> Profile()
        {
            AppUserGetVM appUser = _mapper.Map<AppUserGetVM>(await _userManager.Users.Include(x => x.AppUserToVincodes).ThenInclude(x => x.VinCode).FirstOrDefaultAsync(x => x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name));

            List<AppUserToVincodeVM> appUserToVincodes = _mapper.Map<List<AppUserToVincodeVM>>(await _unitOfWork.AppUserToVincodeRepository.GetAllByExAsync(x =>
            x.AppUserId == appUser.Id, "AppUser", "VinCode"));

            IQueryable<AppUserToVincodeVM> query = appUserToVincodes.AsQueryable();

            query = query.OrderBy(x => x.CreatedAt);

            AccountVM accountVM = new AccountVM()
            {
                AppUserToVincodes = PaginationList<AppUserToVincodeVM>.Create(query, 1, 10),
                Balance = appUser.Balance
            };

            return accountVM;
        }

        public async Task<PaginationList<AppUserToVincodeVM>> Sort(int page, string vin, int sortbydate, int showcount)
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);

            List<AppUserToVincodeVM> appUserToVincodes = _mapper.Map<List<AppUserToVincodeVM>>(await _unitOfWork.AppUserToVincodeRepository.GetAllByExAsync(x =>
            x.AppUserId == appUser.Id, "AppUser", "VinCode"));

            IQueryable<AppUserToVincodeVM> query = appUserToVincodes.AsQueryable();

            if (vin != null)
            {
                query = query.Where(x => x.VinCode.Vin.Contains(vin.Trim().ToUpperInvariant())).OrderBy(x => x.CreatedAt);
            }

            if (sortbydate == 2)
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            return PaginationList<AppUserToVincodeVM>.Create(query, page == 0 ? 1 : page, showcount == 0 ? 1 : showcount);
        }
    }
}
