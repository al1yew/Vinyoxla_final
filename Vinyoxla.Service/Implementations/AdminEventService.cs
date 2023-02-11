using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Exceptions;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.EventVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AdminEventService : IAdminEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminEventService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IQueryable<EventGetVM>> GetAllAsync(string vin, string phone)
        {
            List<EventGetVM> events = _mapper.Map<List<EventGetVM>>(await _unitOfWork.EventRepository.GetAllAsync("AppUser", "EventMessages"));

            IQueryable<EventGetVM> query = events.AsQueryable();

            if (vin != null)
            {
                query = query.Where(x => x.Vin.Contains(vin.Trim().ToUpperInvariant()));
            }

            if (phone != null)
            {
                query = query.Where(x => x.AppUser.UserName.Contains(phone));
            }

            return query;
        }

        public async Task<EventGetVM> GetById(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Id is null!");

            Event eventik = await _unitOfWork.EventRepository.GetAsync(x => x.Id == id, "AppUser", "EventMessages");

            if (eventik == null)
                throw new NotFoundException($"Event cannot be found by id = {id}");

            return _mapper.Map<EventGetVM>(eventik);
        }

        public async Task DeleteEventAsync(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Event cannot be found by id = {id}");

            Event eventik = await _unitOfWork.EventRepository.GetAsync(x => x.Id == id);

            if (eventik == null)
                throw new NotFoundException($"Event cannot be found by id = {id}");

            _unitOfWork.EventRepository.Remove(eventik);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteMessageAsync(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Message cannot be found by id = {id}");

            EventMessage msg = await _unitOfWork.EventMessageRepository.GetAsync(x => x.Id == id);

            if (msg == null)
                throw new NotFoundException($"Message cannot be found by id = {id}");

            _unitOfWork.EventMessageRepository.Remove(msg);
            await _unitOfWork.CommitAsync();
        }
    }
}
