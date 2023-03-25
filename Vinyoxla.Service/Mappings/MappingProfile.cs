using AutoMapper;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;
using Vinyoxla.Service.ViewModels.EventMessageVMs;
using Vinyoxla.Service.ViewModels.EventVMs;
using Vinyoxla.Service.ViewModels.TransactionVMs;
using Vinyoxla.Service.ViewModels.UserVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region AppUser

            CreateMap<AppUser, AppUserListVM>();

            CreateMap<AppUser, AppUserGetVM>()
                .ForPath(des => des.AppUserToVincodes, src => src.MapFrom(x => x.AppUserToVincodes))
                .ForPath(des => des.Events, src => src.MapFrom(x => x.Events))
                .ForPath(des => des.Transactions, src => src.MapFrom(x => x.Transactions));

            CreateMap<AppUserGetVM, AppUserUpdateVM>()
                .ForMember(des => des.PhoneNumber, src => src.MapFrom(x => x.PhoneNumber.Substring(4)));

            #endregion

            #region AppUserToVinCode

            CreateMap<AppUserToVincode, AppUserToVincodeVM>()
              .ForPath(des => des.VinCode, src => src.MapFrom(x => x.VinCode))
              .ForPath(des => des.AppUser, src => src.MapFrom(x => x.AppUser));

            #endregion

            #region VinCode

            CreateMap<VinCode, VinCodeGetVM>()
                .ForPath(des => des.AppUserToVincodes, src => src.MapFrom(x => x.AppUserToVincodes));

            #endregion

            #region Events

            CreateMap<Event, EventGetVM>()
                .ForPath(des => des.AppUser, src => src.MapFrom(x => x.AppUser))
                .ForPath(des => des.EventMessages, src => src.MapFrom(x => x.EventMessages));

            #endregion

            #region Transactions

            CreateMap<Transaction, TransactionGetVM>()
                .ForPath(des => des.AppUser, src => src.MapFrom(x => x.AppUser));

            #endregion

            #region Event Messages

            CreateMap<EventMessage, EventMessageGetVM>()
                .ForPath(des => des.Event, src => src.MapFrom(x => x.Event));

            #endregion
        }
    }
}
