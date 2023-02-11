using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.EventVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Manage")]
    public class EventController : Controller
    {
        private readonly IAdminEventService _adminEventService;
        private readonly IMapper _mapper;
        public EventController(IAdminEventService adminEventService, IMapper mapper)
        {
            _adminEventService = adminEventService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int select, string vin, string phone, int page = 1)
        {
            IQueryable<EventGetVM> events = await _adminEventService.GetAllAsync(vin, phone);

            if (select <= 0)
            {
                select = 5;
            }

            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.Vin = vin;
            ViewBag.WhereWeAre = "Events";

            return View(PaginationList<EventGetVM>.Create(events, page, select));
        }

        public async Task<IActionResult> DeleteEvent(int? id, int select, string vin, string phone, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.Vin = vin;
            ViewBag.WhereWeAre = "Events";

            await _adminEventService.DeleteEventAsync(id);

            IQueryable<EventGetVM> events = await _adminEventService.GetAllAsync(vin, phone);

            return PartialView("_EventIndexPartial", PaginationList<EventGetVM>.Create(events, page, select));
        }

        public async Task<IActionResult> DeleteEventMessage(int? id, int select, string vin, string phone, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.Phone = phone;
            ViewBag.Vin = vin;
            ViewBag.WhereWeAre = "Events";

            await _adminEventService.DeleteMessageAsync(id);

            IQueryable<EventGetVM> events = await _adminEventService.GetAllAsync(vin, phone);

            return PartialView("_EventIndexPartial", PaginationList<EventGetVM>.Create(events, page, select));
        }
    }
}
