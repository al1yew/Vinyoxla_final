using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.TransactionVMs;

namespace Vinyoxla.MVC.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class TransactionController : Controller
    {
        private readonly IAdminTransactionService _adminTransactionService;
        private readonly IMapper _mapper;

        public TransactionController(IAdminTransactionService adminTransactionService, IMapper mapper)
        {
            _adminTransactionService = adminTransactionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int select, string orderId, string sessionId, string phone, int page = 1)
        {
            IQueryable<TransactionGetVM> transactions = await _adminTransactionService.GetAllAsync(phone, orderId, sessionId);

            if (select <= 0)
            {
                select = 10;
            }

            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.SessionId = sessionId;
            ViewBag.OrderId = orderId;
            ViewBag.Phone = phone;
            ViewBag.WhereWeAre = "Transactions";

            return View(PaginationList<TransactionGetVM>.Create(transactions, page, select));
        }

        public async Task<IActionResult> Delete(int? id, int select, string orderId, string sessionId, string phone, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.SessionId = sessionId;
            ViewBag.OrderId = orderId;
            ViewBag.Phone = phone;
            ViewBag.WhereWeAre = "Transactions";

            await _adminTransactionService.DeleteAsync(id);

            IQueryable<TransactionGetVM> transactions = await _adminTransactionService.GetAllAsync(phone, orderId, sessionId);

            return PartialView("_TransactionIndexPartial", PaginationList<TransactionGetVM>.Create(transactions, page, select));
        }

        public async Task<IActionResult> Refund(int? id, int select, string orderId, string sessionId, string phone, int page)
        {
            ViewBag.Select = select;
            ViewBag.Page = page;
            ViewBag.SessionId = sessionId;
            ViewBag.OrderId = orderId;
            ViewBag.Phone = phone;
            ViewBag.WhereWeAre = "Transactions";

            await _adminTransactionService.RefundAsync(id);

            IQueryable<TransactionGetVM> transactions = await _adminTransactionService.GetAllAsync(phone, orderId, sessionId);

            return PartialView("_TransactionIndexPartial", PaginationList<TransactionGetVM>.Create(transactions, page, select));
        }
    }
}
