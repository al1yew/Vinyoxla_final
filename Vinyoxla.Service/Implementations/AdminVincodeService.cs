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

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task DeleteAsync(int? id)
        {
            if (id == null)
                throw new NotFoundException($"Id is null!");

            VinCode dbVin = await _unitOfWork.VinCodeRepository.GetAsync(x => x.Id == id);

            List<AppUserToVincode> relations = await _unitOfWork.AppUserToVincodeRepository.GetAllByExAsync(x => x.VinCodeId == id);

            List<Event> events = await _unitOfWork.EventRepository.GetAllByExAsync(x => x.Vin == dbVin.Vin, "EventMessages");

            foreach (var relation in relations)
            {
                _unitOfWork.AppUserToVincodeRepository.Remove(relation);
            }

            _unitOfWork.VinCodeRepository.Remove(dbVin);

            foreach (var eventik in events)
            {
                if (eventik.EventMessages.Count > 0)
                {
                    eventik.EventMessages.Add(new EventMessage()
                    {
                        Message = "Bu vinkodu ozumuz databazadan silmishik, ona gore userlerin relationu silinib, " +
                        "papkadan fayl silinib, papka qalib, Eventler galib.",
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    });
                }
                else
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

        public async Task<CheckVM> Check()
        {
            List<VinCodeGetVM> dbVincodes = _mapper.Map<List<VinCodeGetVM>>
                (await _unitOfWork.VinCodeRepository.GetAllAsync());

            CheckVM checkVM = new CheckVM()
            {
                AbsentCount = 0,
                AbsentVincodes = new List<VinCodeGetVM>(),
                OldCount = 0,
                OldVincodes = new List<VinCodeGetVM>()
            };

            foreach (VinCodeGetVM dbVin in dbVincodes)
            {
                if ((DateTime.Now - dbVin.CreatedAt.Value).TotalDays >= 7)
                {
                    checkVM.OldVincodes.Add(dbVin);
                    checkVM.OldCount++;
                }

                if (!await FileExists(dbVin.Vin, dbVin.FileName))
                {
                    checkVM.AbsentCount++;
                    checkVM.AbsentVincodes.Add(dbVin);
                }
            }

            return checkVM;
        }

        public async Task<bool> FileExists(string vin, string fileName)
        {
            string path = Path.Combine(_env.WebRootPath);

            string[] folders = { "assets", "files", $"{vin}" };

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                return false;
            }

            return true;
        }
    }
}
