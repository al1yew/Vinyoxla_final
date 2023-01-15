using AutoMapper;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;
using Vinyoxla.Service.ViewModels.VinCodeVMs;

namespace Vinyoxla.Service.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region AppUser

            CreateMap<AppUser, AppUserGetVM>()
                .ForPath(des => des.AppUserToVincodes, src => src.MapFrom(x => x.AppUserToVincodes))
                .ForPath(des => des.Events, src => src.MapFrom(x => x.Events))
                .ForPath(des => des.Transactions, src => src.MapFrom(x => x.Transactions));


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

        }
    }
}
