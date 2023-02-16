using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.MVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        public ReportController(IReportService reportService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _reportService = reportService;
            _env = env;
            Configuration = configuration;
        }

        public async Task<IActionResult> Index(string fileName)
        {
            ResultVM resultVM = await _reportService.GetUsersReport(fileName);

            if (resultVM == null)
            {
                return RedirectToAction("Error", new { errno = 3 });
            }

            return View(resultVM);
        }

        public async Task<IActionResult> Error(int errno)
        {
            return View(errno);
        }

        //public async Task<VirtualFileResult> Try(string vin)
        //{
        //    string path = Path.Combine(_env.WebRootPath);

        //    string[] folders = { "assets", "files", $"{vin}" };

        //    foreach (string folder in folders)
        //    {
        //        path = Path.Combine(path, folder);
        //    }

        //    Guid guid = Guid.NewGuid();

        //    string fileName = vin + "_" + guid + ".pdf";

        //    //nado otpravit request, potom konvertnut base64

        //    string url = $"https://api.allreports.tools/wp-json/v1/get_report_by_wholesaler/{vin}/{Configuration.GetSection("Api_Key:MyKey").Value}/carfax/en";

        //    HttpResponseMessage response = null;

        //    ResponseVM responseVM = new ResponseVM();

        //    using (HttpClient client = new HttpClient())
        //    {
        //        response = await client.GetAsync(url);
        //    }

        //    if (response.IsSuccessStatusCode)
        //    {
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }

        //        path = Path.Combine(path, fileName);

        //        string convertedResponse = await response.Content.ReadAsStringAsync();

        //        responseVM = JsonConvert.DeserializeObject<ResponseVM>(convertedResponse);

        //        byte[] report = Convert.FromBase64String(responseVM.Report.Report);

        //        await System.IO.File.WriteAllBytesAsync(path, report);
        //    }

        //    return File(path, "application/pdf");
        //}
    }
}
