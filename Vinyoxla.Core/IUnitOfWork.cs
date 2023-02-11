using System.Threading.Tasks;
using Vinyoxla.Core.Repositories;

namespace Vinyoxla.Core
{
    public interface IUnitOfWork
    {
        IAppUserRepository AppUserRepository { get; }
        IVinCodeRepository VinCodeRepository { get; }
        IAppUserToVincodeRepository AppUserToVincodeRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        IEventRepository EventRepository { get; }
        IEventMessageRepository EventMessageRepository { get; }
        Task<int> CommitAsync();
        int Commit();
    }
}
