using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.AdminHomeVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AdminHomeService : IAdminHomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private IConfiguration Configuration { get; }

        public AdminHomeService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            Configuration = configuration;
        }

        public async Task<AdminHomeVM> GetData()
        {
            #region Balance

            string url = $"https://api.allreports.tools/wp-json/v1/get_client_payment_balance/{Configuration.GetSection("Api_Key:MyKey").Value}";

            HttpResponseMessage response = null;

            BalanceVM balanceVM = new BalanceVM();

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync(url);
            }

            if (response.IsSuccessStatusCode)
            {
                string convertedResponse = await response.Content.ReadAsStringAsync();

                balanceVM = JsonConvert.DeserializeObject<BalanceVM>(convertedResponse);
            }

            #endregion

            #region Data

            List<AppUser> appUsers = await _unitOfWork.AppUserRepository.GetAllByExAsync(x => !x.IsAdmin);

            int newUsersCount = appUsers.Where(x => (DateTime.Now - x.CreatedAt.Value).TotalDays <= 1).Count();

            List<AppUserToVincode> appUserToVincodes = await _unitOfWork.AppUserToVincodeRepository.GetAllAsync();

            int newRelationCount = appUserToVincodes.Where(x => (DateTime.Now - x.CreatedAt.Value).TotalDays <= 1).Count();

            List<Transaction> transactions = await _unitOfWork.TransactionRepository.GetAllByExAsync(x => x.PaymentIsSuccessful);

            int earned = transactions.Sum(x => x.Amount);

            List<VinCode> vinCodes = await _unitOfWork.VinCodeRepository.GetAllAsync();

            int vinCount = vinCodes.Count;

            #endregion

            AdminHomeVM adminHomeVM = new AdminHomeVM()
            {
                AllReportsBalance = Double.Parse(balanceVM.total_in_cents) / 100,
                UserCount = appUsers.Count,
                TodayUsersCount = newUsersCount,
                TodayRelations = newRelationCount,
                Earned = earned,
                VinCount = vinCount
            };

            return adminHomeVM;
        }
    }
}
