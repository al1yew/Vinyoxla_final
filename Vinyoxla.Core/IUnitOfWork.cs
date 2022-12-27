using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Core.Repositories;

namespace Vinyoxla.Core
{
    public interface IUnitOfWork
    {
        IAppUserRepository AppUserRepository { get; }
        IVinCodeRepository VinCodeRepository { get; }
        IAppUserToVincodeRepository AppUserToVincodeRepository { get; }
        Task<int> CommitAsync();
        int Commit();
    }
}
