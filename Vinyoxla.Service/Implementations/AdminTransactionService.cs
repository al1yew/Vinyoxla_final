using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Exceptions;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.TransactionVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AdminTransactionService : IAdminTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminTransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IQueryable<TransactionGetVM>> GetAllAsync(string phone, string orderId, string sessionId)
        {
            List<TransactionGetVM> dbTransactions = _mapper.Map<List<TransactionGetVM>>(await _unitOfWork.TransactionRepository.GetAllAsync("AppUser"));

            IQueryable<TransactionGetVM> query = dbTransactions.AsQueryable();

            if (orderId != null)
            {
                query = query.Where(x => x.OrderId.Contains(orderId.Trim().ToUpperInvariant()));
            }

            if (sessionId != null)
            {
                query = query.Where(x => x.SessionId.Contains(sessionId.Trim().ToUpperInvariant()));
            }

            if (phone != null)
            {
                query = query.Where(x => x.AppUser.UserName.Contains(phone));
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<TransactionGetVM> GetById(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Id is null!");

            Transaction transaction = await _unitOfWork.TransactionRepository.GetAsync(x => x.Id == id, "AppUser");

            if (transaction == null)
                throw new NotFoundException($"Transaction cannot be found by id = {id}");

            return _mapper.Map<TransactionGetVM>(transaction);
        }

        public async Task DeleteAsync(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Id is null!");

            Transaction dbTransaction = await _unitOfWork.TransactionRepository.GetAsync(x => x.Id == id);

            if (dbTransaction == null)
                throw new NotFoundException($"Transaction cannot be found by id = {id}");

            _unitOfWork.TransactionRepository.Remove(dbTransaction);

            await _unitOfWork.CommitAsync();
        }

        public async Task RefundAsync(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Id is null!");

            Transaction dbTransaction = await _unitOfWork.TransactionRepository.GetAsync(x => x.Id == id, "AppUser");

            if (dbTransaction == null)
                throw new NotFoundException($"Transaction cannot be found by id = {id}");

            AppUser appUser = await _unitOfWork.AppUserRepository.GetAsync(x => x.UserName == dbTransaction.AppUser.UserName);

            if (appUser == null)
                throw new NotFoundException($"AppUser cannot be found by Number = {appUser.UserName}");

            appUser.Balance += 4;

            _unitOfWork.TransactionRepository.Remove(dbTransaction);

            await _unitOfWork.CommitAsync();
        }
    }
}
