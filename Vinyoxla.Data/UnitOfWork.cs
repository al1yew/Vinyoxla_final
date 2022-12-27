using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Repositories;
using Vinyoxla.Data.Repositories;

namespace Vinyoxla.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppUserRepository _appUserRepository;
        private readonly VinCodeRepository _vinCodeRepository;
        private readonly AppUserToVincodeRepository _appUserToVincodeRepository;
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IAppUserRepository AppUserRepository => _appUserRepository != null ? _appUserRepository : new AppUserRepository(_context);
        public IVinCodeRepository VinCodeRepository => _vinCodeRepository != null ? _vinCodeRepository : new VinCodeRepository(_context);
        public IAppUserToVincodeRepository AppUserToVincodeRepository => _appUserToVincodeRepository != null ? _appUserToVincodeRepository : new AppUserToVincodeRepository(_context);


        public int Commit()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
