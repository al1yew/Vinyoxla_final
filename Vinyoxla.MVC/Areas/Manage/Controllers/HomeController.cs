using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IAdminHomeService _adminHomeService;

        public HomeController(IAdminHomeService adminHomeService)
        {
            _adminHomeService = adminHomeService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.WhereWeAre = "Main Page";

            return View(await _adminHomeService.GetData());
        }
    }
}
