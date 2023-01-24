using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Manage")]
    public class RelationController : Controller
    {
        private readonly IAdminRelationService _adminRelationService;

        public RelationController(IAdminRelationService adminRelationService)
        {
            _adminRelationService = adminRelationService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
