using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Implementations
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IWebHostEnvironment _env;
        private IConfiguration Configuration { get; }
        private readonly IUnitOfWork _unitOfWork;
        public PurchaseService(IWebHostEnvironment env, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _env = env;
            Configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public PurchaseVM GetViewModel(SelectedReportVM selectedReportVM)
        {
            PurchaseVM purchaseVM = new PurchaseVM()
            {
                SelectedReportVM = selectedReportVM,
                CardVM = new CardVM()
                {
                    Vin = selectedReportVM.Vin
                }
            };

            return purchaseVM;
        }

        public async Task<ResultVM> GetReport(string vinCode)
        {
            //pathimizi yaratdig, combine etdik.
            string path = Path.Combine(_env.WebRootPath);

            string[] folders = { "assets", "files", $"{vinCode}" };

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            //alinan pdf bizde varsa onu qaytaririg
            if (await _unitOfWork.VinCodeRepository.IsExistAsync(x => x.Vin == vinCode.ToUpperInvariant()))
            {
                VinCode vin = await _unitOfWork.VinCodeRepository.GetAsync(x => x.Vin == vinCode);

                string reportHTML = "";

                path = Path.Combine(path, vin.FileName);

                reportHTML = await System.IO.File.ReadAllTextAsync(path);

                vin.PurchasedTimes = vin.PurchasedTimes + 1;
                await _unitOfWork.CommitAsync();

                ResultVM resultVM = new ResultVM()
                {
                    FileName = vin.FileName,
                    HTML = reportHTML,
                    Vin = vinCode
                };

                return resultVM;
            }

            //alinan pdf bizde yoxdusa, alirig
            string url = $"https://api.allreports.tools/wp-json/v1/get_report_by_wholesaler/{vinCode}/{Configuration.GetSection("Api_Key:MyKey").Value}/carfax/en";

            HttpResponseMessage response = null;

            ResponseVM responseVM = new ResponseVM();

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync(url);
            }

            if (response.IsSuccessStatusCode)
            {
                string convertedResponse = await response.Content.ReadAsStringAsync();

                responseVM = JsonConvert.DeserializeObject<ResponseVM>(convertedResponse);

                byte[] report = Convert.FromBase64String(responseVM.Report.Report);

                string responseHTML = Encoding.UTF8.GetString(report);

                string fileName = vinCode + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".html";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, fileName);

                await System.IO.File.WriteAllTextAsync(path, responseHTML);

                VinCode vin = new VinCode()
                {
                    Vin = vinCode,
                    FileName = fileName,
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                    PurchasedTimes = 1
                };

                await _unitOfWork.VinCodeRepository.AddAsync(vin);
                await _unitOfWork.CommitAsync();

                ResultVM resultVM = new ResultVM()
                {
                    Vin = vinCode,
                    FileName = fileName,
                    HTML = responseHTML
                };

                return resultVM;
            }

            return null;
        }

        public async Task<bool> UserPurchase(CardVM cardVM)
        {
            DateTime cardDate;

            if (cardVM.Month > 12)
            {
                return false;
            }

            if (!DateTime.TryParse("01" + "/" + cardVM.Month + "/" + cardVM.CardYear, out cardDate))
            {
                return false;
            }
            else
            {
                cardDate = DateTime.Parse("01" + "/" + cardVM.Month + "/" + cardVM.CardYear);
            }

            Regex regexString = new Regex(@"^[a-zA-Z\s]*$");

            if (!regexString.IsMatch(cardVM.CardHolder))
            {
                return false;
            }

            Regex regexNumber = new Regex("^[0-9]+$");

            if (!regexNumber.IsMatch(cardVM.CardYear.ToString()) &&
                !regexNumber.IsMatch(cardVM.CardNo.ToString()) &&
                !regexNumber.IsMatch(cardVM.CVV.ToString()) &&
                !regexNumber.IsMatch(cardVM.Month.ToString()))
            {
                return false;
            }

            if (regexNumber.IsMatch(cardVM.PhoneNumber.ToString()))
            {
                if (!(cardVM.PhoneNumber.StartsWith("50") ||
                    cardVM.PhoneNumber.StartsWith("10") ||
                    cardVM.PhoneNumber.StartsWith("51") ||
                    cardVM.PhoneNumber.StartsWith("70") ||
                    cardVM.PhoneNumber.StartsWith("77") ||
                    cardVM.PhoneNumber.StartsWith("99") ||
                    cardVM.PhoneNumber.StartsWith("55")))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (!(cardVM.CardNo.StartsWith("2") ||
                cardVM.CardNo.StartsWith("3") ||
                cardVM.CardNo.StartsWith("4") ||
                cardVM.CardNo.StartsWith("5")))
            {
                return false;
            }

            if (cardDate < DateTime.Now)
            {
                return false;
            }

            //bank api

            return true;
        }
    }
}
