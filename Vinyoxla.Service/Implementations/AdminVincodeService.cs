using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Exceptions;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AdminVincodeService : IAdminVincodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public AdminVincodeService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _env = env;
        }

        public async Task<IQueryable<VinCodeGetVM>> GetAllAsync(string vin)
        {
            List<VinCodeGetVM> dbList = _mapper.Map<List<VinCodeGetVM>>(await _unitOfWork.VinCodeRepository.GetAllAsync());

            IQueryable<VinCodeGetVM> query = dbList.AsQueryable();

            if (vin != null)
            {
                query = query.Where(x => x.Vin.Contains(vin.Trim().ToUpperInvariant()));
            }

            return query;
        }

        public async Task DeleteAsync(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Vincode cannot be found by id = {id}");

            VinCode dbVin = await _unitOfWork.VinCodeRepository.GetAsync(x => x.Id == id);

            List<AppUserToVincode> relations = await _unitOfWork.AppUserToVincodeRepository.GetAllByExAsync(x => x.VinCodeId == id);

            List<Event> events = await _unitOfWork.EventRepository.GetAllByExAsync(x => x.Vin == dbVin.Vin);

            foreach (var relation in relations)
            {
                _unitOfWork.AppUserToVincodeRepository.Remove(relation);
            }

            _unitOfWork.VinCodeRepository.Remove(dbVin);

            foreach (var eventik in events)
            {
                eventik.EventMessages = new List<EventMessage>()
                {
                    new EventMessage()
                    {
                        Message = "Bu vinkodu ozumuz databazadan silmishik, ona gore userlerin relationu silinib, " +
                        "papkadan fayl silinib, papka qalib, Eventler galib.",
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    }
                };
            }

            await _unitOfWork.CommitAsync();

            string path = Path.Combine(_env.WebRootPath);

            string[] folders = { "assets", "files", $"{dbVin.Vin}" };

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            path = Path.Combine(path, dbVin.FileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
