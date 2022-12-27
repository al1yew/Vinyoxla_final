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
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Implementations
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IWebHostEnvironment _env;
        private IConfiguration Configuration { get; }
        public PurchaseService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            Configuration = configuration;
        }

        public PurchaseVM GetViewModel(SelectedReportVM selectedReportVM)
        {
            PurchaseVM purchaseVM = new PurchaseVM()
            {
                SelectedReportVM = selectedReportVM,
                CardVM = new CardVM()
                {
                    Type = selectedReportVM.Type,
                    Vin = selectedReportVM.Vin
                }
            };

            return purchaseVM;
        }

        public async Task<string> GetReport(string vinCode, int reportType)
        {
            string type = reportType == 1 ? "carfax" : "autocheck";

            string url = $"https://api.allreports.tools/wp-json/v1/get_report_by_wholesaler/{vinCode}/{Configuration.GetSection("Api_Key:MyKey").Value}/{type}/en";

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

                string path = Path.Combine(_env.WebRootPath);

                string[] folders = { "assets", "files", $"{vinCode}", $"{type}" };

                foreach (string folder in folders)
                {
                    path = Path.Combine(path, folder);
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, fileName);

                System.IO.File.WriteAllText(path, responseHTML);

                //soxranenie v papku tormozit vse, ili nayti druqoy metod soxraneniya, ili je
                //uje ispolzovat iframe

                return responseHTML;
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
