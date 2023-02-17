using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;

namespace Vinyoxla.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public IActionResult Index()
        {
            HttpContext.Response.Cookies.Delete("topUp");
            HttpContext.Response.Cookies.Delete("cookieInfo");
            return View();
        }

        public async Task<IActionResult> Find(string vinCode)
        {
            return PartialView("_ResultsContainerPartial", await _homeService.Find(vinCode));
        }
    }
}
