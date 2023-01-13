using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IWebHostEnvironment _env;

        public ReportService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<ResultVM> GetUsersReport(string fileName)
        {
            string vin = fileName.Substring(0, 17);

            string path = Path.Combine(_env.WebRootPath);

            string[] folders = { "assets", "files", $"{vin}" };

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                return null;
            }

            string reportHTML = await File.ReadAllTextAsync(path);

            ResultVM resultVM = new ResultVM()
            {
                FileName = fileName,
                HTML = reportHTML,
                Vin = vin
            };

            return resultVM;
        }
    }
}
