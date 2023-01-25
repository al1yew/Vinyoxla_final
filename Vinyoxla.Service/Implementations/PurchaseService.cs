using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels.PurchaseVMs;

namespace Vinyoxla.Service.Implementations
{
    public class PurchaseService : IPurchaseService
    {
        private IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public PurchaseService(IWebHostEnvironment env, IConfiguration configuration, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, IMapper mapper)
        {
            _env = env;
            Configuration = configuration;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<PurchaseVM> GetViewModelForOrderPage(SelectedReportVM selectedReportVM)
        {
            PurchaseVM purchaseVM = new PurchaseVM()
            {
                SelectedReportVM = selectedReportVM,
                OrderVM = new OrderVM()
                {
                    Vin = selectedReportVM.Vin,
                    PhoneNumber = _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated ? await GetUserPhoneNumber() : null
                }
            };

            return purchaseVM;
        }

        public async Task<string> CheckEverything(string phone, string vin)
        {
            //postaraysa sverit mojet tut ne nado daje proverat user online ili net, kjc eyni shey yazmisham e

            VinCode dbVin = await _unitOfWork.VinCodeRepository.GetAsync(x => x.Vin == vin.Trim().ToUpperInvariant());

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                if (dbVin != null)
                {
                    if (await _unitOfWork.AppUserToVincodeRepository.IsExistAsync(x => x.AppUser.UserName == "+994" + phone && x.VinCode.Vin == vin.Trim().ToUpperInvariant()))
                    {
                        Event userEvent = await _unitOfWork.EventRepository.GetAsync(x => x.AppUser.UserName == "+994" + phone && x.Vin == dbVin.Vin, "EventMessages");

                        if (!((DateTime.Now - dbVin.CreatedAt.Value).TotalDays >= 7))
                        {
                            if (await FileExists(dbVin.FileName))
                            {
                                await Refund(phone);

                                #region Event handle

                                userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                userEvent.DidRefundToBalance = true;

                                userEvent.IsApiError = false;
                                userEvent.FileExists = true;
                                userEvent.IsFromApi = false;
                                userEvent.IsRenewedDueToExpire = false;
                                userEvent.IsRenewedDueToAbsence = false;
                                userEvent.ErrorWhileRenew = false;
                                userEvent.ErrorWhileReplace = false;

                                userEvent.EventMessages.Add(new EventMessage()
                                {
                                    Message = "User oz vinkodunu yeniden axtardi, tapdi, yeniden baxir. " +
                                    "Oz kabinetinnen de baxa bilerdi, amma nebilim e, burdan axtardi, pulu yene odedi deye eyni sheye gore, qaytardim",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                });

                                await _unitOfWork.CommitAsync();

                                #endregion

                                return dbVin.FileName;
                            }
                            else
                            {
                                if (await TryToFixAbsence(dbVin.Vin, dbVin.FileName))
                                {
                                    await Refund(phone);

                                    #region Event handle

                                    userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                    userEvent.DidRefundToBalance = true;

                                    userEvent.IsApiError = false;
                                    userEvent.FileExists = true;
                                    userEvent.IsFromApi = true;
                                    userEvent.IsRenewedDueToExpire = false;
                                    userEvent.IsRenewedDueToAbsence = true;
                                    userEvent.ErrorWhileRenew = false;
                                    userEvent.ErrorWhileReplace = false;

                                    userEvent.EventMessages.Add(new EventMessage()
                                    {
                                        Message = "User oz vinkodunu yeniden axtardi, fayl tapilmadi. " +
                                        "Mecburam yeniden alim onu, cunki pul odemishdi. Aldim, qaytardim, ve ikinci defe odediyi pulu qaytardim",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    });

                                    #endregion

                                    dbVin.CreatedAt = DateTime.UtcNow.AddHours(4);

                                    await _unitOfWork.CommitAsync();

                                    return dbVin.FileName;
                                }
                                else
                                {
                                    await Refund(phone);
                                    await Refund(phone);

                                    AppUserToVincode appUserToVincode = await _unitOfWork.AppUserToVincodeRepository.GetAsync(x =>
                                    x.AppUser.UserName == "+994" + phone && x.VinCode.Vin == dbVin.Vin);

                                    _unitOfWork.AppUserToVincodeRepository.Remove(appUserToVincode);

                                    #region Event handle

                                    userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                    userEvent.DidRefundToBalance = true;

                                    userEvent.IsApiError = true;
                                    userEvent.FileExists = false;
                                    userEvent.IsFromApi = true;
                                    userEvent.IsRenewedDueToExpire = false;
                                    userEvent.IsRenewedDueToAbsence = false;
                                    userEvent.ErrorWhileRenew = false;
                                    userEvent.ErrorWhileReplace = true;

                                    userEvent.EventMessages.Add(new EventMessage()
                                    {
                                        Message = "Userin vinkodunu gaytarmag isteyende gorduk ki " +
                                        "report folderde yoxa cixib. Ona gore pulun qaytariram, ozude dvoynoy refund 8 manat (birinci defe ve indiki defe ucun), " +
                                        "bu vinkod daha onun deyil. Negeder alir alsin error verenecen relation olamayacag",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    });

                                    #endregion

                                    await _unitOfWork.CommitAsync();

                                    return "0";
                                }
                            }
                        }
                        else
                        {
                            return "1";
                        }
                    }
                }

                return "2";
            }
            else
            {
                if (dbVin != null)
                {
                    if (await _unitOfWork.AppUserToVincodeRepository.IsExistAsync(x => x.AppUser.UserName == "+994" + phone && x.VinCode.Vin == vin.Trim().ToUpperInvariant()))
                    {
                        Event userEvent = await _unitOfWork.EventRepository.GetAsync(x => x.AppUser.UserName == "+994" + phone && x.Vin == dbVin.Vin, "EventMessages");

                        if (!((DateTime.Now - dbVin.CreatedAt.Value).TotalDays >= 7))
                        {
                            if (await FileExists(dbVin.FileName))
                            {
                                await Refund(phone);

                                #region Event handle

                                userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                userEvent.DidRefundToBalance = true;

                                userEvent.IsApiError = false;
                                userEvent.FileExists = true;
                                userEvent.IsFromApi = false;
                                userEvent.IsRenewedDueToExpire = false;
                                userEvent.IsRenewedDueToAbsence = false;
                                userEvent.ErrorWhileRenew = false;
                                userEvent.ErrorWhileReplace = false;

                                userEvent.EventMessages.Add(new EventMessage()
                                {
                                    Message = "User oz vinkodunu yeniden axtardi, tapdi, yeniden baxir. " +
                                    "Oz kabinetinnen de baxa bilerdi, amma nebilim e, burdan axtardi, pulu yene odedi deye eyni sheye gore, qaytardim",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                });

                                await _unitOfWork.CommitAsync();

                                #endregion

                                return dbVin.FileName;
                            }
                            else
                            {
                                if (await TryToFixAbsence(dbVin.Vin, dbVin.FileName))
                                {
                                    await Refund(phone);

                                    #region Event handle

                                    userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                    userEvent.DidRefundToBalance = true;

                                    userEvent.IsApiError = false;
                                    userEvent.FileExists = true;
                                    userEvent.IsFromApi = true;
                                    userEvent.IsRenewedDueToExpire = false;
                                    userEvent.IsRenewedDueToAbsence = true;
                                    userEvent.ErrorWhileRenew = false;
                                    userEvent.ErrorWhileReplace = false;

                                    userEvent.EventMessages.Add(new EventMessage()
                                    {
                                        Message = "User oz vinkodunu yeniden axtardi, fayl tapilmadi. " +
                                        "Mecburam yeniden alim onu, cunki pul odemishdi. Aldim, qaytardim, ve ikinci defe odediyi pulu qaytardim",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    });

                                    #endregion

                                    dbVin.CreatedAt = DateTime.UtcNow.AddHours(4);

                                    await _unitOfWork.CommitAsync();

                                    return dbVin.FileName;
                                }
                                else
                                {
                                    await Refund(phone);
                                    await Refund(phone);

                                    AppUserToVincode appUserToVincode = await _unitOfWork.AppUserToVincodeRepository.GetAsync(x =>
                                    x.AppUser.UserName == "+994" + phone && x.VinCode.Vin == dbVin.Vin);

                                    _unitOfWork.AppUserToVincodeRepository.Remove(appUserToVincode);

                                    #region Event handle

                                    userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                    userEvent.DidRefundToBalance = true;

                                    userEvent.IsApiError = true;
                                    userEvent.FileExists = false;
                                    userEvent.IsFromApi = true;
                                    userEvent.IsRenewedDueToExpire = false;
                                    userEvent.IsRenewedDueToAbsence = false;
                                    userEvent.ErrorWhileRenew = false;
                                    userEvent.ErrorWhileReplace = true;

                                    userEvent.EventMessages.Add(new EventMessage()
                                    {
                                        Message = "Userin vinkodunu gaytarmag isteyende gorduk ki " +
                                        "report folderde yoxa cixib. Ona gore pulun qaytariram, ozude dvoynoy refund 8 manat (birinci defe ve indiki defe ucun), " +
                                        "bu vinkod daha onun deyil. Negeder alir alsin error verenecen relation olamayacag",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    });

                                    #endregion

                                    await _unitOfWork.CommitAsync();

                                    return "0";
                                }
                            }
                        }
                        else
                        {
                            return "1";
                        }
                    }
                }

                return "2";
            }
        }

        public async Task<bool> FileExists(string fileName)
        {
            string vinCode = fileName.Substring(0, 17);

            #region path

            string path = Path.Combine(_env.WebRootPath);

            string[] folders = { "assets", "files", $"{vinCode}" };

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            path = Path.Combine(path, fileName);

            #endregion

            if (File.Exists(path)) return true;

            return false;
        }

        public async Task<string> ReplaceOldReport(string phone, string vin, bool isFromBalance)
        {
            VinCode vinCode = await _unitOfWork.VinCodeRepository.GetAsync(x => x.Vin == vin.Trim().ToUpperInvariant());

            Event userEvent = await _unitOfWork.EventRepository.GetAsync(x => x.AppUser.UserName == "+994" + phone && x.Vin == vinCode.Vin, "EventMessages");

            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == "+994" + phone);

            AppUser newUser = new AppUser()
            {
                UserName = "+994" + phone,
                PhoneNumber = "+994" + phone,
                PhoneNumberConfirmed = false,
                Balance = 0,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow.AddHours(4)
            };

            if (appUser == null)
            {
                await _userManager.CreateAsync(newUser);
                await _userManager.AddToRoleAsync(newUser, "Member");
            }

            if (await BuyReport(vinCode.Vin, vinCode.FileName))
            {
                #region Event handle

                if (userEvent == null)
                {
                    Event newUserEvent = new Event()
                    {
                        AppUser = appUser ?? newUser,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        DidRefundToBalance = false,
                        ErrorWhileRenew = false,
                        ErrorWhileReplace = false,
                        FileExists = true,
                        IsApiError = false,
                        IsFromApi = true,
                        IsRenewedDueToAbsence = false,
                        IsRenewedDueToExpire = true,
                        Vin = vin,
                        EventMessages = new List<EventMessage>()
                        {
                             new EventMessage()
                             {
                                 Message = "User onda olan vinkodu yeniden alir, reportun 7si cixib, ona gore tezeden alirig",
                                 CreatedAt = DateTime.UtcNow.AddHours(4)
                             }
                        }
                    };

                    await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                }
                else
                {
                    userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                    userEvent.DidRefundToBalance = false;
                    userEvent.IsApiError = false;
                    userEvent.FileExists = true;
                    userEvent.IsFromApi = true;
                    userEvent.IsRenewedDueToExpire = true;
                    userEvent.IsRenewedDueToAbsence = false;
                    userEvent.ErrorWhileRenew = false;
                    userEvent.ErrorWhileReplace = false;
                    userEvent.EventMessages.Add(new EventMessage()
                    {
                        Message = "User onda olan vinkodu yeniden alir, reportun 7si cixib, ona gore tezeden alirig",
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    });
                }

                #endregion

                #region Transaction

                Transaction transaction = new Transaction()
                {
                    Amount = 4,
                    AppUser = appUser,
                    Code = "nese",
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                    IsFromBalance = isFromBalance,
                    IsTopUp = false,
                    PaymentIsSuccessful = true
                };

                await _unitOfWork.TransactionRepository.AddAsync(transaction);

                #endregion

                vinCode.CreatedAt = DateTime.UtcNow.AddHours(4);

                await _unitOfWork.CommitAsync();

                return vinCode.FileName;
            }

            await Refund(phone);

            #region Event handle

            if (userEvent == null)
            {
                Event newUserEvent = new Event()
                {
                    AppUser = appUser ?? newUser,
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                    IsFromApi = true,
                    IsApiError = true,
                    DidRefundToBalance = true,
                    ErrorWhileRenew = true,
                    ErrorWhileReplace = false,
                    FileExists = true,
                    IsRenewedDueToAbsence = false,
                    IsRenewedDueToExpire = false,
                    Vin = vin,
                    EventMessages = new List<EventMessage>()
                        {
                             new EventMessage()
                             {
                                 Message = "User kohne reportunu yenilemek istedi, api error verdi, ona gore refund edirik cunki odenish edib indice. " +
                                 "Kohne report var amma assets de. Axtarsin, alsin, error olsa refundunu alsin, negeder isteyir.",
                                 CreatedAt = DateTime.UtcNow.AddHours(4)
                             }
                        }
                };

                await _unitOfWork.EventRepository.AddAsync(newUserEvent);
            }
            else
            {
                userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                userEvent.DidRefundToBalance = true;
                userEvent.IsApiError = true;
                userEvent.FileExists = true;
                userEvent.IsFromApi = true;
                userEvent.IsRenewedDueToExpire = false;
                userEvent.IsRenewedDueToAbsence = false;
                userEvent.ErrorWhileRenew = true;
                userEvent.ErrorWhileReplace = false;
                userEvent.EventMessages.Add(new EventMessage()
                {
                    Message = "User kohne reportunu yenilemek istedi, api error verdi, ona gore refund edirik cunki odenish edib indice. " +
                    "Kohne report var amma assets de. Axtarsin, alsin, error olsa refundunu alsin, negeder isteyir.",
                    CreatedAt = DateTime.UtcNow.AddHours(4)
                });
            }

            #endregion

            await _unitOfWork.CommitAsync();

            return null;
        }

        public async Task<string> GetReport(string phone, string vin, bool isFromBalance)
        {
            VinCode dbVin = await _unitOfWork.VinCodeRepository.GetAsync(x => x.Vin == vin.ToUpperInvariant().Trim());

            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == "+994" + phone);

            Event userEvent = await _unitOfWork.EventRepository.GetAsync(x => x.AppUser.UserName == "+994" + phone && x.Vin == vin.Trim().ToUpperInvariant(), "EventMessages");

            AppUser newUser = new AppUser()
            {
                UserName = "+994" + phone,
                PhoneNumber = "+994" + phone,
                PhoneNumberConfirmed = false,
                Balance = 0,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow.AddHours(4)
            };

            if (appUser == null)
            {
                await _userManager.CreateAsync(newUser);
                await _userManager.AddToRoleAsync(newUser, "Member");
            }

            Transaction transaction = new Transaction()
            {
                AppUser = appUser ?? newUser,
                Amount = 4,
                PaymentIsSuccessful = true,
                CreatedAt = DateTime.UtcNow.AddHours(4),
                Code = "nese",
                IsTopUp = false,
                IsFromBalance = isFromBalance
            };

            if (dbVin != null)
            {
                AppUserToVincode appUserToVincode = new AppUserToVincode()
                {
                    AppUser = appUser ?? newUser,
                    VinCodeId = dbVin.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(4)
                };

                if (!((DateTime.Now - dbVin.CreatedAt.Value).TotalDays >= 7))
                {
                    if (await FileExists(dbVin.FileName))
                    {
                        dbVin.PurchasedTimes++;

                        #region Event handle

                        if (userEvent == null)
                        {
                            Event newUserEvent = new Event()
                            {
                                AppUser = appUser ?? newUser,
                                CreatedAt = DateTime.UtcNow.AddHours(4),
                                DidRefundToBalance = false,
                                ErrorWhileRenew = false,
                                ErrorWhileReplace = false,
                                FileExists = true,
                                IsApiError = false,
                                IsFromApi = false,
                                IsRenewedDueToAbsence = false,
                                IsRenewedDueToExpire = false,
                                Vin = dbVin.Vin,
                                EventMessages = new List<EventMessage>()
                                {
                                    new EventMessage()
                                    {
                                        Message = "User bazadan papkada olan, kohne olmayan report ile relation qurdu.",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    }
                                }
                            };

                            await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                        }
                        else
                        {
                            userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                            userEvent.DidRefundToBalance = false;
                            userEvent.ErrorWhileRenew = false;
                            userEvent.ErrorWhileReplace = false;
                            userEvent.FileExists = true;
                            userEvent.IsApiError = false;
                            userEvent.IsFromApi = false;
                            userEvent.IsRenewedDueToAbsence = false;
                            userEvent.IsRenewedDueToExpire = false;
                            userEvent.EventMessages = new List<EventMessage>()
                            {
                                new EventMessage()
                                {
                                    Message = "User bazadan papkada olan, kohne olmayan report ile relation qurdu.",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                }
                            };
                        }

                        #endregion

                        await _unitOfWork.TransactionRepository.AddAsync(transaction);
                        await _unitOfWork.AppUserToVincodeRepository.AddAsync(appUserToVincode);
                        await _unitOfWork.CommitAsync();

                        return dbVin.FileName;
                    }
                    else
                    {
                        if (await TryToFixAbsence(dbVin.Vin, dbVin.FileName))
                        {
                            dbVin.PurchasedTimes++;
                            dbVin.CreatedAt = DateTime.UtcNow.AddHours(4);

                            #region Event handle

                            if (userEvent == null)
                            {
                                Event newUserEvent = new Event()
                                {
                                    AppUser = appUser ?? newUser,
                                    CreatedAt = DateTime.UtcNow.AddHours(4),
                                    DidRefundToBalance = false,
                                    ErrorWhileRenew = false,
                                    ErrorWhileReplace = false,
                                    FileExists = true,
                                    IsApiError = false,
                                    IsFromApi = true,
                                    IsRenewedDueToAbsence = true,
                                    IsRenewedDueToExpire = false,
                                    Vin = dbVin.Vin,
                                    EventMessages = new List<EventMessage>()
                                    {
                                        new EventMessage()
                                        {
                                            Message = "User bazadan papkada olmayan, kohne olmayan report ile relation qurdu. " +
                                            "Pul odeyib deye yari yolda goymadig, getdik aldig reportu",
                                            CreatedAt = DateTime.UtcNow.AddHours(4)
                                        }
                                    }
                                };

                                await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                            }
                            else
                            {
                                userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                userEvent.DidRefundToBalance = false;
                                userEvent.ErrorWhileRenew = false;
                                userEvent.ErrorWhileReplace = false;
                                userEvent.FileExists = true;
                                userEvent.IsApiError = false;
                                userEvent.IsFromApi = true;
                                userEvent.IsRenewedDueToAbsence = true;
                                userEvent.IsRenewedDueToExpire = false;
                                userEvent.EventMessages = new List<EventMessage>()
                                {
                                    new EventMessage()
                                    {
                                        Message = "User bazadan papkada olmayan, kohne olmayan report ile relation qurdu. " +
                                        "Pul odeyib deye yari yolda goymadig, getdik aldig reportu",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    }
                                };
                            }

                            #endregion

                            await _unitOfWork.TransactionRepository.AddAsync(transaction);
                            await _unitOfWork.AppUserToVincodeRepository.AddAsync(appUserToVincode);
                            await _unitOfWork.CommitAsync();

                            return dbVin.FileName;
                        }
                        else
                        {
                            await Refund(phone);

                            #region Event handle

                            if (userEvent == null)
                            {
                                Event newUserEvent = new Event()
                                {
                                    AppUser = appUser ?? newUser,
                                    CreatedAt = DateTime.UtcNow.AddHours(4),
                                    DidRefundToBalance = true,
                                    ErrorWhileRenew = false,
                                    ErrorWhileReplace = true,
                                    FileExists = false,
                                    IsApiError = true,
                                    IsFromApi = true,
                                    IsRenewedDueToAbsence = false,
                                    IsRenewedDueToExpire = false,
                                    EventMessages = new List<EventMessage>()
                                    {
                                        new EventMessage()
                                        {
                                            Message = "User bazada olan, sveji olan, papkada olmayan reportu almag istedi, " +
                                            "amma biz o reportu yenisi ile evez ede bilmedik, api error verdi. " +
                                            "Ona gore event yarandi, relation ise yox. Pulunu da qaytardig",
                                            CreatedAt = DateTime.UtcNow.AddHours(4)
                                        }
                                    }
                                };

                                await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                            }
                            else
                            {
                                userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                                userEvent.DidRefundToBalance = true;
                                userEvent.ErrorWhileRenew = false;
                                userEvent.ErrorWhileReplace = true;
                                userEvent.FileExists = false;
                                userEvent.IsApiError = true;
                                userEvent.IsFromApi = true;
                                userEvent.IsRenewedDueToAbsence = false;
                                userEvent.IsRenewedDueToExpire = false;
                                userEvent.EventMessages = new List<EventMessage>()
                                {
                                    new EventMessage()
                                    {
                                        Message = "User bazada olan, sveji olan, papkada olmayan reportu almag istedi, " +
                                        "amma biz o reportu yenisi ile evez ede bilmedik, api error verdi. " +
                                        "Ona gore event yarandi, relation ise yox. Pulunu da qaytardig",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    }
                                };
                            }

                            #endregion

                            await _unitOfWork.TransactionRepository.AddAsync(transaction);
                            await _unitOfWork.CommitAsync();

                            return null;
                        }
                    }
                }
                else
                {
                    if (await BuyReport(dbVin.Vin, dbVin.FileName))
                    {
                        dbVin.PurchasedTimes++;
                        dbVin.CreatedAt = DateTime.UtcNow.AddHours(4);

                        #region Event handle

                        if (userEvent == null)
                        {
                            Event newUserEvent = new Event()
                            {
                                AppUser = appUser ?? newUser,
                                CreatedAt = DateTime.UtcNow.AddHours(4),
                                DidRefundToBalance = false,
                                ErrorWhileRenew = false,
                                ErrorWhileReplace = false,
                                FileExists = true,
                                IsApiError = false,
                                IsFromApi = true,
                                IsRenewedDueToAbsence = false,
                                IsRenewedDueToExpire = true,
                                Vin = dbVin.Vin,
                                EventMessages = new List<EventMessage>()
                                {
                                    new EventMessage()
                                    {
                                        Message = "User bazadan papkada olan, kohne olan report ile relation qurdu. " +
                                        "Yari yolda qoymayag deye getdik reportu yeniledik, pul odeyib axi.",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    }
                                }
                            };

                            await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                        }
                        else
                        {
                            userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                            userEvent.DidRefundToBalance = false;
                            userEvent.ErrorWhileRenew = false;
                            userEvent.ErrorWhileReplace = false;
                            userEvent.FileExists = true;
                            userEvent.IsApiError = false;
                            userEvent.IsFromApi = true;
                            userEvent.IsRenewedDueToAbsence = false;
                            userEvent.IsRenewedDueToExpire = true;
                            userEvent.EventMessages = new List<EventMessage>()
                            {
                                new EventMessage()
                                {
                                    Message = "User bazadan papkada olan, kohne olan report ile relation qurdu. " +
                                    "Yari yolda qoymayag deye getdik reportu yeniledik, pul odeyib axi.",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                }
                            };
                        }

                        #endregion

                        await _unitOfWork.TransactionRepository.AddAsync(transaction);
                        await _unitOfWork.AppUserToVincodeRepository.AddAsync(appUserToVincode);
                        await _unitOfWork.CommitAsync();

                        return dbVin.FileName;
                    }
                    else
                    {
                        await Refund(phone);

                        #region Event handle

                        if (userEvent == null)
                        {
                            Event newUserEvent = new Event()
                            {
                                AppUser = appUser ?? newUser,
                                CreatedAt = DateTime.UtcNow.AddHours(4),
                                DidRefundToBalance = true,
                                ErrorWhileRenew = true,
                                ErrorWhileReplace = false,
                                FileExists = true,
                                IsApiError = true,
                                IsFromApi = true,
                                IsRenewedDueToAbsence = false,
                                IsRenewedDueToExpire = false,
                                Vin = dbVin.Vin,
                                EventMessages = new List<EventMessage>()
                                {
                                    new EventMessage()
                                    {
                                        Message = "User bazada ve papkada olan kohne reportu almag istedi, pul odeyib deye getdik onu yenilemeye. " +
                                        "Yeniliye bilmedik, ona gore pulun gaytardig.",
                                        CreatedAt = DateTime.UtcNow.AddHours(4)
                                    }
                                }
                            };

                            await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                        }
                        else
                        {
                            userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                            userEvent.DidRefundToBalance = true;
                            userEvent.ErrorWhileRenew = true;
                            userEvent.ErrorWhileReplace = false;
                            userEvent.FileExists = true;
                            userEvent.IsApiError = true;
                            userEvent.IsFromApi = true;
                            userEvent.IsRenewedDueToAbsence = false;
                            userEvent.IsRenewedDueToExpire = false;
                            userEvent.EventMessages = new List<EventMessage>()
                            {
                                new EventMessage()
                                {
                                    Message = "User bazada ve papkada olan kohne reportu almag istedi, pul odeyib deye getdik onu yenilemeye. " +
                                    "Yeniliye bilmedik, ona gore pulun gaytardig, dedik birazdan yene yoxla.",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                }
                            };
                        }

                        #endregion

                        await _unitOfWork.TransactionRepository.AddAsync(transaction);
                        await _unitOfWork.CommitAsync();

                        return null;
                    }
                }
            }
            else
            {
                Guid guid = Guid.NewGuid();

                string fileName = vin + "_" + guid + ".html";

                VinCode newVin = new VinCode()
                {
                    Vin = vin,
                    FileName = fileName,
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                    PurchasedTimes = 1
                };

                AppUserToVincode appUserToVincode = new AppUserToVincode()
                {
                    AppUser = appUser ?? newUser,
                    VinCode = newVin,
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                };

                if (await BuyReport(vin, fileName))
                {
                    #region Event handle

                    if (userEvent == null)
                    {
                        Event newUserEvent = new Event()
                        {
                            AppUser = appUser ?? newUser,
                            CreatedAt = DateTime.UtcNow.AddHours(4),
                            DidRefundToBalance = false,
                            ErrorWhileRenew = false,
                            ErrorWhileReplace = false,
                            FileExists = true,
                            IsApiError = false,
                            IsFromApi = true,
                            IsRenewedDueToAbsence = false,
                            IsRenewedDueToExpire = false,
                            Vin = vin,
                            EventMessages = new List<EventMessage>()
                            {
                                new EventMessage()
                                {
                                    Message = "report yox idi, yaratdig yenisini, relation gurdug",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                }
                            }
                        };

                        await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                    }
                    else
                    {
                        userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                        userEvent.DidRefundToBalance = false;
                        userEvent.ErrorWhileRenew = false;
                        userEvent.ErrorWhileReplace = false;
                        userEvent.FileExists = true;
                        userEvent.IsApiError = false;
                        userEvent.IsFromApi = true;
                        userEvent.IsRenewedDueToAbsence = false;
                        userEvent.IsRenewedDueToExpire = false;
                        userEvent.EventMessages = new List<EventMessage>()
                        {
                            new EventMessage()
                            {
                                Message = "report yox idi, yaratdig yenisini, relation gurdug",
                                CreatedAt = DateTime.UtcNow.AddHours(4)
                            }
                        };
                    }

                    #endregion

                    await _unitOfWork.TransactionRepository.AddAsync(transaction);
                    await _unitOfWork.AppUserToVincodeRepository.AddAsync(appUserToVincode);
                    await _unitOfWork.CommitAsync();

                    return fileName;
                }
                else
                {
                    await Refund(phone);

                    #region Event handle

                    if (userEvent == null)
                    {
                        Event newUserEvent = new Event()
                        {
                            AppUser = appUser ?? newUser,
                            CreatedAt = DateTime.UtcNow.AddHours(4),
                            DidRefundToBalance = true,
                            ErrorWhileRenew = false,
                            ErrorWhileReplace = false,
                            FileExists = false,
                            IsApiError = true,
                            IsFromApi = true,
                            IsRenewedDueToAbsence = false,
                            IsRenewedDueToExpire = false,
                            Vin = vin,
                            EventMessages = new List<EventMessage>()
                            {
                                new EventMessage()
                                {
                                    Message = "User yeni report almag istedi, ala bilmedi, cunki api error verdi. Pulun qaytardig",
                                    CreatedAt = DateTime.UtcNow.AddHours(4)
                                }
                            }
                        };

                        await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                    }
                    else
                    {
                        userEvent.UpdatedAt = DateTime.UtcNow.AddHours(4);
                        userEvent.DidRefundToBalance = true;
                        userEvent.ErrorWhileRenew = false;
                        userEvent.ErrorWhileReplace = false;
                        userEvent.FileExists = false;
                        userEvent.IsApiError = true;
                        userEvent.IsFromApi = true;
                        userEvent.IsRenewedDueToAbsence = false;
                        userEvent.IsRenewedDueToExpire = false;
                        userEvent.EventMessages = new List<EventMessage>()
                        {
                            new EventMessage()
                            {
                                Message = "User yeni report almag istedi, ala bilmedi, cunki api error verdi. Pulun qaytardig",
                                CreatedAt = DateTime.UtcNow.AddHours(4)
                            }
                        };
                    }

                    #endregion

                    await _unitOfWork.TransactionRepository.AddAsync(transaction);
                    await _unitOfWork.CommitAsync();

                    return null;
                }
            }
        }

        public async Task<bool> TryToFixAbsence(string vin, string fileName)
        {
            return await BuyReport(vin, fileName);
        }

        public async Task<bool> BuyReport(string vin, string fileName)
        {
            #region path

            string path = Path.Combine(_env.WebRootPath);

            string[] folders = { "assets", "files", $"{vin}" };

            foreach (string folder in folders)
            {
                path = Path.Combine(path, folder);
            }

            #endregion

            #region pokupka

            string url = $"https://api.allreports.tools/wp-json/v1/get_report_by_wholesaler/{vin}/{Configuration.GetSection("Api_Key:MyKey").Value}/carfax/en";

            HttpResponseMessage response = null;

            ResponseVM responseVM = new ResponseVM();

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync(url);
            }

            #endregion

            if (response.IsSuccessStatusCode)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, fileName);

                #region convertaciya

                string convertedResponse = await response.Content.ReadAsStringAsync();

                responseVM = JsonConvert.DeserializeObject<ResponseVM>(convertedResponse);

                byte[] report = Convert.FromBase64String(responseVM.Report.Report);

                string responseHTML = Encoding.UTF8.GetString(report);

                #endregion

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                #region zameni v ix html kode

                responseHTML = responseHTML.Replace("<link href=\"https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/jquery.qtip.min.css\" media=\"all\" rel=\"stylesheet\" type=\"text/css\"/>", "<style> /* fallback */@font-face{font-family: 'Material Icons'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/materialicons/v139/flUhRq6tzZclQEJ-Vdg-IuiaDsNc.woff2) format('woff2');}/* fallback */@font-face{font-family: 'Material Icons Sharp'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/materialiconssharp/v108/oPWQ_lt5nv4pWNJpghLP75WiFR4kLh3kvmvR.woff2) format('woff2');}.material-icons{font-family: 'Material Icons'; font-weight: normal; font-style: normal; font-size: 24px; line-height: 1; letter-spacing: normal; text-transform: none; display: inline-block; white-space: nowrap; word-wrap: normal; direction: ltr; -webkit-font-feature-settings: 'liga'; -webkit-font-smoothing: antialiased;}.material-icons-sharp{font-family: 'Material Icons Sharp'; font-weight: normal; font-style: normal; font-size: 24px; line-height: 1; letter-spacing: normal; text-transform: none; display: inline-block; white-space: nowrap; word-wrap: normal; direction: ltr; -webkit-font-feature-settings: 'liga'; -webkit-font-smoothing: antialiased;}#qtip-overlay.blurs,.qtip-close{cursor: pointer}.qtip{position: absolute; left: -28000px; top: -28000px; display: none; max-width: 280px; min-width: 50px; font-size: 10.5px; line-height: 12px; direction: ltr; box-shadow: none; padding: 0}.qtip-content,.qtip-titlebar{position: relative; overflow: hidden}.qtip-content{padding: 5px 9px; text-align: left; word-wrap: break-word}.qtip-titlebar{padding: 5px 35px 5px 10px; border-width: 0 0 1px; font-weight: 700}.qtip-titlebar+.qtip-content{border-top-width: 0 !important}.qtip-close{position: absolute; right: -9px; top: -9px; z-index: 11; outline: 0; border: 1px solid transparent}.qtip-titlebar .qtip-close{right: 4px; top: 50%; margin-top: -9px}* html .qtip-titlebar .qtip-close{top: 16px}.qtip-icon .ui-icon,.qtip-titlebar .ui-icon{display: block; text-indent: -1000em; direction: ltr}.qtip-icon,.qtip-icon .ui-icon{-moz-border-radius: 3px; -webkit-border-radius: 3px; border-radius: 3px; text-decoration: none}.qtip-icon .ui-icon{width: 18px; height: 14px; line-height: 14px; text-align: center; text-indent: 0; font: normal 700 10px/13px Tahoma, sans-serif; color: inherit; background: -100em -100em no-repeat}.qtip-default{border: 1px solid #F1D031; background-color: #FFFFA3; color: #555}.qtip-default .qtip-titlebar{background-color: #FFEF93}.qtip-default .qtip-icon{border-color: #CCC; background: #F1F1F1; color: #777}.qtip-default .qtip-titlebar .qtip-close{border-color: #AAA; color: #111}.qtip-light{background-color: #fff; border-color: #E2E2E2; color: #454545}.qtip-light .qtip-titlebar{background-color: #f1f1f1}.qtip-dark{background-color: #505050; border-color: #303030; color: #f3f3f3}.qtip-dark .qtip-titlebar{background-color: #404040}.qtip-dark .qtip-icon{border-color: #444}.qtip-dark .qtip-titlebar .ui-state-hover{border-color: #303030}.qtip-cream{background-color: #FBF7AA; border-color: #F9E98E; color: #A27D35}.qtip-red,.qtip-red .qtip-icon,.qtip-red .qtip-titlebar .ui-state-hover{border-color: #D95252}.qtip-cream .qtip-titlebar{background-color: #F0DE7D}.qtip-cream .qtip-close .qtip-icon{background-position: -82px 0}.qtip-red{background-color: #F78B83; color: #912323}.qtip-red .qtip-titlebar{background-color: #F06D65}.qtip-red .qtip-close .qtip-icon{background-position: -102px 0}.qtip-green{background-color: #CAED9E; border-color: #90D93F; color: #3F6219}.qtip-green .qtip-titlebar{background-color: #B0DE78}.qtip-green .qtip-close .qtip-icon{background-position: -42px 0}.qtip-blue{background-color: #E5F6FE; border-color: #ADD9ED; color: #5E99BD}.qtip-blue .qtip-titlebar{background-color: #D0E9F5}.qtip-blue .qtip-close .qtip-icon{background-position: -2px 0}.qtip-shadow{-webkit-box-shadow: 1px 1px 3px 1px rgba(0, 0, 0, .15); -moz-box-shadow: 1px 1px 3px 1px rgba(0, 0, 0, .15); box-shadow: 1px 1px 3px 1px rgba(0, 0, 0, .15)}.qtip-bootstrap,.qtip-rounded,.qtip-tipsy{-moz-border-radius: 5px; -webkit-border-radius: 5px; border-radius: 5px}.qtip-rounded .qtip-titlebar{-moz-border-radius: 4px 4px 0 0; -webkit-border-radius: 4px 4px 0 0; border-radius: 4px 4px 0 0}.qtip-youtube{-moz-border-radius: 2px; -webkit-border-radius: 2px; border-radius: 2px; -webkit-box-shadow: 0 0 3px #333; -moz-box-shadow: 0 0 3px #333; box-shadow: 0 0 3px #333; color: #fff; border: 0 solid transparent; background: #4A4A4A; background-image: -webkit-gradient(linear, left top, left bottom, color-stop(0, #4A4A4A), color-stop(100%, #000)); background-image: -webkit-linear-gradient(top, #4A4A4A 0, #000 100%); background-image: -moz-linear-gradient(top, #4A4A4A 0, #000 100%); background-image: -ms-linear-gradient(top, #4A4A4A 0, #000 100%); background-image: -o-linear-gradient(top, #4A4A4A 0, #000 100%)}.qtip-youtube .qtip-titlebar{background-color: #4A4A4A; background-color: rgba(0, 0, 0, 0)}.qtip-youtube .qtip-content{padding: .75em; font: 12px arial, sans-serif; filter: progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#4a4a4a, EndColorStr=#000000); -ms-filter: \"progid:DXImageTransform.Microsoft.Gradient(GradientType=0,StartColorStr=#4a4a4a,EndColorStr=#000000);\"}.qtip-youtube .qtip-icon{border-color: #222}.qtip-youtube .qtip-titlebar .ui-state-hover{border-color: #303030}.qtip-jtools{background: #232323; background: rgba(0, 0, 0, .7); background-image: -webkit-gradient(linear, left top, left bottom, from(#717171), to(#232323)); background-image: -moz-linear-gradient(top, #717171, #232323); background-image: -webkit-linear-gradient(top, #717171, #232323); background-image: -ms-linear-gradient(top, #717171, #232323); background-image: -o-linear-gradient(top, #717171, #232323); border: 2px solid #ddd; border: 2px solid rgba(241, 241, 241, 1); -moz-border-radius: 2px; -webkit-border-radius: 2px; border-radius: 2px; -webkit-box-shadow: 0 0 12px #333; -moz-box-shadow: 0 0 12px #333; box-shadow: 0 0 12px #333}.qtip-jtools .qtip-titlebar{background-color: transparent; filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#717171, endColorstr=#4A4A4A); -ms-filter: \"progid:DXImageTransform.Microsoft.gradient(startColorstr=#717171,endColorstr=#4A4A4A)\"}.qtip-jtools .qtip-content{filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#4A4A4A, endColorstr=#232323); -ms-filter: \"progid:DXImageTransform.Microsoft.gradient(startColorstr=#4A4A4A,endColorstr=#232323)\"}.qtip-jtools .qtip-content,.qtip-jtools .qtip-titlebar{background: 0 0; color: #fff; border: 0 dashed transparent}.qtip-jtools .qtip-icon{border-color: #555}.qtip-jtools .qtip-titlebar .ui-state-hover{border-color: #333}.qtip-cluetip{-webkit-box-shadow: 4px 4px 5px rgba(0, 0, 0, .4); -moz-box-shadow: 4px 4px 5px rgba(0, 0, 0, .4); box-shadow: 4px 4px 5px rgba(0, 0, 0, .4); background-color: #D9D9C2; color: #111; border: 0 dashed transparent}.qtip-cluetip .qtip-titlebar{background-color: #87876A; color: #fff; border: 0 dashed transparent}.qtip-cluetip .qtip-icon{border-color: #808064}.qtip-cluetip .qtip-titlebar .ui-state-hover{border-color: #696952; color: #696952}.qtip-tipsy{background: #000; background: rgba(0, 0, 0, .87); color: #fff; border: 0 solid transparent; font-size: 11px; font-family: 'Lucida Grande', sans-serif; font-weight: 700; line-height: 16px; text-shadow: 0 1px #000}.qtip-tipsy .qtip-titlebar{padding: 6px 35px 0 10px; background-color: transparent}.qtip-tipsy .qtip-content{padding: 6px 10px}.qtip-tipsy .qtip-icon{border-color: #222; text-shadow: none}.qtip-tipsy .qtip-titlebar .ui-state-hover{border-color: #303030}.qtip-tipped{border: 3px solid #959FA9; -moz-border-radius: 3px; -webkit-border-radius: 3px; border-radius: 3px; background-color: #F9F9F9; color: #454545; font-weight: 400; font-family: serif}.qtip-tipped .qtip-titlebar{border-bottom-width: 0; color: #fff; background: #3A79B8; background-image: -webkit-gradient(linear, left top, left bottom, from(#3A79B8), to(#2E629D)); background-image: -webkit-linear-gradient(top, #3A79B8, #2E629D); background-image: -moz-linear-gradient(top, #3A79B8, #2E629D); background-image: -ms-linear-gradient(top, #3A79B8, #2E629D); background-image: -o-linear-gradient(top, #3A79B8, #2E629D); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#3A79B8, endColorstr=#2E629D); -ms-filter: \"progid:DXImageTransform.Microsoft.gradient(startColorstr=#3A79B8,endColorstr=#2E629D)\"}.qtip-tipped .qtip-icon{border: 2px solid #285589; background: #285589}.qtip-tipped .qtip-icon .ui-icon{background-color: #FBFBFB; color: #555}.qtip-bootstrap{font-size: 14px; line-height: 20px; color: #333; padding: 1px; background-color: #fff; border: 1px solid #ccc; border: 1px solid rgba(0, 0, 0, .2); -webkit-border-radius: 6px; -moz-border-radius: 6px; border-radius: 6px; -webkit-box-shadow: 0 5px 10px rgba(0, 0, 0, .2); -moz-box-shadow: 0 5px 10px rgba(0, 0, 0, .2); box-shadow: 0 5px 10px rgba(0, 0, 0, .2); -webkit-background-clip: padding-box; -moz-background-clip: padding; background-clip: padding-box}.qtip-bootstrap .qtip-titlebar{padding: 8px 14px; margin: 0; font-size: 14px; font-weight: 400; line-height: 18px; background-color: #f7f7f7; border-bottom: 1px solid #ebebeb; -webkit-border-radius: 5px 5px 0 0; -moz-border-radius: 5px 5px 0 0; border-radius: 5px 5px 0 0}.qtip-bootstrap .qtip-titlebar .qtip-close{right: 11px; top: 45%; border-style: none}.qtip-bootstrap .qtip-content{padding: 9px 14px}.qtip-bootstrap .qtip-icon{background: 0 0}.qtip-bootstrap .qtip-icon .ui-icon{width: auto; height: auto; float: right; font-size: 20px; font-weight: 700; line-height: 18px; color: #000; text-shadow: 0 1px 0 #fff; opacity: .2; filter: alpha(opacity=20)}#qtip-overlay,#qtip-overlay div{left: 0; top: 0; width: 100%; height: 100%}.qtip-bootstrap .qtip-icon .ui-icon:hover{color: #000; text-decoration: none; cursor: pointer; opacity: .4; filter: alpha(opacity=40)}.qtip:not(.ie9haxors) div.qtip-content,.qtip:not(.ie9haxors) div.qtip-titlebar{filter: none; -ms-filter: none}.qtip .qtip-tip{margin: 0 auto; overflow: hidden; z-index: 10}.qtip .qtip-tip,x:-o-prefocus{visibility: hidden}.qtip .qtip-tip,.qtip .qtip-tip .qtip-vml,.qtip .qtip-tip canvas{position: absolute; color: #123456; background: 0 0; border: 0 dashed transparent}.qtip .qtip-tip canvas{top: 0; left: 0}.qtip .qtip-tip .qtip-vml{behavior: url(#default#VML); display: inline-block; visibility: visible}#qtip-overlay{position: fixed}#qtip-overlay div{position: absolute; background-color: #000; opacity: .7; filter: alpha(opacity=70); -ms-filter: \"progid:DXImageTransform.Microsoft.Alpha(Opacity=70)\"}.qtipmodal-ie6fix{position: absolute !important}.detailLineBreak{clear: both;}.open-cip-menu,.close-cip-menu{display: none;}/*** Point of Impact ***/.poi,.poi div{color: #212121;}.poi div{text-align: center;}.poi>div{font-size: 10px;}.poi .outer{background: unset !important; overflow: hidden; border: 1px solid #212121; display: inline-block; vertical-align: middle; margin: 3px; position: relative; top: 0; right: 0; z-index: 0;}.poi .outer::after{content: url(\"https://media.carfax.com/img/vhr/poi/poi-crosshatch-background-wider.svg\"); position: absolute; left: 0; top: 0; z-index: -2; width: 101%;}.poi .inner{margin: 11px 12px 12px 12px;}.poi .inner div:first-child{z-index: 2; position: relative; border: 1px solid #212121; box-sizing: border-box;}.poi .outer div:not(:first-child){background-color: white; position: absolute; z-index: 1;}.poi .outer .top-left{width: 34%; height: 34%; top: 0; left: 0;}.poi .outer .top-center{width: 34%; height: 50%; top: 0; left: 33%;}.poi .outer .top-right{width: 34%; height: 34%; top: 0; right: 0;}.poi .outer .left-center{width: 50%; height: 34%; top: 33%; left: 0;}.poi .outer .right-center{width: 50%; height: 34%; top: 33%; right: 0;}.poi .outer .bottom-left{width: 34%; height: 34%; bottom: 0; left: 0;}.poi .outer .bottom-center{width: 34%; height: 50%; bottom: 0; left: 33%;}.poi .outer .bottom-right{width: 34%; height: 34%; bottom: 0; right: 0;}.poi.leftfront .outer .top-left,.poi.front .outer .top-center,.poi.rightfront .outer .top-right{background: #fff; display: none; top: -.2px;}.poi.leftside .outer .left-center,.poi.rightside .outer .right-center{background: #fff; display: none;}.poi.leftrear .outer .bottom-left,.poi.rear .outer .bottom-center,.poi.rightrear .outer .bottom-right{background: #fff; display: none; bottom: -.2px;}.dmg-title{letter-spacing: -0.24px; text-align: center; margin-top: 10px; line-height: 1.7;}.car-container{background-color: #fffbe8 !important; padding: 10px !important;}.poi-pixel{height: 100%; width: 100%;}.image-container{display: flex;}.poi-row{display: block; min-width: 40px;}.poi-right-column{display: inline-flex; vertical-align: top; flex-direction: column; width: 44%; margin-right: 5px; margin-bottom: 8px;}.poi-fox-box{width: 54%; display: inline-flex; justify-content: flex-end;}.poi-yellow-fill-container{position: relative; z-index: 0; bottom: 0; width: 278px; border: 1px solid #E0E0E0; border-radius: 3px; margin-left: -5px;}.poi-container{position: absolute; z-index: 1; top: 0; width: 278px; display: block; background-color: unset; border: none; max-height: none;}.yellow-fill{width: 100%; height: 100%; position: relative; top: 0; border-radius: 3px;}.standard-fox{position: relative; right: 15px; top: 83px; height: 304px;}.poi-iso-fox{position: relative; right: 15px; height: 292px;}.poi-iso-fox.truck{top: 27px;}.poi-iso-fox.suv{top: 26px;}.poi-iso-fox.car{top: 11px;}.poi-iso-fox.van{top: 26px;}.smallest-fox{margin-right: -352px; margin-bottom: -16px; position: relative; top: 15px; height: 100px; z-index: 2;}.spacer-div{min-height: 10px;}.pad-for-smashed{padding-left: 90px;}.car-height{height: 275px;}.truck-height{height: 293px;}.van-height{height: 290px;}.suv-height{height: 292px;}.car-roof-height{height: 275px;}.truck-roof-height{height: 293px;}.van-roof-height{height: 290px;}.suv-roof-height{height: 292px;}/*** Ratings and Reviews ***/.ratings-container{display: flex; align-items: center; word-break: normal;}.ratings-container strong{font-weight: 500;}.ratings-stars-container{padding: 8px 0 0 0; flex-wrap: wrap;}.ratings-stars-container svg{height: 24px; width: 24px; flex: none;}.cadinfo .ratings-stars-container{font-size: 16px;}.ratings-verified-reviews-container{flex-wrap: nowrap;}.ratings-verified-reviews-container img{height: 16px; padding-bottom: 4px; max-width: none;}.ratings-average-value{font-weight: 500; padding-right: 5px;}.ratings-verified-reviews{padding: 4px 4px 4px 0; color: #3777BC; cursor: pointer; text-decoration: underline; flex: none;}.ratings-verified-reviews:visited,.ratings-verified-reviews:hover{color: #2C5F96;}.verified-reviews-padded{/* delete this - it was just to support a transition between builds */ padding-left: 8px;}.verified-reviews-padding{padding-right: 8px;}.details-row,.details-owner-row,.details-mobile-row{display: flex; justify-content: space-around;}.details-row>div,.details-mobile-row>div{padding: 8px 3px;}.details-row.recallDetailRow>div,.details-mobile-row.recallDetailRow>div{padding-top: 0;}.details-column{display: flex; flex-direction: column;}/*** Recall stuff that was still inline ***/.recallDetailRow{border-top: 0 none !important}.recallToggle{line-height: 12px; width: 41.2%; margin: 3px; font-weight: 500; display: flex; align-items: center; cursor: pointer; float: right;}.recallToggle a{text-decoration: none; padding-left: 12px;}.recallText{background: #fffbe7; margin: 30px 1% 1% 1%; padding: 10px; border: 1px solid #ccc; -webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; box-sizing: border-box; clear: both; position: relative; display: none;}.recallText p+p{margin-top: 16px;}.recallText:after,.recallText:before{bottom: 100%; left: 59.6%; border: solid transparent; content: \" \"; height: 0; width: 0; position: absolute; pointer-events: none;}.recallText:after{border-color: rgba(255, 251, 231, 0); border-bottom-color: #fffbe7; border-width: 9px; margin-left: -9px;}.recallText:before{border-color: rgba(204, 204, 204, 0); border-bottom-color: #ccc; border-width: 10px; margin-left: -10px;}.bluetexticon{color: #3777bc;}.material-icons-sharp.md-16{font-size: 16px;}.accident-superiority-container{display: flex; align-items: center; width: 100%;}@media print{.accident-superiority-container{display: flex;}.accident-superiority-container img{width: 100%;}}@media all and (-ms-high-contrast: none),(-ms-high-contrast: active){.material-icons-sharp{/* sharp icons don't work in ie for unknown reason */ font-family: 'Material Icons' !important;}}@media print{.showonprint{display: block !important;}.bluetexticon i{display: none;}.bluetexticon:before{content: '-';}.recallText:before, .recallText:after{display: none;}}/*** New Facelift Stuffffffffffff ***/.vehicle-description{font-size: 16px; font-weight: 500; line-height: 120%; margin-bottom: 4px;}.vehicle-details{font-size: 12px; line-height: 1.17;}.section-description{font-size: 12px; color: #FFFFFF; font-weight: normal; white-space: nowrap;}.section-header-title{display: flex; align-items: center;}.column-radius{padding-left: 17px;}.disclaimer-section{font-size: 12px;}.disclaimer-section p{margin-top: 18px;}.history-overview-row strong{margin-right: 2px;}.header-row-text{font-size: 16px; display: block; vertical-align: middle;}ul.detail-record-comments-groups>li.detail-record-comments-group:first-of-type{font-weight: 500;}ul.detail-record-comments-groups>li.detail-record-comments-group:not(:first-of-type){padding-top: 1em;}ul.detail-record-comments-group-inner-lines{font-weight: 400; margin-left: 7px;}li.detail-record-comments-group-inner-line::before{content: \"-\"; margin-left: -7px;}.soft-lemon-text{font-weight: 400; padding-bottom: 1em;}@media screen and (min-width: 1280px){.DescCol .header-jump-link div:not(#vhrHeaderRow0){width: 60%;}.superiority-carfox-header #vhrHeaderRow0, .superiority-carfox-header #vhrHeaderRow1, .superiority-carfox-header #vhrHeaderRow2{width: 55% !important;}}@media print{.superiority-carfox-header #vhrHeaderRow0, .superiority-carfox-header #vhrHeaderRow1, .superiority-carfox-header #vhrHeaderRow2{width: 40%;}}.header-icon-container{height: 100%; display: flex; align-items: center; justify-content: center;}a{color: #3777BC;}a:visited,a:hover{color: #2C5F96;}a[rel=\"tooltip\"]{text-decoration: none;}#otherInformationTable strong{font-weight: 500;}.additional-history-jump-link{font-weight: 500;}.title-history-guaranteed{font-weight: 700; color: #7DC243;}.large-title-history-guaranteed{font-weight: 700; color: #7DC243; font-size: 18px;}.owner-bar{display: flex; justify-content: space-between; background-color: #D4E2F1; height: 56px; line-height: 1.5; border-bottom: 1px solid #BDBDBD; border-radius: 4px 4px 0 0;}@media print{.owner-bar{height: 100%; line-height: 1;}}.owner-bar.ownerXpert{height: 86px;}.owner-desc{padding: 6px 0 0 23px; font-weight: 500; display: flex;}@media print{.owner-desc{padding-top: 4px;}}.owner-desc-icon{padding-top: 2px;}.owner-desc-text{padding-left: 11px; font-weight: normal;}.owner-number-text{font-size: 16px; font-weight: 500;}@media print{.owner-number-text{margin-bottom: 4px;}}.owner-bar-right-side{width: 165px; display: flex; flex-direction: column; padding: 8px 16px 0 0; text-align: right;}.secHdrRow{color: #ffffff; background-color: #3777BC; height: 56px; border-radius: 4px 4px 0 0;}.section-header-logo{height: 24px; width: 128px;}.section-header-text{font-size: 16px; font-weight: 500; color: #FFFFFF; white-space: nowrap; line-height: 22px; margin-left: 10px;}.summaryOdd{background-color: #f5f5f5;}.summaryEven{background-color: #ffffff;}/*classes for HBV no arrows display*/.chbv-section-no-arrows{display: flex; min-height: 160px; max-height: 160px; border: 1px solid #BDBDBD; border-top: none; border-radius: 0 0 4px 4px; margin-bottom: 25px;}.chbv-no-arrows-left-col{min-width: 15%;}.chbv-no-arrows-middle-col{display: flex; flex-direction: column; align-items: center; justify-content: center; min-width: 70%;}.chbv-no-arrows-right-col{display: flex; flex-direction: column; justify-content: center; align-items: center; width: 15%;}.pricing-no-arrows{display: flex; flex-direction: column; justify-content: space-between; align-items: center; height: 45%; width: 100%;}.pricing-text-no-arrows{font-size: 28px; font-weight: 500; color: #3777bc;}.tos-toggle-no-arrows{display: flex; flex-direction: column; justify-content: space-between; align-items: flex-end; position: relative; top: 30px; height: 32%; width: 90%;}.radio-text-no-arrows{font-size: 14px; font-weight: 500; margin: auto 10px;}.radio-visible{visibility: visible !important;}.medium-button-text{font-size: 14px; color: #FFFFFF; padding: 0 13px; font-weight: 400;}/*end no arrows display*/@media print{.owner-bar-right-side{padding-top: 4px;}}.owner-type{text-transform: capitalize;}@media screen and (max-width: 900px){.accident-superiority-container{display: none;}}@media (max-width: 1280px){.tablet-hide{display: none;}}@media print{.owner-type{margin-bottom: 4px;}}.details-list li:first-of-type{font-weight: 500; margin-left: 0; list-style: none;}.details-list li:not(:first-of-type){margin-left: 8px; list-style: none; position: relative;}.details-list li:not(:first-of-type):before{content: \"-\"; position: absolute; left: -7px;}ul.source-lines>li.source-line:not(:first-of-type)>a{display: none;}@media print{ul.source-lines>li.source-line:not(:first-of-type)>a{display: block;}ul.source-lines>li.source-line:first-of-type>a{color: #212121;}}/* External Site Warning/Interstitial Modal Styles */#external-site-warning{width: 648px; display: none;}.external-site-warning-header{display: flex; flex-direction: row; justify-content: space-between; align-items: center; font-size: 18px; font-weight: 500; padding: 24px 24px 16px 24px;}.external-site-warning-content{padding: 0 24px 24px 24px;}.external-site-warning-content p{font-size: 16px; padding-bottom: 40px;}.external-site-warning-content>a,.external-site-warning-content>div{display: inline-block; border-radius: 4px;}.external-site-warning-content>a{background-color: #7DC243; color: #FFFFFF; padding: 12px 24px; text-decoration: none;}.external-site-warning-content>div{border: 1px solid #3777BC; padding: 11px 24px; font-weight: 500; color: #3777BC;}/* Airbag Deployed Modal */.airbag-deployed-modal{width: 100%; max-width: 648px; padding: 0; font-size: 16px; line-height: 24px;}.airbag-deployed-header{display: flex; flex-direction: row; justify-content: space-between; align-items: center; border-bottom: 1px solid #E0E0E0; padding: 24px 24px 16px 24px;}.airbag-deployed-header h3{font-size: 18px; font-weight: 500;}.airbag-deployed-content{padding: 24px;}.airbag-deployed-content>p{padding-bottom: 40px;}.airbag-deployed-table{display: flex; flex-direction: column;}.airbag-deployed-table>div{display: flex; flex-direction: row;}.airbag-deployed-table>div:nth-child(2n){background: #EEEEEE;}.airbag-deployed-table>.airbag-deployed-table-header{background: #3777BC; color: #fff;}.airbag-deployed-table>div>div{padding: 8px 16px;}.airbag-deployed-table>div:first-child>div{font-weight: 500;}.airbag-deployed-table>div>div:first-child{width: 40%; font-weight: 500;}.airbag-deployed-table>div>div:nth-child(2){width: 60%;}.airbag-deployed-modal button{background-color: #7DC243; height: 40px; width: 60px; border-radius: 4px; border: 0; color: #fff; text-align: center; margin: 24px; font-size: 14px;}/* Global Modal Styles */.open-global-modal,.close-global-modal{cursor: pointer;}#global-modal,.global-modal-content{display: none;}#global-modal .modal-overlay{position: fixed; width: 100%; height: 100%; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0, 0, 0, 0.5); z-index: 20; cursor: pointer;}#global-modal .modal-content{position: fixed; border-radius: 4px; box-shadow: 0 15px 12px 0 rgba(0, 0, 0, 0.22), 0 19px 38px 0 rgba(0, 0, 0, 0.3); background-color: #fff; z-index: 21;}#global-modal.vhr-mobile .modal-content{height: 100%; width: 100%; top: 0; bottom: 0; overflow-y: scroll;}/* favorites */.favorites{display: flex; align-items: center; font-size: 14px; line-height: 24px; position: relative;}.favorites .favorites-cell{display: flex; align-items: center;}.favorites i{color: #F04D88; position: relative; top: -2px;}.favorites .favorites-number{font-weight: 500;}.favorites .favorites-label{padding-left: 8px;}.cadcontent,.cadcontent .favorites{display: flex; flex-direction: column; align-items: center;}.cadinfo .favorites{border-top: 1px solid #E0E0E0; margin-top: 5px; padding-top: 7px;}.cadinfo .favorites .favorites-label{padding-left: 0;}.cadinfo .favorites-label,.hdrNIL .favorites-label{cursor: pointer; color: #3777BC;}.cadinfo .favorites .favorites-number{font-size: 16px;}.hdrNIL .favorites-label:hover~.favorites-hover,.cadinfo .favorites-label:hover~.favorites-hover{display: block;}.favorites-hover span{font-weight: 500;}.favorites-hover{display: none; background-color: white; z-index: 200; position: absolute; left: -20%; top: 25px; width: 100%; border: 1px solid #CCCCCC; box-shadow: 0 0 6px 5px rgba(0, 0, 0, .1); font-size: 12px; line-height: 16px; padding: 14px 12px; text-align: left;}.cadinfo .favorites-hover{left: -35%; top: 50px; width: 150%;}.favorites-hover:before{content: \"\"; position: absolute; top: -9px; left: calc(50% - 8px); border-width: 0 8px 8px; border-style: solid; border-color: #CCCCCC transparent; display: block; width: 0;}.favorites-hover:after{content: \"\"; position: absolute; top: -8px; left: calc(50% - 8px); border-width: 0 8px 8px; border-style: solid; border-color: #FFFFFF transparent; display: block; width: 0;}/*info icon*/.info-icon{height: 24px; width: 24px; color: #3777BC;}/* name in lights info modal */.nil-modal{display: none; width: 546px; color: #424242; line-height: 24px; background-color: white;}.nil-modal .modal-main-text{position: relative; padding: 24px 24px 0 24px;}.nil-modal .modal-main-text .close-global-modal{position: absolute; top: 0; right: 0; padding: 24px;}.nil-modal p{padding-bottom: 16px; margin-right: 87px;}.nil-modal span{font-weight: 500; display: block;}.nil-modal .close-button{padding: 16px 24px 24px 24px; box-sizing: border-box;}.nil-modal .close-button .close-global-modal{background-color: #7DC243; color: white; font-weight: 500; border-radius: 4px; width: 67px; height: 40px; line-height: 40px; text-align: center;}.nil-modal .close-button .close-global-modal:hover{background-color: #90C067;}.nil-modal .close-button .close-global-modal:active{outline: none; background-color: #649B35;}.nil-modal .close-button .close-global-modal:focus,.nil-modal .close-button .close-global-modal:focus:active{outline: none;}@media screen and (max-width: 720px){.vehicleRecordSource .favorites{display: none;}}@media screen and (min-width: 1280px){.favorites-hover{width: 70%; left: -10%;}}@media screen{#print-only-nil{display: none;}}@media screen,print{html, body{min-width: 720px;}html, body, div, span, object, iframe, h1, h2, h3, h4, h5, h6, p, blockquote, pre, a, abbr, acronym, address, del, strong, sub, sup, tt, var, b, u, i, dl, dt, dd, ol, ul, li, fieldset, form, label, table, caption, tbody, tr, th, td{margin: 0; padding: 0; border: 0; outline: 0;}ol, ul{list-style: none;}blockquote, q{quotes: none;}blockquote:before, blockquote:after, q:before, q:after{content: ''; content: none;}:focus{outline: 0;}ins{text-decoration: none; text-decoration: none;}img{border: 0;}/*END RESET*/ body{font-family: 'Roboto', sans-serif; font-size: 14px; color: #212121; font-weight: normal; line-height: 125%;}#tabvhr{text-align: center; margin-bottom: 30px;}#reportBody{max-width: 1004px;}#hlModule, #ucl-module, #sgi-module, #icbc-module, #lienModule{border: 3px solid #98a3b1; margin: 0 auto; margin-bottom: 24px;}#sumOwnModule, #summaryTitleHistory, #otherInfoModule, #glossaryModule{margin: 0 auto; margin-bottom: 24px; border: 0px;}#detailsModule{margin-bottom: 0;}#sumOwnModule tr:last-child, #summaryTitleHistory tr:last-child, #otherInfoModule tr:last-child, #glossaryModule td{border-radius: 0 0 4px 4px;}#summaryTitleHistory td:last-child:not(.statCol):not(#bbgPara):not(.nopad){border-top: 0px solid #BDBDBD; border-right: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; border-radius: 0 0 4px 4px;}.qtip{max-width: 400px;}.tool-qtip .qtip-content{background: #fffbe7; padding: 8px 16px; font-size: 12px; line-height: 1.5; border-radius: 4px; border: 1px solid #BDBDBD; position: relative;}.tool-qtip{border: none; border-radius: 4px; box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);}.tool-qtip .qtip-tip{border-color: #BDBDBD; border: 1px solid #BDBDBD;}.glossary-section-border{border-top: 0px solid #BDBDBD; border-right: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD;}#sumOwnModule tr:last-child td:first-child, #otherInfoModule tr:last-child td:first-child{border-radius: 0 0 0 4px;}#sumOwnModule tr:last-child td:last-child, #otherInfoModule tr:last-child td:last-child{border-radius: 0 0 4px 0;}#sumOwnModule td:first-child{padding-left: 16px;}.advisorCheck{margin-top: 10px;}.clearfix{clear: both;}.pad2{padding: 2px !important;}.pad3{padding: 3px !important;}.pad5{padding: 5px !important;}.pad10{padding: 10px !important;}.nopad{padding: 0;}.red{color: #cc0000}.alertRed{color: #F44336;}.grey{color: #cccccc;}.smallgrey{color: #333333;}.hltitle{border-bottom: 1px solid #cccccc; background-color: #ffffcc}.invisibleText{position: relative; display: none; z-index: 2;}#leadIn{font-size: 12px; margin: 18px 0;}.hidden, #hidden, #forPersonalUsePrint, .varTagNoDisplay{display: none;}.hilite{background-color: #FFFF33}.centered{text-align: center;}.rightTextAlign{text-align: right;}.bigwhite{color: #ffffff;}.container{margin: 0 auto; text-align: left;}/****************MODULE GLOBAL********************/ .secHdrRow{color: #ffffff; background-color: #3777BC; height: 56px; border-radius: 4px 4px 0 0;}.secHdrRow-cell{display: flex; justify-content: space-between; align-items: center; height: 56px;}.backToTop{margin-left: 8%; display: flex; justify-content: flex-end;}.caret{margin-right: 5%;}.secHdrRow th:first-child{border-radius: 4px 0 0 0; border: solid 1px #3777bc;}.secHdrRow th:last-child{border-radius: 0 4px 0 0;}.secHdrRow th.statCol{text-align: center; padding: 0; border-bottom: none; font-weight: 500;}.secHdrRow th.statCol:last-child{border-right: none;}.secHdrRow .statCol img{margin-bottom: 2px;}.secHdrRow.detail-section-label{/*padding-top: 15px;*/ border-radius: 4px 4px 0 0; height: 55px;}.section-header-container{display: flex; flex-direction: column; align-items: flex-start; margin-left: 17px;}.sectionHeaderRowDetailedHistory{display: flex; justify-content: space-between; align-items: center;}.section-title{display: flex; padding-left: 17px;}.section-header-title{display: flex; align-items: center;}.section-header-logo{height: 24px; width: 128px;}.section-header-text{font-size: 18px; font-weight: 500; color: #FFFFFF; white-space: nowrap; line-height: 22px; margin-left: 10px;}/* This pushes the \"hide\" button to the far right. */ .section-header-text-chbv{width: 70%;}.section-description{font-size: 12px; color: #FFFFFF; font-weight: normal; white-space: nowrap; margin-top: 2px; line-height: normal;}.section-header-link, .section-header-link:visited{color: #FFFFFF; border: none; font-size: 14px; font-weight: 500;}.section-header-link:hover{color: #EEEEEE;}.ownerColumnTitle{width: 13%;}.back-to-top-link{display: flex; align-items: center; text-decoration: none; width: 125px;}.glossaryLink{padding-right: 24px !important; text-align: right !important; border: 0 none #fff !important;}.glossaryLink a, .glossaryLink a:visited{color: #FFFFFF; border: 0 none #fff; font-size: 12px; font-weight: 500;}.glossaryLink a:hover{color: #FFFF99;}#glossaryModule .glossaryLink{padding-top: 2px;}#glossaryModule .secHdrRow th{border-radius: 4px 4px 0 0;}/************************************ * HEADER ************************************/ #showMeHeader{margin: 0 auto; padding: 6px 0 18px 0; text-align: center;}#langToggle{color: #666; margin: 0 auto; text-align: right; padding-top: 3px;}a#language-toggle, a#language-toggle:visited{color: #003366; background-color: #efefef; padding: 10px; -moz-border-radius-bottomleft: 3px; -webkit-border-bottom-left-radius: 3px; -moz-border-radius-bottomright: 3px; -webkit-border-bottom-right-radius: 3px;}a#language-toggle:hover{color: #FF6600;}#mainHead, #hdr{border-bottom: 2px solid #aab6cb;}.equip-bullet{padding-right: 5px;}#installed-equipment-list td{vertical-align: top; white-space: nowrap;}a#vehicle-information-ucl-link{text-decoration: none; color: #000000;}a#vehicle-information-ucl-link:hover{text-decoration: underline; color: #0645AD;}#optStd{width: auto}div#dealer-name-in-lights{display: none;}label#headerVehicleInformation{font-weight: 500;}label#headerSectionVehicleLocationTitle{color: #000000; font-size: 12px;}.headerSection-cpo, .dnil-cpo{font-size: 12px; color: #424242; font-weight: 500; font-style: italic; padding-top: 6px; line-height: 18px;}#hdrXpert{position: relative; left: 7px; float: left; z-index: 99;}#hdrXpert label{padding-left: 7px; padding-top: 7px;}#vinDecode{margin: 0px 2px 26px 0;}.header-jump-link, .header-jump-link:visited, .header-jump-link:hover{text-decoration: none; color: #000000;}.topline{background-color: #ebe7cb;}.XDescCol{background-color: #fffbe7; border-bottom: 1px solid #ebe7cb; border-right: 2px solid #aab6cb; text-align: left; padding-left: 7px;}.XDactive{background-image: url(https://api.lot.report/media/carfax/XDescbg.gif); background-repeat: repeat-y; cursor: pointer;}.XDinactive{background-image: none; cursor: auto;}td.XDesc{padding-right: 4px;}.XDinactive .XDesc{text-decoration: none;}.XDactive .XDesc{text-decoration: underline;}td.hdrIcon{text-align: center;}.XDactive{background-image: url(https://api.lot.report/media/carfax/XDescbg.gif); background-repeat: repeat-y; cursor: pointer;}.flagged{display: block;}img.headerRowIcon{margin: 0; padding: 0; border: 0;}.hdrHL{margin-top: 40px;}#lastOdoReportedRow span{display: flex;}#lastOdoReportedRow strong{display: block; align-self: center; line-height: 14px;}#vhrHeaderRow0{height: 100%; display: flex; align-items: center; padding-left: 24px;}#vhrHeaderRow1{height: 100%; display: flex; align-items: center; padding-left: 24px;}#vhrHeaderRow2{height: 100%; display: flex; align-items: center; padding-left: 24px;}#vhrHeaderRow3{height: 100%; display: flex; align-items: center; padding-left: 24px;}#vhrHeaderRow4{height: 100%; display: flex; align-items: center; padding-left: 24px;}#vhrHeaderRow5{height: 100%; display: flex; align-items: center; padding-left: 24px;}/************************************* SUMMARY************************************/ td.statCol>td.ishrep{padding-left: 8px;}.summaryOdd{background-color: #EEEEEE;}.summaryEven{background-color: #ffffff;}.summaryAlert{background-color: #ffffdd;}.tcCopy, .eventCol>a:first-child{padding-left: 16px;}.tcCopy a, .tcCopy a:visited{text-decoration: underline !important; border: 0 !important;}.bbg-info-row{padding: 16px 27px 16px 0; min-height: 43px;}.bbg-image{height: 100%; float: left; margin: -4px 16px 0 16px;}.bbg-alert-image{height: 50px;}.large-title-history-alert{color: #F44336; font-size: 18px; font-weight: 700;}#bbg-paragraph{margin-left: 15px;}#bbgPara{text-align: left;}#bbg-paragraph>a:first-of-type::before{content: \"A\"; white-space: pre;}#guaranteeReg, #bbgCertificate{display: inline;}.ownerColumns1{width: 25%;}.ownerColumns2{width: 16%;}th.backToTopHeading{border-right: 1px solid #BDBDBD;}.statCol{border-top: 0px none #BDBDBD; border-right: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; text-align: center; padding: 3px; overflow: hidden;}/* When there is 1 owner */ th:nth-of-type(3):nth-last-child(1), th:nth-of-type(3):nth-last-child(1)~.statCol{width: 22%;}/* When there is 2 owners */ th:nth-of-type(3):nth-last-child(2), th:nth-of-type(3):nth-last-child(2)~.statCol{width: 21.2%;}/* When there is 3+ owners */ th:nth-of-type(3):nth-last-child(3), th:nth-of-type(3):nth-last-child(3)~.statCol{width: 18.8%;}.eventCol{border-top: 0px solid #BDBDBD; border-right: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; text-align: left; padding: 3px;}#summaryOwnershipHistoryTable .eventCol{padding: 12px 3px;}#otherInformationTable .eventCol{padding: 8px 3px;}#oneOwnerLogo{position: absolute; z-index: 5; margin: 4px 0 0 -310px; float: left; width: 180px;}#alertIcon{position: absolute; margin-top: -5px; padding-left: 12px; z-index: 9;}.alertModule{border: 1px solid #FF3B30; border-radius: 4px; margin-bottom: 15px; padding: 8px 16px; display: flex; align-items: center; background-color: rgba(255, 59, 48, 0.05);}.alert-icon{padding: 0 17px 0 0;}.bbg-alert-strong{color: #F44336; font-weight: 500;}.alertModule strong{color: #cf1313;}.iconAndText{margin: auto; width: 90%; display: flex; justify-content: center; align-content: center;}.iconAndText img{width: 20px; height: 26px;}.iconAndText div{text-align: left; padding-left: 12px; margin-top: auto; margin-bottom: auto;}/* align warranty and recall checkmarks with others */ #columnWarrantyCellResultTxt1>div:nth-child(1)>div:nth-child(2){padding-right: 26px;}#columnWarrantyCellResultTxt2>div:nth-child(1)>div:nth-child(2){padding-right: 24px;}#columnWarrantyCellResultTxt3>div:nth-child(1)>div:nth-child(2){padding-right: 24px;}#columnRecallResultTxt1>div:nth-child(1)>img:nth-child(1){padding-left: 4px;}#columnRecallResultTxt2>div:nth-child(1)>img:nth-child(1){padding-left: 4px;}#columnRecallResultTxt3>div:nth-child(1)>img:nth-child(1){padding-left: 4px;}/* align warranty and recall checkmarks with others */ #columnWarrantyCellResultTxt1>div:nth-child(1)>div:nth-child(2){padding-right: 26px;}#columnWarrantyCellResultTxt2>div:nth-child(1)>div:nth-child(2){padding-right: 24px;}#columnWarrantyCellResultTxt3>div:nth-child(1)>div:nth-child(2){padding-right: 24px;}#columnRecallResultTxt1>div:nth-child(1)>img:nth-child(1){padding-left: 4px;}#columnRecallResultTxt2>div:nth-child(1)>img:nth-child(1){padding-left: 4px;}#columnRecallResultTxt3>div:nth-child(1)>img:nth-child(1){padding-left: 4px;}/************************************* DETAILS************************************/ #endOfOwnerShipDate{display: block; text-align: left; vertical-align: top; color: #fff;}.rolocolumn{padding: 0 0 0 6px; text-align: left; vertical-align: top; color: #fff;}.folderstyle{width: 100%;}.folderstyle td{padding: 3px;}.folderstyle th{padding: 4px; text-align: left;}.last-detail-row{border-radius: 0 0 4px 4px;}.mileage{text-align: right;}.evenrow td, .evenrow{border-top: 1px solid #BDBDBD; vertical-align: top;}.folderstyle>.evenrow:last-of-type{border-radius: 0 0 4px 4px;}.printrow td{background-color: #f3f3f3; border-top: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; vertical-align: top; text-align: center;}.oddrow td, .oddrow{background-color: #EEEEEE; border-top: 1px solid #BDBDBD; vertical-align: top;}.cpoTile{margin: 8px 0;}.cpoSVG-max{max-width: 180px;}.ownerTabPrint{background-color: #4776a9; width: 170px;}.buttress{background-color: #ffffff !important; text-align: right !important;}.bornOnTxt{margin-bottom: 0px; margin-top: 10px; font-size: 12px;}.invisibleText{position: relative; display: none; z-index: 2;}.details-column{display: flex; flex-direction: column;}.details-owners-container{border-top: 1px solid #3777BC; border-right: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD;}.details-owner-container{display: block; /* Do not change this: Firefox can't print display flex for large containers */ border: 1px solid #BDBDBD; margin: 24px 16px 0 16px; border-radius: 4px;}.details-row-header{background-color: #E0E0E0; border-radius: 4px 4px 0 0; font-weight: 500; padding: 4px 0;}.details-owner-row>div{width: 49%;}.details-row>div:nth-child(1){width: 8%;}.details-row>div:nth-child(2){width: 7%;}.details-row>div:nth-child(3){width: 28%;}.details-row>div:nth-child(4){width: 2%;}.details-row>div:nth-child(5){width: 40%;}.data-research .details-row>div:nth-child(1){word-wrap: break-word; width: 9%;}.data-research .details-row>div:nth-child(2){width: 10%;}.data-research .details-row>div:nth-child(3){width: 11%;}.data-research .details-row>div:nth-child(4){width: 10%;}.data-research .details-row>div:nth-child(5){width: 22%;}.data-research .details-row>div:nth-child(6){width: 5%;}.data-research .details-row>div:nth-child(7){width: 30%;}.detail-icon-show{display: block;}.detail-icon-hide{display: none;}.vehicleRecordSource{overflow-wrap: break-word;}/* comparability hack for IE11 */ .vehicleRecordSource>.detailLineUrl{word-break: break-all; display: inline-block;}.windowStickerIcon{padding-bottom: 8px; width: 128px; height: 86px; display: block;}/*** Severity Scale Desktop ***/ #severity-container{height: 87px; width: 278px; display: flex; flex-direction: column; align-items: center; margin: 0 0 10px 0; position: absolute; top: 0; z-index: 1;}.severity-yellow-fill-container{position: relative; z-index: 0; top: 4px; bottom: 0; height: 99px; width: 278px; margin-left: 10px; margin-bottom: 5px; border: 1px solid #E0E0E0; border-radius: 3px;}.scale-text{color: #424242; letter-spacing: -0.24px; display: flex; margin-top: 15px; margin-bottom: 10px; align-items: center; max-height: 20px; font-size: 12px; font-weight: bold;}.severity-scale{width: 278px; height: 99px; position: relative; color: #424242; border: 1px solid #e0e0e0; border-radius: 3px; display: flex; flex-direction: column; align-items: center; justify-content: center; margin-bottom: 5px; margin-left: -5px;}.severity-scale>div{position: relative;}.severity-scale-fill{width: 100%; height: 100%; position: absolute; top: 0; left: 0;}.severity-scale-label{font-size: 12px; font-weight: bold; letter-spacing: -.24px; margin-bottom: 15px;}.severity-scale-info-icon{color: #3777bc; cursor: pointer; font-size: 16px; position: relative; top: 4px; left: 2px;}.poi div.severity-scale-slider{width: 212px; height: 12px; border: 1px solid #212121; position: relative; text-align: left;}.severity-scale svg{position: relative; border: 1px solid #212121; margin-top: -4px; height: 19px;}.VerySevereDamageSeverity{margin-left: 186px; width: 25px;}.SevereDamageSeverity{margin-left: 166px; width: 50px;}.ModerateToSevereDamageSev{margin-left: 120px; width: 50px;}.ModerateToSevereDamageSevStructural{margin-left: 90px; width: 122px;}.ModerateDamageSeverity{margin-left: 80px; width: 50px;}.MinorToModerateDamageSev{margin-left: 35px; width: 50px;}.MinorToModerateDamageSevStructural{margin-left: 25px; width: 110px;}.MinorDamageSeverity{margin-left: -4px; width: 50px;}.VeryMinorDamageSeverity{margin-left: -1px; width: 25px;}.severity-scale-ruler{display: flex; justify-content: space-around; font-size: 10px; width: 100%; margin-top: 10px;}.off-center{position: relative; left: 10px;}.center{align-items: center; justify-content: center;}.scale-container{display: flex; flex-direction: column; align-items: center; justify-content: center; height: 70px;}.scale-rectangle{height: 12px; width: 212px; border: 1px solid #212121; position: relative;}.severity-legend{display: flex; justify-content: space-between; width: 232px; z-index: 1; padding-top: 7px;}.severity-type{display: flex; justify-content: center; height: 15px; color: #212121; letter-spacing: -0.16px; padding: 0 3px; font-size: 10px;}.absolutely{position: absolute; top: 0;}.severity-indicator-container-narrow{position: relative; height: 20px; width: 27px;}.severity-indicator-container{position: relative; height: 20px; width: 52px;}.white-fill{position: relative; top: 0; left: 0; height: 100%; width: 100%;}#scale-indicator{display: flex; z-index: 100; position: absolute; height: 100%; width: 100%; top: 0;}.slider-box{display: flex;}.yellow-fill{width: 100%; height: 100%; position: relative; top: 0;}.pad-left{padding-left: 10px;}.severity-scale-popup{display: none; background: white; padding: 3em; width: 90%; max-width: 600px;}.severity-scale-popup-text-container .severity-scale-popup-text, .severity-scale-popup-text-container .severity-scale-popup-text div, .severity-scale-popup div{text-align: left;}.severity-scale-popup-text-container{display: flex; justify-content: space-between;}.severity-scale-popup-text{font-size: 16px;}.severity-scale-popup-text strong{font-weight: 500; padding-bottom: 12px; display: block;}.severity-scale-popup ul{line-height: 1.2em; list-style: disc; margin: 12px 16px;}.severity-scale-popup div>ul{margin: 12px 12px 12px 30px;}.severity-scale-popup div>ul>ul{margin: 12px 16px;}.close-modal, .close-global-modal, .severity-scale-info-icon{cursor: pointer;}.severity-scale-popup button{background-color: #7DC243; height: 38px; width: 86px; border-radius: 4px; border: 0; color: #fff; text-align: center; margin-top: 2em; font-size: 14px;}/* Ratings Histogram */ .histogram-popup{width: 648px; border-radius: 4px; box-shadow: 0 15px 12px 0 rgba(0, 0, 0, 0.22), 0 19px 38px 0 rgba(0, 0, 0, 0.3); display: none; flex-direction: column; background-color: #fff;}.histogram-popup-title{display: flex; justify-content: space-between; align-items: center; height: 68px;}.histogram-popup-title h3{font-size: 18px; font-weight: 500; padding: 28px 0 22px 24px;}.histogram-popup-title i{padding-right: 24px;}.histogram-popup-ratings-container{display: flex;}.histogram-popup img{height: 24px;}.histogram-popup .ratings-container svg{height: 24px; width: 24px;}.histogram-popup .ratings-container{justify-content: center;}.histogram-popup .ratings-verified-reviews{font-size: 14px; color: #9e9e9e; font-weight: 400; line-height: 24px; text-decoration: none; padding-right: 8px;}.histogram-popup-ratings-summary h4{text-align: center; font-size: 14px; font-weight: 500;}.histogram-popup-ratings-summary{padding: 24px 32px 32px 24px;}.histogram-popup .ratings-stars-container{margin-bottom: 8px; flex-wrap: wrap;}.histogram-popup .ratings-stars-container .ratings-average-value{width: 100%; text-align: center; font-size: 64px; color: #3777BC; height: 72px; font-weight: 600; line-height: 72px;}.histogram-popup-chart-container{width: 432px; padding-right: 48px; padding-left: 20px; display: flex; flex-direction: column; justify-content: center;}.histogram-popup-bar-container{display: flex; justify-content: space-between; margin-bottom: 8px;}.histogram-popup-bar-container:last-child{margin: 0;}.histogram-popup-chart-container div{font-size: 14px;}.histogram-popup-bar-container>div:last-child{width: 27px;}.histogram-progress-star-label{width: 12.5%; min-width: 40px;}.histogram-popup .histogram-progress-percentage{color: #9E9E9E; width: 12.5%; min-width: 36px; text-align: right;}.histogram-progress-bar{background: #3777BC;}.histogram-progress-background{width: 272px; background: #F5F5F5; flex-grow: 1;}.material-icons.md-18{font-size: 16px; line-height: 2;}.material-icons.blue{color: #3777bc;}.ratings-container svg{height: 24px; width: 24px; flex: none;}.histogram-progress-star-label{width: 12.5%; min-width: 40px;}.histogram-progress-bar{background: #3777BC;}.histogram-progress-background{background: #F5F5F5; flex-grow: 1;}/************************************* GLOSSARY/FOOTER************************************/ dl{padding: 5px;}dt{text-align: left; font-weight: 500;}dd{margin-bottom: 10px; text-align: left;}dd .inset{margin: 10px 20px; text-align: left;}dd ul{margin: 0 20px 20px 20px; list-style: disc;}dd li{margin-top: 5px; text-align: left;}dd strong{font-weight: 500;}/************************************* HOTLISTINGS************************************/ .results{padding: 4px; background-color: #d7deda; margin-bottom: 4px;}table.premiumResultSet{border: #c8ccc8 1px solid; margin-bottom: 2px; background-color: #ffffe6; line-height: 135%;}table.premiumResultSet td{padding: 3px;}table.resultSet{border: #c8ccc8 1px solid; padding: 5px; margin-bottom: 2px; background-color: #ffffff;}table.resultSet td{padding: 3px;}.results .premOrange{width: 35%;}.results img.cfxlogo{border: 0;}#zipInput{padding: 1px;}.hlElem{padding-left: 6px; text-align: left; margin-bottom: 5px;}/************************************* NIL************************************/ .dlrNILModule, .dlrContactModule{border: 3px solid #98a3b1; margin-bottom: 15px; padding: 5px 10px; background-color: #ebeff4; float: none;}.dlrNILModule h2{margin: 5px 0;}.dlrContactModule h2{width: 100%; border-bottom: 1px dotted #999999; padding: 3px 0;}.dlrContactModule .NILcontainer{margin-top: 3px; padding: 5px 0; float: left; width: 100%;}.contactForm{float: left; width: 48%; padding-left: 10px; border-left: 2px solid #98a3b1;}.cFormIntro{display: block; margin-bottom: 4px;}.cFormEntry{display: block;}.dlrContact{width: 48%; float: left; line-height: 130%;}address{font-style: normal;}h4 a, h4 a:visited{color: #0000cc; margin: 5px 0; display: block;}.entryLabel{float: left; width: 27%; text-align: right; padding: 4px 3px 0 0;}.entryInput{float: left; padding-bottom: 3px;}/*NILBottom*/ .nameInLights{border: 3px solid #98a3b1; margin: 0 auto; margin-bottom: 15px; background-color: #ebeff4;}#dlrNIL{padding: 10px 0 0 0; text-align: left}.dlrNILloc{border-bottom: 1px dotted rgb(145, 156, 176); text-align: left;}.dlrNILloc h2{display: inline}#dlrNIL h2{margin: 0px; text-align: left}#dlrNIL a:visited{color: #0000ff;}#dlrInfo h2{margin-bottom: 0px; margin-left: 7px;}address{font-style: normal;}.faded{color: #999999;}.normaltxt{color: #000000;}#addressBlock{width: 220px; padding: 2px;}#dlrNIL ul, #dlrInfo ul{margin: 0px 3px 3px 3px; padding: 0px;}#dlrNIL li, #dlrInfo li{padding-left: 15px; list-style: none; margin: 0px; line-height: 17px;}#dlrDaP{background: url(https://api.lot.report/media/carfax/map.gif) no-repeat 0px 2px;}#dlrPhone{background: url(https://api.lot.report/media/carfax/phone.gif) no-repeat 1px 3px;}#dlrLink{background: url(https://api.lot.report/media/carfax/globe.gif) no-repeat 0px 3px;}#findOther{background: url(https://api.lot.report/media/carfax/search.gif) no-repeat 0px 3px;}#nilMsg{padding: 3px;}.NILwrapper{background-color: #f5f5f5; padding: 5px;}#twocol address{width: 60%; float: left; text-align: left;}#twocol #dlrLinks{width: 30%; float: left; text-align: left;}#onecol address{width: 51%; float: left; text-align: left; padding-left: 10px;}/************************************* XPERT STYLES************************************/ div.xpertrow{border-top: 0px none; text-align: right !important; display: flex; justify-content: flex-end; padding: 20px 12px 8px 0;}div.uclLeadFormRow{padding: 20px 12px 8px 12px; justify-content: flex-start;}div.xpertrow.fauxWide{padding-top: 6px; border-top: 1px solid #BDBDBD;}.xpertSmall{width: 240px; clear: both; text-align: right;}.xpertLarge{width: 56%; text-align: right;}.xpertFauxWide{width: 100%; margin: 0 8px 0 34px;}.xpertLargeImage img{height: 97px; width: 118px;}.xpertDetailInRecord{background-image: url(https://api.lot.report/media/carfax/xpert-details-infer-record-bubble.gif); background-repeat: no-repeat; background-position: bottom; padding: 0px 4px 10px 25px; vertical-align: top; min-height: 40px; height: auto !important; height: 40px; text-align: left; line-height: 110%;}.xpert-wrapper{width: 100%; align-self: center;}.xpertLongFormat{background-color: #FFFBE7; border: 1px solid #BDBDBD; border-radius: 4px; padding: 14px 28px; vertical-align: top; height: auto; text-align: left; width: auto;}.speech-bubble-arrow{bottom: -28px; position: relative; right: 15px;}.speech-bubble-arrow::before{border-style: solid; border-width: 6px 9px 6px 0; border-color: transparent #BDBDBD; content: \"\"; position: absolute; left: -23px; top: -66px;}.speech-bubble-arrow::after{border-style: solid; border-width: 6px 9px 6px 0; border-color: transparent #FFFBE7; content: \"\"; position: absolute; left: -22px; top: -66px;}.speech-bubble-arrow.wide::before{left: 6px; top: -66px;}.speech-bubble-arrow.wide::after{left: 7px; top: -66px;}.xpertLongFormat .xpertText{float: right; padding-right: 4px; width: 265px; text-align: left;}.xpertLongFormat ul{margin-left: 15px; list-style: square;}.xpertLongFormat li{margin-top: 3px;}.xpertTab{background-image: url(https://api.lot.report/media/carfax/xpert_tabbg-bubble.gif); background-repeat: no-repeat; background-position: bottom; padding: 0; vertical-align: top; min-height: 101px; height: auto !important; height: 101px; text-align: left;}.carfoxTab{padding-left: 14px; display: flex;}.carfoxTab img{align-self: flex-end; padding-right: 10px; height: 78px;}.carfoxTabContent{padding: 8px; /*min-width needed for IE*/ min-width: 230px; max-width: 250px; height: 54px; line-height: 18px; background-color: #FFFBE7; align-self: center; border: 1px solid #BDBDBD; border-radius: 4px;}.carfoxTabContent:before{content: \"\"; position: relative; display: block; width: 0; top: 14px; left: -17px; border-style: solid; border-width: 6px 9px 6px 0; border-color: transparent #BDBDBD;}.carfoxTabContentText{margin-top: -10px;}.carfoxTabContent:after{content: \"\"; position: relative; display: block; width: 0; top: -42px; left: -16px; border-style: solid; border-width: 6px 9px 6px 0; border-color: transparent #FFFBE7;}.owner-tab-balancing-container{width: 20%;}#wmXpertImage{margin-top: auto;}/*********************** * NON MODULE STYLES ************************/ #RRR{margin: 0 auto; height: 25px; background-image: url(https://api.lot.report/media/carfax/blank640grey.gif); background-repeat: no-repeat; padding: 2px 0 0 0;}#RRR .rightpad{padding-right: 7px; text-align: right;}#RRR .leftpad{padding-left: 7px; text-align: left;}#RRR #runAnotherTd{float: left; text-align: left; display: inline; padding-left: 7px;}#RRR .print{float: right; display: inline; text-align: right; padding: 3px 10px;}#flyout{float: left; position: absolute; z-index: 999; left: -1599em; margin: 2px 0 0 -1px; padding: 0px; height: 170px; width: 343px;}#emailAnnex{background-color: #F8F8F8; padding: 10px; height: 160px; width: 343px; border: 1px solid #999999;}#emailAnnex input{margin-bottom: 5px;}#emailAnnex textarea{margin-bottom: 5px;}#emailAnnex td, #emailAnnex tr{padding: 0px; margin-left: 5px; margin-top: 0px; text-align: left;}#annexMsg{color: #ff0000; margin: 0px auto; text-align: left; display: none;}#RRRmsg{color: #ff0000; width: 140px; margin: 0px auto; text-align: center; display: block;}#forPersonalUse{float: left; width: 40%; text-align: left; padding: 0;}#emailReport{float: left; text-align: left; padding: 2px 7px; display: inline;}/*upModules*/ #organicUpSellBanner{background-color: #e5ecf9; padding: 10px; margin-bottom: 15px; color: #003366;}.xLink{text-align: center; margin-bottom: 24px;}td .xLink{margin: 10px 0;}#otherInfoModule .xLink{margin: 3px 0;}.tooltip{display: none;}/************************ MODAL************************/ #modal{display: none; position: fixed; _position: absolute; /* hack for internet explorer 6*/ width: 518px; z-index: 51; height: 308px;}#backgroundPopup{display: none; position: fixed; _position: absolute; height: 100%; width: 100%; top: 0; left: 0; background: #000000; border: 1px solid #cecece; z-index: 50;}a.closeModal, a.closeModal:visited{text-decoration: none;}a.closeModal:hover{text-decoration: underline;}.closeModalBtn{text-align: left; margin-left: -10px; z-index: 199; margin-top: -325px;}#flashcontent{width: 518px;}/************************* DEALER CONTACT FORM *************************/ #dlrNIL{margin-bottom: 12px;}#dlrNIL h2{margin-bottom: 0px;}#dlrNIL a:visited{color: #0000ff;}#dlrInfo h2{margin-bottom: 0px;}#addressBlock{width: 220px; padding: 2px;}#dlrNIL ul, #dlrInfo ul{margin: 0px 3px 3px 3px; padding: 0px;}#dlrNIL li, #dlrInfo li{padding-left: 15px; list-style: none; margin: 0px; line-height: 17px;}#dlrMap{background: url(https://api.lot.report/media/carfax/map.gif) no-repeat 0px 2px;}#dlrPhone{background: url(https://api.lot.report/media/carfax/phone.gif) no-repeat 1px 3px;}#dlrLink{background: url(https://api.lot.report/media/carfax/globe.gif) no-repeat 0px 3px;}#findOther{background: url(https://api.lot.report/media/carfax/search.gif) no-repeat 0px 3px;}#nilMsg{padding: 3px;}.hdrNIL #dealer-name{font-weight: 700; font-size: 14px; padding-top: 8px;}#dealerInfo{padding: 0px 0px 10px 10px; text-align: left; vertical-align: top; width: 346px;}#dealerLeadTd{border-left: 2px solid rgb(204, 204, 204); padding: 5px 5px 0px 5px; text-align: left; vertical-align: top; width: 310px;}#phoneErrorMsg{padding: 3px;}.nameInLight{border: 0px; width: 656px; background: url(https://api.lot.report/media/carfax/blue.gif);}#contactDealerFormMini tr td{padding: 2px}#dealerInfo div#dealer-name a#dealer-name-link, #dealerInfo div#dealer-name{margin: 10px 0px 5px 0px;}/*************************** ** Social Media Links *****************************/ .socialLinks{line-height: 16px; font-size: 12px; margin-bottom: 24px; font-weight: bold;}.socialLinks img{vertical-align: middle; margin-right: 5px;}.socialLinks a{text-decoration: none; margin-left: 12px;}.fbBlue, .fbBlue:hover, .fbBlue:visited{color: #3B5998;}.twitterBlue, .twitterBlue:hover, .twitterBlue:visited{color: #55ACEE;}.carfaxIconBlack, .carfaxIconBlack:hover, .carfaxIconBlack:visited{color: #212121;}#topbarSocial{float: right; text-align: right; padding-top: 13px; margin-right: 15px;}.fb_edge_comment_widget{display: none !important;}/*************************** ** footer signature *****************************/ #signatureModule{border: 1px solid #BDBDBD; border-radius: 4px; background-color: #EEEEEE; margin: 24px 0; padding: 24px; font-weight: 500; height: 130px; display: flex; flex-direction: column; justify-content: space-between;}#signatureModule #signatureLines{display: flex; justify-content: space-between;}#signatureModule .signatureLine{border-top: 1px solid #979797; width: 48%; padding-top: 8px; display: flex; justify-content: space-between;}#signatureModule .signatureLine .sig{padding-left: 5px;}#signatureModule .signatureDate{margin-right: 16%;}/**Deprecated Signature styles**/ #signatureModule.mercedes{background-color: white;}#signatureModule.audi{background-color: #f5f5f5;}#signatureModule.mercedes #signatureText{width: 55%;}/**End Deprecated Signature styles**/ #cfxHdrBar{height: 64px; background-color: #3777BC; display: flex; padding: 12px 0 2px 17px; -webkit-box-sizing: border-box; -moz-box-sizing: border-box; box-sizing: border-box; border-radius: 4px 4px 0 0;}#cfxHdrBar img{margin-right: 10px;}.cfx-hdr-text{font-size: 24px; font-weight: 500; color: #FFFFFF; margin-top: 8px;}.cfx-hdr-text sup{font-size: 8px; vertical-align: text-top;}.hdrWrap{display: flex; background: #ebeff7; border-color: #BDBDBD; border-width: 0 1px 1px 1px; border-style: solid; border-radius: 0 0 4px 4px; -webkit-box-sizing: border-box; -moz-box-sizing: border-box; box-sizing: border-box; height: 294px;}.header-right-container{display: flex; justify-content: space-between; width: 100%;}.report-price-bubble{height: 24px; width: 80px; border-radius: 50px; background-color: #2C5F96; color: #fff; font-size: 12px; display: flex; align-items: center; justify-content: center; margin-top: -4px; margin-right: 16px;}.vehicleInformationSection{background: #ebeff7; border-radius: 0 0 0 4px; width: 40%; padding: 8px 16px 8px 16px; line-height: 115%; -webkit-box-sizing: border-box; -moz-box-sizing: border-box; box-sizing: border-box; display: flex; flex-direction: column; justify-content: space-between;}.vTbl{width: 60%; background: #fffbe7; min-height: 200px; position: relative; -webkit-box-sizing: border-box; -moz-box-sizing: border-box; box-sizing: border-box; border-radius: 0 0 4px 0; border-left: 1px solid #BDBDBD;}.icoCol{text-align: center; height: 48px; width: 45px; border-bottom: 1px solid #BDBDBD; border-right: 1px solid #BDBDBD;}.DescCol{border-bottom: 1px solid #BDBDBD; text-align: left; vertical-align: middle; height: 48px;}.DescCol strong{display: inline-block; float: left; margin: 0px 4px 0 0; vertical-align: middle;}.vTbl tr:nth-child(6) td{border-bottom: none;}.vTbl table{width: 100%; border-collapse: collapse;}.vTbl tr{cursor: pointer; background: none;}.rowshading{background-image: url(https://api.lot.report/media/carfax/XDescbg.gif) !important; background-repeat: repeat-y;}.rowInfo{display: none; position: absolute; width: 280px; padding: 10px; background: #fff1a8; border: 1px solid #dfd061; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; -webkit-box-shadow: 0 1px 4px rgba(50, 50, 50, 0.75); -moz-box-shadow: 0 1px 4px rgba(50, 50, 50, 0.75); box-shadow: 0 1px 4px rgba(50, 50, 50, 0.75); overflow: hidden}#xpertWMNew{position: absolute; top: 138px; width: 65%;}.headerCarfox{position: absolute; bottom: -1px; right: 0;}.defaultHeaderCarfox{transform: translate(7.8%, 0%); width: 39%;}.regularOilChangesHeaderCarfox{transform: translate(0.5%); width: 40.7%;}.newCarHeaderCarfox{width: 39.7%; transform: translate(-1.5%, 6%);}.xpertWMNewBox{background-color: #FFFFFF; border: 1px solid #BDBDBD; border-radius: 4px; position: absolute; top: 39px; right: 21%; height: auto; width: 61%; padding: 4% 5% 4% 4%; text-align: left;}.xpertWMNewBox:after, .xpertWMNewBox:before{left: 100%; top: 30%; border: solid transparent; content: \" \"; height: 0; width: 0; position: absolute; pointer-events: none;}.xpertWMNewBox:after{border-color: rgba(255, 255, 255, 0); border-left-color: #FFFFFF; border-width: 7px; margin-top: -7px;}.xpertWMNewBox:before{border-color: rgba(235, 231, 203, 0); border-left-color: #BDBDBD; border-width: 8px; margin-top: -8px;}.wmActivateList .cta{color: #333; line-height: 26px; padding-top: 8px;}.wmActivateList{margin-left: 35px;}.wmActivateList h5{line-height: 22px; font-size: 24px; color: #3777BC; padding-bottom: 12px;}.shCopy{padding-left: 8px;}.wmActivateList ul li{list-style: none; line-height: 22px;}.wmActivateList ul li::before{content: \"Ã¢â‚¬Â¢\"; margin-right: 25px; position: relative; top: 0em; font-size: 11px;}.wmvisitbox{padding: 0 10px 10px 10px; text-align: center; line-height: 30px; font-size: 16px; margin-top: 15px;}.wmvisitbox strong{display: block; padding-bottom: 15px;}.wmvisitbox h4{line-height: 16px;}.wmvisitbox h4.noncsn{margin: 10px 0 30px 0;}.xpertWMNewBox p strong{line-height: 16px;}.left{float: left;}.serviceHistoryStatCol>*{vertical-align: middle; color: #3777BC;}.serviceHistoryStatCol img{padding-right: 8px;}#serviceHistoryDevice{position: relative; left: 57px;}/*trueframe popup */ #externalsitewarning{display: none;}#externalsitewarning{width: 100%; height: 100%; z-index: 20; position: absolute; top: 0; left: 0;}#externalsitewarning .modalFrost{width: 100%; height: 100%; background: #333; opacity: 0.6;}#externalsitewarning .modalClose{float: right; margin-right: -16px; margin-top: -16px; width: 32px; height: 32px; cursor: pointer;}#externalsitewarning .modalBox{width: 500px; margin: 0 auto; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; -moz-box-shadow: 0 1px 3px 0 #ccc; -webkit-box-shadow: 0 1px 3px 0 #ccc; box-shadow: 0 1px 3px 0 #ccc; border: 4px solid #fff; background: #e4eff1; position: fixed; top: calc(50% - 82px); left: calc(50% - 250px);}#externalsitewarning .modalContent{padding: 20px 10px; line-height: 150%; text-align: center;}#externalsitewarning .confirm{-moz-box-shadow: inset 0px 1px 0px 0px #bbdaf7; -webkit-box-shadow: inset 0px 1px 0px 0px #bbdaf7; box-shadow: inset 0px 1px 0px 0px #bbdaf7; background: -webkit-gradient(linear, left top, left bottom, color-stop(0.05, #79bbff), color-stop(1, #378de5)); background: -moz-linear-gradient(center top, #79bbff 5%, #378de5 100%); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#79bbff', endColorstr='#378de5'); background-color: #79bbff; -moz-border-radius: 6px; -webkit-border-radius: 6px; border-radius: 6px; border: 1px solid #84bbf3; display: inline-block; color: #ffffff; padding: 6px 24px; text-decoration: none; text-shadow: 1px 1px 0px #528ecc; margin: 20px;}#externalsitewarning .confirm:hover{background: -webkit-gradient(linear, left top, left bottom, color-stop(0.05, #378de5), color-stop(1, #79bbff)); background: -moz-linear-gradient(center top, #378de5 5%, #79bbff 100%); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#378de5', endColorstr='#79bbff'); background-color: #378de5;}#externalsitewarning .confirm:active{position: relative; top: 1px;}#externalsitewarning .btnCancel{-moz-box-shadow: inset 0px 1px 0px 0px #ffffff; -webkit-box-shadow: inset 0px 1px 0px 0px #ffffff; box-shadow: inset 0px 1px 0px 0px #ffffff; background: -webkit-gradient(linear, left top, left bottom, color-stop(0.05, #ededed), color-stop(1, #dfdfdf)); background: -moz-linear-gradient(center top, #ededed 5%, #dfdfdf 100%); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#ededed', endColorstr='#dfdfdf'); background-color: #ededed; -moz-border-radius: 6px; -webkit-border-radius: 6px; border-radius: 6px; border: 1px solid #dcdcdc; display: inline-block; color: #777777; padding: 6px 24px; text-decoration: none; text-shadow: 1px 1px 0px #ffffff; margin: 20px;}#externalsitewarning .btnCancel:hover{background: -webkit-gradient(linear, left top, left bottom, color-stop(0.05, #dfdfdf), color-stop(1, #ededed)); background: -moz-linear-gradient(center top, #dfdfdf 5%, #ededed 100%); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#dfdfdf', endColorstr='#ededed'); background-color: #dfdfdf; cursor: pointer;}#externalsitewarning .btnCancel:active{position: relative; top: 1px;}.iucl-heading{color: #3777bc; padding: 15px 0 0 15px;}#ucl-module input[type=\"checkbox\"]{margin: 0; padding: 0;}#ucl-module .iucl-showme-fox{top: 45px !important;}#addVinAlertButton{margin: 0 auto; text-align: left;}#addVinAlertButton button{background: #76b93f; color: #fff; zoom: 1; height: 34px; padding: 0 10px; border: 1px solid #76b93f; -webkit-border-radius: 4px; -moz-border-radius: 4px; border-radius: 4px; cursor: pointer; vertical-align: middle;}#addVinAlertButton p{padding: 10px 0px;}#addVinAlertWatchButton>i{display: none;}#addVinAlertWatchButton:disabled>span{display: none;}#addVinAlertWatchButton:disabled>i{display: inline;}.noPrint>td:not(.buttress)>img, .noPrint>img, .nopad>img, .xpertImg{display: block;}.xpertImg{margin-bottom: -8px; margin-right: 20px;}.dealer-web-address{word-wrap: break-word;}#checklist .dealer-web-address{max-width: 200px;}/* Trade In Leads */ .trade-in-leads-banner{height: 56px; margin-top: -10px; margin-bottom: 18px; background: #FFF0BF; display: flex; align-items: center; justify-content: center; border-radius: 4px;}.tradeInLeadsBtn{height: 40px; padding: 0px 15px; border-radius: 4px; background-color: #7DC243; cursor: pointer;}.tradeInLeadsBtn:hover{background-color: #3bac3b;}.tradeInLeadsBtnTxt{color: #FFFFFF;}.sellYourCar{font-size: 18px; display: flex; margin-right: 20px; color: #424242;}.threeSteps{font-weight: 700; padding-left: 5px;}}@media screen and (max-width: 1280px){.hdrWrap{flex-direction: column; height: auto;}.vehicleInformationSection{width: 100%; flex-direction: row;}.vTbl{width: 100%; border-left: none; border-top: 1px solid #BDBDBD; border-radius: 0 0 4px 4px; min-height: auto;}.vTbl tr:last-child td{border-bottom: none;}.vTbl.wellmaintained tr:last-child:nth-child(3) td{border-bottom: 1px solid #BDBDBD;}.regularOilChangesHeaderCarfox{transform: translate(0.5%); width: 245px;}.defaultHeaderCarfox{transform: translate(0%, 0%); width: 234px; margin-right: 16px;}.newCarHeaderCarfox{width: 248px; transform: translate(-4%, 5%);}#xpertWMNew{position: static; padding: 32px 0 32px 32px; width: 80%;}.xpertWMNewBox{position: static;}.xpertWMNewBox::after, .xpertWMNewBox::before{left: 59.3%; top: 68%;}#headerSectionNameInLights{max-width: 50%; min-width: 30%;}/*Advantage Dealer NIL Top Section*/ div#dealer-name-in-lights{box-sizing: border-box; display: flex; border-radius: 4px; border: 2px solid #3777BC; margin: auto auto 16px auto;}#dealer-name-in-lights .adv-dealer{background-color: #3777BC; max-width: 145px; min-width: 145px; display: flex; justify-content: center; align-items: center;}.adv-dealer #adv-dealer-logo{width: 90px; height: 70px;}div#name-in-lights{padding: 20px 20px 10px 20px; flex: 3; font-size: 14px;}#dnil-report-provided-by-row{display: flex;}#dnil-report-provided-by-row #report-provided-by{flex: 8;}#dnil-report-provided-by-row #dnil-info{margin-top: -3px;}#name-in-lights #dealer-name{font-size: 18px; line-height: 24px; font-weight: 700; width: 90%;}#name-in-lights .dnil-cpo{font-size: 14px;}#name-in-lights #dealer-inventory-link{padding-top: 6px;}#name-in-lights .ratings-stars-container strong{font-size: 16px;}#name-in-lights .favorites .favorites-number{font-size: 16px;}}@media screen and (max-width: 1004px){#xpertWMNew{width: 56vw; padding: 32px 0 32px 4vw;}.xpertWMNewBox{padding: 32px 3vw; width: 50vw;}.xpertWMNewBox::after, .xpertWMNewBox::before{left: 60.1vw; top: 70%;}}@media screen{.printOnly{display: none;}}@page{margin: 1cm;}@media print{/* Rules to make the report print shorter */ body{font-size: 12px; line-height: 105%;}.header-row-text{font-size: 13px; max-width: 200px;}#summaryOwnershipHistoryTable .eventCol{padding: 4px 3px;}.section-header-container{padding-top: 2px;}.bbg-info-row{padding: 8px 27px 16px 0;}#otherInformationTable .eventCol, .details-row>div{padding: 4px 3px;}#otherInformationTable .tcCopy{padding-top: 3px;}.secHdrRow, .secHdrRow-cell{height: 48px;}#glossaryModule .secHdrRow, #glossaryModule .secHdrRow-cell, .secHdrRow.detail-section-label{height: 40px;}.ratings-stars-container, .ratings-verified-reviews, .ratings-verified-reviews-container img{padding: 0;}.ratings-verified-reviews{padding-right: 4px;}/* End rules to make report print shorter */ #RRR, #RRR *{display: none !important;}.noPrint{display: none;}.printOnly{display: block;}.ownerTabPrint{border: 2px solid #4776a9; border-right-width: 0; background-color: #4776a9;}#oneOwnerLogo{margin: 5px 0 0 0; left: 350px; width: 120px;}.xpertTab{margin-top: 15px; background-image: none; border: 0 none;}.xpertDetailInRecord{border: 1px solid #BDBDBD; padding: 2px; background-image: none;}.xpertLongFormat{border: 1px solid #BDBDBD; background-image: none;}.speech-bubble-arrow{display: none;}a{text-decoration: none;}.eventCol a, .eventCol a:visited, a.vhr-tooltip, a.vhr-tooltip:visited{border: 0;}#hdr, #sumOwnModule, #summaryTitleHistory, #otherInfoModule, #hlModule, #detailsModule, #glossaryModule, #sgi-module, #icbc-module{background-color: #ffffff;}#lefthanger, #chiVidTab{display: none !important;}.help{display: none;}#uclLeadFrame{display: none !important;}#value-container>div:first-child{border: 1px solid #BDBDBD;}#cfxHdrBar, .secHdrRow, #glossaryModule .secHdrRow th{border: 1px solid #BDBDBD;}.secHdrRow.sectionHeaderRowDetailedHistory{border-top: 1px solid #BDBDBD; border-bottom: none; border-left: 1px solid #BDBDBD; border-right: 1px solid #BDBDBD;}tr.secHdrRow th:first-child{border-top: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; border-right: none;}tr.summaryOdd td, tr.summaryEven td, #dmvTitleProblemsTable td.eventCol, #dmvTitleProblemsTable td.statCol{border-top: none; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; border-right: none;}tr.secHdrRow .ownerColumnTitle{border-top: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; border-right: none;}tr.secHdrRow .ownerColumnTitle:last-child{border: 1px solid #BDBDBD;}tr.summaryOdd td:last-child, tr.summaryEven td:last-child, #dmvTitleProblemsTable td.eventCol:last-child, #dmvTitleProblemsTable td.statCol:last-child{border-top: none; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; border-right: 1px solid #BDBDBD;}tr.secHdrRow th{border-top: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: none; border-right: none;}div.details-owners-container{border-top: 1px solid #BDBDBD;}.backToTop{display: none;}.glossaryLink, #linkToGlossaryInGlossHead{display: none;}#vhrHeaderRow4{width: 70%;}#vhrHeaderRow5{width: 60%;}#reportBody{width: 98%;}.speech-bubble-arrow{bottom: -28px; position: relative; right: -12px;}.speech-bubble-arrow.wide{bottom: -28px; position: relative; right: 14px;}.speech-bubble-arrow::after{border-color: transparent #FFFFFF;}.carfoxTabContent:after{border-color: transparent #FFFFFF;}.details-row>div:nth-child(5){width: 41%;}.details-row>div:nth-child(2){width: 9%;}.warrantyBottomHalfWhite tr:nth-child(2) td{border-top: 1px solid #BDBDBD;}}.advantageShopLogo{max-width: 90px; max-height: 30px; width: 90px; height: 30px;}@media print{.hdrWrap{height: 100%;}.vTbl{min-height: 0;}}/*Additional History - Text Wrapping - 1 owners*/@media screen and (max-width: 858px){#otherInfoModule tr td:nth-child(2):nth-last-child(1) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(1)~.statCol .iconAndText div{width: 45%;}}@media screen and (max-width: 802px){#otherInfoModule tr td:nth-child(2):nth-last-child(1) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(1)~.statCol .iconAndText div{width: 58%;}}/*End 1-owner text wrapping*//*Additional History - Text Wrapping - 2+ owners*/@media screen and (max-width: 890px){#otherInfoModule tr td:nth-child(2):nth-last-child(2) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(2)~.statCol .iconAndText div{width: 45%;}}@media screen and (max-width: 830px){#otherInfoModule tr td:nth-child(2):nth-last-child(2) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(2)~.statCol .iconAndText div{width: 58%;}}/*End 2-owner text wrapping*//* Back to Top text - 2 owners`*/@media screen and (max-width: 760px){th:nth-of-type(2):nth-last-child(3) .backToTop p{display: none;}th:nth-of-type(2):nth-last-child(3) .back-to-top-link{width: 24px; padding-right: 4px;}}/* Back to Top text - 3+ owners`*/@media screen and (max-width: 1020px){th:nth-of-type(2):nth-last-child(4) .backToTop p{display: none;}th:nth-of-type(2):nth-last-child(4) .back-to-top-link{width: 24px; padding: 0 12px;}}/*Additional History - Text Wrapping - 3+ owners*/@media screen and (max-width: 1000px){#otherInfoModule tr td:nth-child(2):nth-last-child(3) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(3)~.statCol .iconAndText div{width: 45%;}}@media screen and (max-width: 935px){#otherInfoModule tr td:nth-child(2):nth-last-child(3) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(3)~.statCol .iconAndText div{width: 58%;}}/*Title History - 3+ owners*/@media screen and (max-width: 780px){.tcCopy, .eventCol>a:first-child{padding-left: 8px;}#summaryTitleHistory tr td:nth-child(2):nth-last-child(3) .iconAndText div, #summaryTitleHistory tr td:nth-child(2):nth-last-child(3)~.statCol .iconAndText div{display: none;}.section-header-container{margin-left: 12px;}}/*Additional History - align checkmarks on tablet */@media screen and (max-width: 1000px){#columnRecallResultTxt1>div:nth-child(1)>img:nth-child(1){padding-left: initial;}#columnRecallResultTxt2>div:nth-child(1)>img:nth-child(1){padding-left: initial;}#columnRecallResultTxt3>div:nth-child(1)>img:nth-child(1){padding-left: initial;}#columnWarrantyCellResultTxt1>div:nth-child(1)>div:nth-child(2){padding-right: initial;}#columnWarrantyCellResultTxt2>div:nth-child(1)>div:nth-child(2){padding-right: initial;}#columnWarrantyCellResultTxt3>div:nth-child(1)>div:nth-child(2){padding-right: initial;}}/*Additional History - align checkmarks on tablet */@media screen and (max-width: 780px){#warrantyCheckMark1{margin-left: 2px;}#warrantyCheckMark2{margin-left: 2px;}#warrantyCheckMark3{margin-left: 2px;}}/*Additional History - Text Disappearing - 3+ owners*/@media screen and (max-width: 780px){#otherInfoModule tr td:nth-child(2):nth-last-child(3) .iconAndText div, #otherInfoModule tr td:nth-child(2):nth-last-child(3)~.statCol .iconAndText div{display: none;}}@media screen and (max-width: 721px){.cpoTile{display: none;}}@media screen and (max-width: 780px){.owner-bar .carfoxTab{padding-left: 16px;}}@media screen and (max-width: 1020px){/* Ratings Histogram */ .histogram-popup{border: none; border-radius: 0; flex-direction: column; align-items: center;}.histogram-popup-title{width: 100%; height: 64px;}.histogram-popup-title h3{font-size: 24px; font-weight: 500; border: none;}.histogram-popup-title-bar{width: 95%; border-bottom: 1px solid #e0e0e0;}.histogram-popup-ratings-container{justify-content: center; padding-top: 40px;}.histogram-popup-ratings-summary h4{font-weight: 500;}/*.ratings-container svg{*/ /* height: 24px;*/ /* width: 24px;*/ /* min-width: 100%;*/ /*}*/ .histogram-popup .ratings-stars-container .ratings-average-value{font-weight: 500;}.histogram-progress-star-label{width: 12.5%; min-width: 40px;}.histogram-progress-bar{background: #3777BC;}.histogram-progress-background{background: #F5F5F5; flex-grow: 1;}}/* Lead In Section*/@media screen and (max-width: 1050px){#leadIn{margin: 16px 12px;}} @media all and (max-width: 821px){:-ms-fullscreen, :root #wmXpertImage{padding-top: 10px;}}/* Disclaimer and Social Links Sections*/@media screen and (max-width: 1050px){.socialLinks{margin: 16px 12px;}#disclaimerSection{margin: 16px 12px;}}@media screen and (max-width: 1280px){#sumOwnModule, #summaryTitleHistory, #otherInfoModule, #glossaryModule{margin-bottom: 16px;}#leadIn{font-size: 12px; margin-top: 16px; margin-bottom: 16px;}.xLink{margin-bottom: 16px;}.socialLinks{margin-bottom: 16px;}}@media screen and (max-width: 1050px){#oneOwnerLogo{margin-top: 20px; width: 140px; left: 88%;}}@media screen and (max-width: 750px){#oneOwnerLogo{left: 650px;}}.superiority-banner{background-image: url(\"https://api.lot.report/media/carfax/superiority_banner_background.png\"); height: 112px; margin-bottom: 24px; display: flex; align-items: center;}.superiority-banner img:first-child{align-self: flex-end; margin-left: 16px;}.superiority-most-accident{line-height: 1.1; font-size: 26px; font-weight: 700; color: #ffffff; position: relative; left: 30px;}.superiority-record-most-accident{line-height: 1.1; font-size: 22px; font-weight: 700; color: #3777bc; padding-left: 6px; text-align: left;}@media print{.superiority-banner.noPrint{display: none;}.trade-in-leads-banner{display: none;}/*Rules to print the NIL banner section*/ #print-only-nil .print-adv-dealer-logo{width: 90px; padding-right: 20px;}#print-only-nil .report-by-container{width: 100%;}.print-dealer-address{color: #424242; display: flex; line-height: 24px; flex-wrap: wrap;}#print-only-nil .dealer-phone{color: #424242; line-height: 24px;}#print-only-nil #dealer-street1{flex: none;}#print-only-nil #dealer-city-state-zip{flex: none;}#print-only-nil #dealer-street1::after{content: \", \"; white-space: pre;}#print-only-nil #dealer-name a{text-decoration: none; color: #424242; font-size: 18px; font-weight: bold; line-height: 24px;}.print-report-provided-by{color: #3777bc; font-size: 14px; font-weight: 500; line-height: 24px;}#print-only-nil{display: flex; box-sizing: border-box; border-radius: 4px; border: 1px solid #BDBDBD; padding: 15px 20px 10px 20px; margin: auto auto 16px auto; align-items: center; justify-content: space-between;}.print-dnil-cpo{font-size: 14px; color: #424242; font-weight: 500; font-style: italic; line-height: 24px;}.print-rating-text{font-size: 24px; font-weight: 400; color: #424242; line-height: 32px; flex: none;}.print-ratings{display: flex; align-items: center;}.print-ratings-container{display: flex; align-items: center; word-break: normal; flex-direction: column;}.print-ratings-container strong{font-weight: 500;}.print-ratings-container svg{height: 24px; width: 24px; flex: none; padding-right: 7px;}.print-reviews{display: flex;}.print-ratings-verified-reviews{color: #9E9E9E; font-size: 14px; line-height: 24px; padding-right: 6px; flex: none;}.print-reviews img{padding-bottom: 2px;}#print-only-nil .favorites-number{color: #424242; font-size: 24px; font-weight: 500; line-height: 32px;}#print-only-nil .favorites{flex-direction: column;}#print-only-nil .favorites-label{color: #9E9E9E; font-size: 14px; line-height: 24px; flex: none;}.print-fav-and-ratings{display: flex; align-items: center; flex: none;}.print-line{margin-left: 20px; margin-right: 15px; height: 80px; width: 1px; border-right: 1px solid #E0E0E0;}.print-dealerlogo-and-reportby{display: flex;}.print-dealer-logo{display: flex; align-items: center; flex: none;}}/*WARRANTY CHECK*/.warranty-check-container{font-family: Roboto; display: flex; flex-direction: column; max-width: 912px; margin: auto; border: 1px solid #BDBDBD; border-radius: 0 0 4px 4px;}.cfx-logo-container{margin-left: 17px; min-width: 150px;}.warranty-check-mobile-header{display: none;}.warranty-section-header-text{display: flex; align-items: center; font-size: 18px; color: #fff;}#warranty-check-form{display: flex; flex-direction: column; max-width: 98%; margin: auto;}.wc-inner-container{min-width: 95%; margin: 15px; display: flex;}.confirmation-text{padding-top: 16px;}.wc-section-header{display: flex; align-items: center; margin: auto; width: 912px; max-width: 912px; height: 56px; background-color: #3777bc; font-size: 16px; color: #fff; border-radius: 4px 4px 0 0;}#VinPhrase.vin-phrase{display: flex; flex-direction: column; min-width: 0; font-size: 14px; line-height: 1.8;}.warranty-condition-area{padding: 0 20px 24px; /*display: flex;*/ flex-direction: column; min-width: 0;}.weight-700{font-weight: 700;}.weight-500{font-weight: 500;}.grey-details-container{display: flex; height: 233px; border: 1px solid #E0E0E0; border-radius: 4px; background-color: #F5F5F5; margin-bottom: 24px;}.vehicle-details-grid{display: flex; flex-direction: column; width: 55%; padding: 24px; color: #212121;}.vehicle-detail-row{display: flex; justify-content: space-between; align-items: center; height: 40px; border-bottom: 1px solid #bdbdbd;}.vehicle-data{font-size: 16px; font-weight: 700;}.fox-box{display: flex; width: 40%; justify-content: center;}.recalculate-warranty-container{display: flex; flex-direction: column; min-height: 135px; padding-top: 5px;}.warranty-recalculator-row{display: flex; align-items: center; justify-content: space-between; height: 120px; max-width: 505px;}.recalculator-label{font-sze: 14px; line-height: 2.0; font-weight: 500;}.wc-close-window{display: flex; align-items: center; justify-content: center; height: 48px; background-color: #333333; color: #fff; border: 1px solid #424242; border-radius: 4px; min-width: 200px; margin: auto; cursor: pointer;}#warranty-check-form input[type=text]{height: 45px; min-width: 268px; border: 1px solid #E0E0E0; border-radius: 4px; padding-left: 12px;}#warranty-check-form input[type=text]::placeholder{color: #212121; font-size: 14px; text-align: right;}#warranty-check-form input[type=submit]{position: relative; top: 14px; height: 48px; min-width: 200px; border: 1px solid #7DC243; border-radius: 4px; font-family: Roboto; font-size: 16px; font-weight: 400; color: #fff; background-color: #7DC243; cursor: pointer;}.mobile-warranty-info-container{display: none;}.warranty-info-container{display: flex; flex-direction: column; border: 1px solid #E0E0E0; border-radius: 4px 4px 0 0; min-width: 0;}.warranty-info-cols-header{display: flex; align-items: center; height: 39px; width: 100%; background-color: #3777bc; font-size: 14px; font-weight: 500; color: #fff;}.mobile-warranty-info-cols-header{display: none;}.warranty-info-cols-header-cell{flex: 0 1 32.5%; color: #fff; font-weight: 500; padding-left: 17px;}.warranty-info-row{display: flex; align-items: center; max-height: max-content; min-height: 48px; border-bottom: 1px solid #E0E0E0; line-height: 1.6;}.warranty-info-cell{flex: 0 1 32.5%; padding-left: 17px;}.notes-cell{flex: 0 1 59%; line-height: 1.7; padding: 0 20px 0 8px;}.notes-row{display: flex; height: max-content; margin: 15px 0;}.mobile-notes-row{display: flex; height: max-content; margin-bottom: 10px;}.warranty-info-row:nth-child(odd){background: #F5F5F5;}.warranty-check-disclaimer{display: flex; margin: 24px 0;}.disclaimer-icon{display: flex; align-items: flex-start; justify-content: center; padding-right: 10px;}.disclaimer-body{line-height: 1.7;}@media screen and (max-width: 920px){body{padding: 0; margin: 0;}#bottombracket{/*margin-top: 65px;*/ top: 0;}.global-modal-container{min-width: 320px; margin: auto;}#topbracket{display: flex; width: 100%; justify-content: left; /*margin-bottom: 50px;*/}.balancing-buttons{flex: 0; min-width: 33%; max-width: 33%;}.warranty-check-container{max-width: 672px; min-width: 320px; /*margin-top:80px;*/}.wc-section-header{width: 100%; min-width: 320px;}.inner-container{max-width: 93%;}.vehicle-details-grid{width: 100%;}.fox-box{display: none;}}@media screen and (max-width: 720px){.top-bar-buttons{display: none;}#bottombracket{margin-top: 0px; top: 0;}.wc-section-header{justify-content: center;}.cfx-logo-container{display: none;}.warranty-check-container{margin: 85px 16px 0 16px;}.warranty-check-mobile-header{display: flex; justify-content: center; align-items: center; top: 0; box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.42); height: 64px; width: 100%; z-index: 20; position: fixed; background: #3777BC;}.mobile-warranty-header-logo{width: 167px; height: 34px;}.inner-container{width: 95%; margin: 10px auto; display: flex; flex-direction: column; padding: 0 16px;}#topbracket{justify-content: center;}.balancing-buttons{display: none;}#warranty-check-form input[type=text]{width: 96%}.vehicle-details-grid{padding: 16px;}.grey-details-container{flex-direction: column; margin: 0 auto 24px; min-width: 100%; height: auto;}.vehicle-detail-row{display: flex; flex-direction: column; align-items: flex-start; justify-content: center; height: max-content; padding: 10px 0; width: 91%;}.warranty-recalculator-row{flex-direction: column; align-items: flex-start; height: auto; min-width: 100%; margin-bottom: 24px;}.recalculate-warranty-input-group{display: flex; flex-direction: column; width: 100%; margin-bottom: 15px;}.warranty-recalculator-submit{width: 100%}#warranty-check-form input[type=submit]{min-width: 100%; top: 0;}.warranty-info-container{display: none;}.mobile-warranty-info-container{display: flex; flex-direction: column; border: solid 1px #bdbdbd; border-radius: 4px;}.mobile-warranty-info-cols-header{display: flex; align-items: center; justify-content: center; height: 48px; background-color: #3777bc; border-radius: 4px 4px 0 0; color: #fff; font-weight: 500;}.warranty-info-row{flex-direction: column; padding: 0 10px; height: max-content;}.mobile-warranty-info-row{display: flex; flex-direction: column; padding: 0 10px; height: max-content; line-height: 1.8;}.mobile-warranty-info-row:nth-child(odd){background: #F5F5F5;}.mobile-warranty-info-cell{display: flex; justify-content: space-between; width: 100%; line-height: 2.0;}.warranty-info-type{text-align: left; font-weight: 700; width: 100%; line-height: 3;}.warranty-info-category{min-width: 20%;}}#headerHat{margin-bottom: 5px; display: flex; justify-content: space-between;}.headerHatSection{display: inline-block; ; /*width: 49%;*/}.headerHatSectionLeft{text-align: right;}#nonCipPrintBtn{height: 38px; width: 94px; border: 1px solid #3777BC; border-radius: 4px; /* box-shadow: inset 0 -3px 5px 1px #F5F5F5;*/ font-size: 14px; color: #3777bc; display: flex; align-items: center; justify-content: center; margin-bottom: 20px;}div#nonCipPrintBtn:hover{background-color: #eeeeee; cursor: pointer;}div#nonCipPrintBtn:active{background-color: #3777BC; color: #ffffff; cursor: pointer;}.headerHatSectionWidgets{display: flex; flex-direction: column; align-items: flex-end;}#onlineListingsToggleLabel{margin-right: 12px;}#printBar{text-align: right; padding: 5px; display: inline-block; height: 34px;}#printImage{display: inline-block; height: 24px; margin-top: 5px; margin-bottom: 5px;}#printText{display: inline-block; height: 24px; margin-top: 5px; margin-bottom: 5px; padding-left: 8px; vertical-align: middle}#printTextLink{text-decoration: none; color: #212121;}#languageToggle{display: flex; padding-bottom: 12px;}.languageToggleButton{display: flex; align-items: center; justify-content: center; border: 1px solid; text-align: center; cursor: pointer; font-size: 12px; height: 33px; width: 77px;}.languageToggleLeftButton{border-radius: 4px 0 0 4px; width: 77px;}.languageToggleRightButton{border-radius: 0 4px 4px 0; margin-left: -4px; width: 77px;}.selectedLanguageButton{background-color: #3777BC; border-color: #3777BC; color: #FFFFFF;}.unselectedLanguageButton{color: #3777BC; border-color: #BDBDBD; background-color: #FFFFFF;}.hotListingsSwitch{position: relative; display: inline-block; width: 40px; height: 15px;}.hotListingsSwitch input{display: none;}.hotListingsSlider{position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background-color: #ccc; -webkit-transition: .4s; transition: .4s;}.hotListingsSlider:before{position: absolute; content: \"\"; height: 22px; width: 22px; left: -4px; bottom: -2px; background-color: white; -webkit-transition: .4s; transition: .4s; box-shadow: 1px 1px rgba(128, 128, 128, 0.5);}input:checked+.hotListingsSlider{background-color: #4776a9;}input:focus+.hotListingsSlider{box-shadow: 0 0 1px #4776a9;}input:checked+.hotListingsSlider:before{-webkit-transform: translateX(22px); -ms-transform: translateX(22px); transform: translateX(22px);}/* Rounded sliders */.hotListingsSlider.round{border-radius: 34px;}.hotListingsSlider.round:before{border-radius: 50%;}@media print{#languageToggle{display: none;}}@media screen,print{.XDactive>.XDescCol{background-image: url(\"https://api.lot.report/media/carfax/XDescbg.gif\"); background-color: #FFFBE7; background-repeat: repeat-y;}/*main structural*/ #topbar{height: 65px; width: 100%; z-index: 20; position: fixed; background: #3777BC; margin-top: -65px; min-width: 720px; box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.24);}#topbracket{width: 100%; display: flex; justify-content: space-between;}.balancing-buttons{flex: 1;}.top-bar-buttons{flex: 1; display: flex; justify-content: flex-end; padding-top: 7px;}#bottombracket{width: 100%; max-width: 1280px; clear: right; overflow-y: scroll; -ms-overflow-style: none; scrollbar-width: none;}#bottombracket::-webkit-scrollbar{display: none; width: 0 !important;}#cipcontent{padding: 10px 0 0 0; margin-top: 20px; background-color: #fff; text-align: left;}/* header elements */ .hdrlogo{/*margin-top: 10px;*/ margin: 10px auto;}.top-bar-button{height: 40px; border: 1px solid #FFFFFF; border-radius: 4px; cursor: pointer; overflow: hidden; font-weight: 500; display: flex; align-items: center; justify-content: center; padding-left: 15px; padding-right: 15px; margin-right: 10px;}.top-bar-button, .top-bar-button:visited{color: #FFFFFF; text-decoration: none;}.top-bar-button:hover{background-position: 0 -37px !important; background-color: #EEEEEE; color: #3777BC;}.top-bar-button:hover>.top-bar-button-icon{fill: #3777BC;}.top-bar-button:active{background-color: #FFFFFF;}.top-bar-button-icon{vertical-align: middle; padding-right: 9px; fill: #FFFFFF;}#shareBtn{min-width: 39px;}.printhover{background-position: 0 -37px !important;}/* leftside Elements */ #navMain{padding: 24px 10px 20px 10px; color: #333333; border-bottom: 1px solid #BDBDBD;}.ciptabs>a, .cipupgrade li{display: block; height: 40px; cursor: pointer; overflow: hidden !important; text-decoration: none;}.cip-tab{border-bottom: 1px solid #BDBDBD; height: 29px; padding: 10px 0 0 16px; font-size: 16px; font-weight: 500; color: #3777BC; background-color: #FFFFFF;}.cip-tab sup{font-size: 7px; font-weight: normal;}#tab_wc{display: block}#tab_wc.fr_ca{display: block}.tabactive div{background-color: #7dc243; color: #FFFFFF;}.tabhover div{background-color: #EEEEEE;}#footer{clear: both; width: 100%;}.advDealerLogo{position: absolute; z-index: 5; margin: 0px 0 0 53px; float: left; width: 120px;}.cadbadge{width: 217px; padding: 30px 0 0 0; margin: 15px 0 0 6px;}.cadinfo{border: 1px solid #999999; padding-top: 5px; background-color: #ffffff; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; box-shadow: 0 3px 4px 1px #999999;}.cadaddress .dealer-phone{font-weight: 700;}.caddealerwebsite a{font-size: 12px; font-weight: 500; line-height: 2;}.cadcpo{font-size: 12px; color: #424242; font-weight: 500; font-style: italic; padding-top: 8px; text-align: center; line-height: 18px;}#CADtag{padding: 10px; color: #666666; display: none;}.cad{padding-top: 45px;}.cad .cadinfo{border-style: none; padding-top: 45px;}.cad #CADtag{display: block;}.cadlead{margin: 0 0 4px 0; font-size: 11px; width: 100%; text-align: center;}.cadinfo address{font-style: normal; margin: 5px; font-size: 12px; text-align: center;}.cadinfo #dealer-name{line-height: 125%; font-size: 16px; font-weight: 700; width: 100%; text-align: center;}/*cipcontent elements*/ #loadingBox{display: none; z-index: 250; position: absolute; text-align: center; background: #fff; color: #333; width: 150px; height: 150px; left: 37%; padding-top: 20px; top: 300px; border: 1px solid #666; overflow: hidden; -webkit-border-radius: 10px; -moz-border-radius: 10px; border-radius: 10px; -moz-box-shadow: 5px 5px 4px #333; -webkit-box-shadow: 5px 5px 4px #333; box-shadow: 5px 5px 4px #333;}.printactive{background-color: #fff; color: #333; border: 2px solid #ff9900 !important;}#printModal h4{color: #333;}.printOption{margin: 15px; padding: 5px 5px 8px 5px; -webkit-border-radius: 10px; -moz-border-radius: 10px; border-radius: 10px; border: 1px solid #ccc;}ul.printlist{margin-left: 20px; padding-top: 7px;}ul.printlist li{margin-bottom: 6px;}ul.printlist li label{line-height: 145%;}#printError{display: none; text-align: center; margin: 15px; color: red; background-color: rgb(255, 238, 238); padding: 12px; border: 2px solid darkred; display: none; -webkit-border-radius: 10px; -moz-border-radius: 10px; border-radius: 10px;}#btnPrint{margin-left: 20px;}#emailModal{display: none;}.cadcontent{padding: 3px 10px 4px 10px; display: flex; flex-direction: column; align-items: center;}#tabvhr, #tabws, #tabbbg, #tabwc, #upgrade{margin: 0 0 20px 0 !important; text-align: left;}.CADPrintBanner{display: none; margin: 0 auto; position: relative;}.CADPrintBanner label{color: #999999;}.CADbanner{width: 100%; overflow: visible;}.CADbanner p{color: #666666; position: absolute; top: 28px; left: 140px; line-height: 120%;}.CADbanner p:first-line{color: #333333}/*upgrade elements */ #upgradeWrapper{/*background: #fff url(/img/vhr/cip/upgradebg.png) no-repeat top*/ /*left;*/ width: 663px; height: 271px;}#upgradeContent{padding: 10px 163px 36px 10px; color: #336699;}#upgradeContent h3{line-height: 120%;}#upgradeContent p{line-height: 120%; margin-top: 10px;}#upgradeContent ul{line-height: 140%; color: #333333; list-style: disc; margin: 0 15px; padding: 10px;}#upgradeContent li{line-height: 120%}#langToggle img{cursor: pointer;}#ugpradeMessage{background: #FFFBE7; /* Old browsers */ background: -moz-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* FF3.6+ */ background: -webkit-gradient(linear, left top, left bottom, color-stop(2%, rgba(235, 231, 203, 1)), color-stop(30%, rgba(255, 251, 231, 1))); /* Chrome,Safari4+ */ background: -webkit-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* Chrome10+,Safari5.1+ */ background: -o-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* Opera11.10+ */ background: -ms-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* IE10+ */ /*filter: progid : DXImageTransform.Microsoft.gradient (startColorstr='#ebe7cb', endColorstr='#fffbe7', GradientType=0);*/ /* IE6-9 */ background: linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* W3C */ border: 1px solid #ccc; -webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; color: #4a7800; line-height: 20px; padding: 0 10px; display: inline-block; margin: 0; height: 40px; overflow: hidden;}#ugpradeMessage tr{height: 40px;}#ugpradeMessage td{text-align: center; width: 450px;}#upgradeMessageWrapper{text-align: center; position: fixed; left: 20px; top: 3px; z-index: 7; width: 100%;}}@media screen{#tabvhr, #tabws, #tabbbg, #tabwc{display: none;}#nav{width: 228px; height: 100%; background-color: #f5f5f5; border-right: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD;}/* fixed tabs */ .fixedTabs #nav{position: fixed; z-index: 10;}.fixedTabs #cipcontent{/*padding-left: 253px;*/}/* scrolling tabs */ #bottombracket.scrollingTabs{display: flex; flex-wrap: nowrap; justify-content: space-around;}.scrollingTabs #nav{top: 0; flex: 0 0 228px; height: auto;}.scrollingTabs #cipcontent{margin-left: 20px; align-self: flex-end;}#shareBtn:hover{background-position: 0 -37px !important;}#shareSpanner{display: block; z-index: 200; position: fixed; height: 20px; width: 128px;}#shareMenu:before{content: \"\"; position: absolute; top: -9px; /* value=- border-top-width - border-bottom-width */ right: 14px; /* controls horizontal position */ border-width: 0 8px 8px; /* vary these values to change the angle of the vertex */ border-style: solid; border-color: #CCCCCC transparent; /* reduce the damage in FF3.0 */ display: block; width: 0;}#shareMenu{list-style: none; display: none; border: 1px solid #CCCCCC; border-radius: 4px; -webkit-border-radius: 4px; -moz-border-radius: 4px; box-shadow: 0px 0px 3px 3px rgba(0, 0, 0, .1); -webkit-box-shadow: 0px 0px 3px 3px rgba(0, 0, 0, .1); width: 126px; color: #424242; position: fixed; z-index: 50; background-color: #FFFFFF;}#shareMenu:after{content: \"\"; position: absolute; top: -8px; /* value=- border-top-width - border-bottom-width */ right: 14px; /* controls horizontal position */ border-width: 0 8px 8px; /* vary these values to change the angle of the vertex */ border-style: solid; border-color: #FFFFFF transparent; /* reduce the damage in FF3.0 */ display: block; width: 0;}#shareMenu li{padding: 11px 16px; cursor: pointer;}#shareMenu li:hover{background-color: #eef1f8;}#shareMenu li img{vertical-align: -2px; padding-right: 5px;}#shEmail i{font-size: 16px; vertical-align: -3px; padding-right: 5px;}#emailConsole, #emailSent, #emailNotSent{position: fixed; z-index: 200; left: calc(50% - 320px); top: calc(50% - 280px); color: #424242; width: 640px; height: 561px; border: 1px solid #999; background-color: #FFFFFF; cursor: default; box-shadow: 0 15px 12px 0 rgba(0, 0, 0, 0.22), 0 19px 38px 0 rgba(0, 0, 0, 0.3);}.consoleTitle{float: none; padding: 24px 24px 16px 24px; border-bottom: 1px solid #BDBDBD;}.consoleTitle h6{float: left; font-weight: 500; font-size: 18px;}.consoleContent{padding: 12px 24px 24px 24px;}.consoleContent textarea, .consoleContent input[type=\"text\"]{width: 295px; padding: 12px 16px; margin-bottom: 20px; font-family: roboto; font-size: 14px;}.consoleContent textarea{width: 556px; margin-bottom: 0;}.consoleContent label{font-weight: 500; padding-bottom: 8px;}.consoleContent .shareEmailBtnHolder{display: flex; width: 55%; height: 64px; color: #fff; align-items: flex-end;}.consoleContent .btn{height: 38px; width: 86px; border-radius: 4px; margin-right: 20px; border: 0; background: none; box-shadow: none; color: #fff; text-align: center;}#shareEmailSendButton{background-color: #7DC243;}#shareEmailSendButton:hover{background-color: #9cc465; cursor: pointer;}#shareEmailCancelButton{background-color: #424242;}#shareEmailCancelButton:hover{background-color: #555555; cursor: pointer;}.consoleCancel{float: right;}.consoleCancel:hover{cursor: pointer;}#emailConsole label{display: block;}.clear{clear: both;}.charCount{float: right; font-style: italic; padding: 3px;}#emailConsole fieldset{border: 0; margin: 0; padding: 0;}.sentContent{padding: 10px 24px 24px 24px; clear: both; text-align: left; font-weight: 500;}#emailSentSuccessIcon{float: left; height: 20px; color: #7DC243; font-size: 22px; padding-right: 8px;}#emailSent h6{line-height: 135%;}#emailSent .btn{height: 38px; width: 48px; border-radius: 4px; margin: 382px 0 0 24px; float: left; border: 0; background: none; box-shadow: none; color: #fff; text-align: center; background-color: #7DC243;}#emailSent .btn:hover{background-color: #9cc465; cursor: pointer;}#emailNotSentWarningIcon{float: left; height: 20px; color: #F44336; font-size: 22px; padding-right: 8px;}#emailNotSent h6{padding-top: 3px;}#emailNotSent .btn{height: 38px; width: 48px; border-radius: 4px; margin: 382px 0 0 24px; float: left; border: 0; background: none; box-shadow: none; color: #fff; text-align: center; background-color: #424242;}}@media only screen and (min-width: 1280px){#bottombracket.scrollingTabs{margin-left: calc(50% - 640px);}#bottombracket.fixedTabs{margin-left: calc(50% - 635px);}}@media only screen and (max-width: 1280px){.nav-menu-mask{z-index: 6; color: #000; opacity: 0.7; display: none;}#nav{height: 100%; width: 372px; z-index: 2; background-color: #2c5f96; border: none;}#navMain{display: none;}.cip-tab{border-bottom: 1px solid rgba(0, 0, 0, .1); height: auto; padding: 20px 0 20px 24px; color: #FFFFFF; background-color: #2c5f96;}.ciptabs>a{height: auto;}.ciptabs>a:hover div{background-color: #3777bc;}.open-cip-menu, .close-cip-menu{cursor: pointer; padding: 17px 17px 18px 17px;}.tabactive div{background-color: rgba(0, 0, 0, 0.1);}.close-cip-menu svg, .open-cip-menu svg{height: 30px; width: 30px;}.close-cip-menu{background-color: #2c5f96;}/* CIP menu closed */ #nav, .scrollingTabs #nav{display: block; position: absolute; top: -50000px;}.open-cip-menu{display: inline-block;}.close-cip-menu{display: none;}#nav>div:last-child{/* keeping this visible but off-screen ensures the RR stars will have the necessary gradient available */ display: block; position: absolute; top: -3000px;}/* CIP menu is open */ .cip-menu-open .nav-menu-mask{z-index: 6; display: block; background-color: rgba(0, 0, 0, 0.5); height: 100%; width: 100%; position: fixed; top: 65px;}.cip-menu-open .open-cip-menu{display: none;}.cip-menu-open .close-cip-menu{display: inline-block;}.cip-menu-open #nav, .scrollingTabs .cip-menu-open #nav{position: fixed; top: auto; left: 0; z-index: 10; height: 100%;}.cip-menu-open #nav, .cip-menu-open #modalCurtain{display: block;}.cip-menu-open #cipPrintBtn, .cip-menu-open #shareBtn{visibility: hidden;}/* End CIP menu styles */ .fixedTabs #cipcontent{padding-left: 0px;}.scrollingTabs #cipcontent{margin-left: 0;}}@media print{#carfaxcip{overflow: visible;}#cipcontent{padding: 0; margin: 0; overflow: visible; overflow-y: visible; display: block;}#bottombracket{position: relative; overflow-y: visible; overflow: visible; top: 0px; bottom: auto; height: auto; background: none; display: block; padding: 0;}#langToggle{display: none;}#carfax{overflow: visible;}#nav, #topbracket, #topbar, #loadingBox, #printModal, .cad, #shareMenu{display: none;}#tabvhr, #tabws, #tabbbg, #tabwc, #upgrade{margin: 0; overflow: visible;}.CADPrintBanner{display: block;}.cipNoPrint{display: none !important;}}#bottombracket{margin-top: 65px;}@media print{#bottombracket{margin-top: 0;}}@media screen,print{.floatLeft{float: left; margin-right: 10px; margin-left: 4px}.floatRight{float: right; margin-right: 4px;}#tabws, #tabbbg, #tabwc{margin: 0 auto;}#toggleButtonActivator{float: left; margin: 0px; font-weight: 500; text-align: left; padding: 0 0 5px 0;}#toggleButtonActivator li{float: left; list-style-image: none; list-style-position: outside; list-style-type: none; padding: 3px;}.toggleActivated, .toggleActivated a, .toggleActivated a:visited{background-color: #FF9900; color: #FFFFFF; text-decoration: none;}.toggleDeactivated, .toggleDeactivated a, .toggleDeactivated a:visited{color: #666666; text-decoration: none;}.toggleDeactivated a:hover{text-decoration: underline;}#toggleIndicator{visibility: hidden;}#icr-updated-message{color: #FF9900;}#contentWrapper{width: 980px; margin: 0px auto; margin-bottom: 50px; padding-top: 15px; padding-bottom: 15px; border-color: #EFEFEF rgb(153, 153, 153) rgb(153, 153, 153); border-style: solid; border-width: 1px;}#contentWrapper .usageLabel{margin: 0px auto;}#upgradeBar{margin: 0px auto; padding: 0; background: url(\"/img/vhr/upgradeRightBg.gif\") top right no-repeat; height: 40px; width: 720px; vertical-align: middle;}#upgradeBar img{margin: 4px 20px 5px 5px; padding-bottom: 0px; float: left;}#upgradeBar .rightHand{float: right; text-align: right; padding: 8px 10px 0 0;}.rightHand label{margin-bottom: 2px;}#upgrade2CIP, #upgrade2CIPDisabled{margin: 0; padding-left: 108px; width: 610px;}#upgrade2CIPDisabled{display: none;}#header{width: 980px; background: #FFFFFF; line-height: normal; overflow: hidden; margin: 0 auto;} #header ul{padding: 0; list-style: none; float: left;}#header li{float: left; background: url(/img/vhr/left.gif) no-repeat left top; margin: 0; padding: 0 0 0 3px;}#header li a{float: left; display: block; background: url(/img/vhr/right.gif) no-repeat right top; padding: 5px 15px 4px 6px; color: #003366; text-decoration: none;} #header li a{float: none;}/* End IE5-Mac hack */ #header li a:hover{color: #326198; background-image: url(/img/vhr/right_on.gif); text-decoration: underline;}#header #active{background-image: url(/img/vhr/left_on.gif);}#header #active a{background-image: url(/img/vhr/right_on.gif); color: #990000; padding-bottom: 5px; text-decoration: none;}#header #active a:hover{background-image: url(/img/vhr/right_on.gif); color: #990000; padding-bottom: 5px; text-decoration: underline;}#header .nodata{float: left; display: block; background: url(/img/vhr/right.gif) no-repeat right top; padding: 5px 15px 4px 6px; color: #666666; text-decoration: none;}#printVisible{float: right; margin: 5px auto; text-align: right; padding: 5px;}#printVisible a.orange{color: #FF6600;}.printBtn{background-color: #e8e8e8; padding: 5px; border-radius: 3px; -moz-border-radius: 3px; -webkit-border-radius: 3px; border: 2px solid #d0d0d0; overflow: visible;}.printBtn a, .printBtn a:visited{text-decoration: none;}.printBtn a:hover{text-decoration: underline;}.active{display: block; visibility: visible;}.inactive{display: none;}.animatedLoadingImage{display: none; z-index: 250; position: absolute; text-align: center; background: #FFFFFF; width: 300px; height: 150px; left: 37%; padding-top: 60px; top: 300px; border: 1px solid #C0C0C0;}/*******************UPGRADE MESSAGE *************/ #naMsg{min-height: 1000px; height: auto !important;}.upgradeMsg{min-height: 210px; width: 75%; max-width: 603px; border: 1px solid #3777BC; background-color: rgba(55, 119, 188, 0.05); box-shadow: 0 1px 4px 0 rgba(0, 0, 0, 0.15); color: #3777BC; font-weight: 500; padding: 24px; position: relative; margin-left: 20px; margin-top: 30px;}.upgradeMsg p{font-size: 16px; line-height: 120%; margin-top: 10px;}.upgradeMsg ul{color: #212121; list-style: disc; margin: 0 15px; padding: 18px 14px;}.upgradeMsg li{line-height: 180%}.upgradeToCipBtn{height: 40px; width: 270px; border-radius: 4px; background-color: #7dc243; position: relative;}.upgradeToCipBtn a{color: #FFFFFF; text-decoration: none; position: relative; top: 30%; left: 8%;}.viewSampleBtn{float: right; height: 40px; width: 130px; border: 1px solid #3777BC; border-radius: 4px; background-color: #FFFFFF; position: relative;}.viewSampleLink{margin-top: 8px; position: absolute; top: 10%; left: 17%; text-decoration: none;}#naMsg .leaning-fox{height: 246px; position: absolute; left: 98.7%; top: 24px;}/************************HIGHLIGHTS************************/ #highlightsWrapper{display: flex; flex-direction: column;}#highlights-main-content{display: flex; margin-left: 70px;}#leftSide{width: 290px; float: left; margin-right: 35px;}#rightSide{width: 250px; float: right; text-align: left; font-weight: 500;}#ownershipHistory, #accidentIssues, #titleProbs{margin-bottom: 10px;}.vh_note{padding: 2px;}.vh_smallTopPadding{margin-top: 6px;}.vh_clean{width: 90px; margin: 0 auto; display: flex; align-items: center;}.vh_clean img{float: left; padding-top: 3px}.vh_clean div{text-align: left; padding-left: 8px;}.vh_chkDesc{text-align: left; line-height: 1.2em;}.vh_bold_green{color: #009900;}.vh_statCol{border-top: 1px none #aaaaaa; border-right: 1px solid #aaaaaa; border-bottom: 1px solid #cccccc; border-left: 1px none #aaaaaa; text-align: center; padding: 4px 0 4px 0;}.vh_gridCap{border-bottom: 1px solid #cccccc; text-align: left;}.vh_gridCapNoBottomBorder{text-align: left;}.vh_oneOwnerCell{border-bottom: 1px solid #cccccc; padding: 3px;}.vh_vh_pad3{padding: 3px;}#vehicleInfo{padding: 2px; text-align: left; line-height: 120%; font-weight: bold;}.vh_ymm{font-size: 18px;}#CPObadge{padding-top: 1px;}#warrantyInfo{width: 250px; padding: 2px; border: 1px solid #09357a; text-align: left;}.vh_warrantyStatus{padding-top: 2px; ; padding-bottom: 2px;}#dlrInfo{text-align: center; line-height: 120%;}div.vh_dlrName div#dealer-name a#dealer-name-link{color: #000; text-decoration: none; line-height: 150%;}div.vh_dlrName div#dealer-name{color: #000; text-decoration: none; line-height: 150%;}#dlrRating{width: 248px; padding: 1px; border: 1px solid #09357a; background-color: #eeeeee; margin-top: 1px; vertical-align: bottom;}#vh_disclaimer{color: #666;}#vh_disclaimer p{margin: 5px 7px 5px 7px; line-height: 12px; text-align: left;}#vh_disclaimer a:link, #vh_disclaimer a:hover, #vh_disclaimer a:visited, #vh_disclaimer a:hover, #vh_disclaimer :visited{color: #666;}.vh_topMarginFive{margin-top: 10px;}.vh_learnMore{font: normal 9px Tahoma, Verdana, Helvetica, sans-serif;}.vh_blueBorder{border-left: 13px solid #336699; border-right: 13px solid #336699;}.vh_vehicleInfoHeight{height: 205px;}.vh_warrantyInfoHeight{height: 123px;}.vh_dealerInfoHeight{height: 200px;}#nonSrrFooter, #printFriendlyNonSrrFooter{clear: both; margin-top: 3px; margin-left: 70px;}#nonSrrFooter p, #printFriendlyNonSrrFooter p{font: normal normal normal 11px/12px Arial, Helvetica, sans-serif; color: #CCCCCC; margin: 0; padding: 0; text-align: left;}/************************OTHER REPORTS************************/ .invisibleText{position: relative; display: none; z-index: 2;}#organicUpSellBanner{width: 660px; border: 0px; background-color: #e5ecf9; text-align: left; padding: 5px 5px 10px 5px; margin-bottom: 15px;}#organicUpSellBanner p, #organicUpSellBanner strong{text-align: left; padding: 0; margin: 0; padding-left: 10px;}#helpCenterLinksParagraph{text-align: center; display: block; border-right: 1px solid #bdbdbd; border-left: 1px solid #bdbdbd; border-bottom: 1px solid #bdbdbd; border-radius: 0 0 4px 4px; padding: 16px 16px 18px 16px;}.bornOnTxt{margin-bottom: 0px; margin-top: 10px; font-size: 12px;}.wcPrintBar{text-align: right; margin: 10px 0 10px 0;}.warrantyBottomHalfWhite{border: 3px solid #98a3b1; border-top: 0px none #ffffff; background-color: #ffffff; padding: 10px; margin: 0 0 20px 0; voice-family: inherit; width: 644px;}#VinPhrase{margin: 15px 0;}#tabwc td{padding: 3px;}#tabbbg .outline{border: #333333 thick groove; background-position: center center; background-repeat: no-repeat;}#tabbbg .normal{color: #333333;}#tabbbg .txtSmall{color: #333333;}#tabbbg .ttl01{color: #165899;}#tabbbg .infoBld{color: #165899;}#tabbbg .outlineThin{border: #cccccc 1px solid; padding: 3px;}#tabbbg .borderTop{background: #fff url(/img/bbg/bkgBbgTop.gif) repeat-x bottom 50%;}#tabbbg .borderBtm{background: url(/img/bbg/bkgBbgBtm.gif) repeat-x 50% top;}#tabbbg .borderRgt{background: url(/img/bbg/bkgBbgRgt.gif) repeat-y left 50%;}#tabbbg .borderLft{background: url(/img/bbg/bkgBbgLft.gif) repeat-y right 50%;}#facebox{position: absolute; width: 100%; top: 0; left: 0; z-index: 200; text-align: left;}#facebox .popup{position: relative;}#facebox table{margin: auto; border-collapse: collapse;}#facebox .body{padding: 10px; background: #fff; width: 700px; border: 1px solid black; align: center;}#facebox .loading, #facebox .image{text-align: center;}#facebox img{border: 0;}#facebox .footer{border-top: 1px solid #DDDDDD; padding-top: 5px; margin-top: 10px; text-align: right;}#facebox .tl, #facebox .tr, #facebox .bl, #facebox .br{height: 10px; width: 10px; overflow: hidden; padding: 0;}.summaryDesc{background-color: #336699; color: #FFFFFF;}}@media print{.previewSection{display: none;}.varTagNoDisplay{display: none;}#upgradeBar{display: none;}#contentWrapper{display: block; border-width: 0px; width: 680px;}#printVisible{margin: 0px auto; display: none;}#header, .printPageLink, .printAllLink{display: none;}#contentWrapper{border: 0px none #ffffff;}#forPersonalUsePrint{display: block; visibility: visible; margin-left: 30px; text-align: left;}#highlights-main-content{margin-left: auto; margin-right: auto;}#nonSrrFooter, #printFriendlyNonSrrFooter{margin-left: auto; margin-right: auto;}.inactive{display: block; visibility: visible;}/* Had to do printing with multiple classes to handle funkiness with internet explorer. */ /* This will be tabws */ .printPage{page-break-after: always;}.printThisPage_Print{display: block; visibility: visible;}.printThisPage_NoPrint{display: none;}.animatedLoadingImage{display: none;}/* HIGHLIGHTS */ #CPObadge{padding-top: 5pt;}#printFriendlyNonSrrFooter, #printFriendlyNonSrrFooter p{display: none;}#nonSrrFooter p{font: normal normal normal 7pt/7pt Arial, Helvetica, sans-serif; color: #CCCCCC; margin: 0; padding: 0; ; margin: 0;}.vh_dlrHR{padding-bottom: 2pt;}.vh_smallTopPadding{margin-top: 3pt;}.vh_warrantyStatus{padding-top: 2pt; padding-bottom: 2pt;}.vh_vehicleInfoHeight{height: 170pt;}.vh_warrantyInfoHeight{height: 90pt;}.vh_dealerInfoHeight{height: 160pt;}#vehicleInfo{padding: 2pt; text-align: left;}#dlrInfo{text-align: center; vertical-align: middle;}#dlrRating{width: 185pt; padding: 1pt; border: 1pt solid #09357a; background-color: #eeeeee; margin-top: 1pt; vertical-align: middle;}#vh_disclaimer{color: #666;}#vh_disclaimer p{margin: 5pt 2pt 5pt 2pt; width: 185pt; line-height: 7pt; text-align: center; vertical-align: middle;}#vh_disclaimer a:link, #vh_disclaimer a:hover, #vh_disclaimer a:visited, #vh_disclaimer a:hover, #vh_disclaimer :visited{color: #666;}}.vh_dlrSub{word-wrap: break-word; max-width: 264px;}.CADPrintBanner{margin: 15px auto; position: relative;}.CADPrintBanner label{color: #999999; display: block;}.CADbanner{width: 100%; overflow: visible;}.CADDealerInfo div{color: #666666; line-height: 120%;}.CADDealerInfo{position: absolute; top: 28px; left: 140px; display: inline;}.CADDealerInfo a{color: #336699; text-decoration: none; border-bottom: 1px dotted #336699; display: inline; margin-top: 5px;}.CADDealerInfo #dealer-name-link{color: #333333; border-bottom: none;}.tooltip{display: none; position: absolute; top: 0; left: 0; z-index: 4;}@media print{.CADPrintBanner{display: block;}div.CADPrintBanner.noPrint{display: none;}#showMeHeader{display: none;}.CADPrintBanner{width: 670px;}#cadDefLink{display: none;}}@media screen{.CADPrintBanner{width: 980px;}}.nmvtis-upgrade{}.lien-report{background: rgb(235, 231, 203); /* Old browsers */ background: -moz-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* FF3.6+ */ background: -webkit-gradient(linear, left top, left bottom, color-stop(2%, rgba(235, 231, 203, 1)), color-stop(30%, rgba(255, 251, 231, 1))); /* Chrome,Safari4+ */ background: -webkit-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* Chrome10+,Safari5.1+ */ background: -o-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* Opera11.10+ */ background: -ms-linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* IE10+ */ filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#ebe7cb', endColorstr='#fffbe7', GradientType=0); /* IE6-9 */ background: linear-gradient(top, rgba(235, 231, 203, 1) 2%, rgba(255, 251, 231, 1) 30%); /* W3C */ border: 1px solid #ccc; -webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; color: #4a7800; height: 35px; line-height: 35px; padding: 0 10px; display: inline-block; overflow: hidden; min-width: 195px; vertical-align: middle; text-align: center; float: left; margin: 7px 0 0 78px;}.upsellMessage{text-align: left !important; line-height: 35px;}.upsellMessage img{margin-top: 2px; margin-right: 10px;}.upsellText{padding-right: 10px;}.lien-report .btnGreenCheck{-moz-box-shadow: inset 0px 1px 0px 0px #a4e271; -webkit-box-shadow: inset 0px 1px 0px 0px #a4e271; box-shadow: inset 0px 1px 0px 0px #a4e271; background: -webkit-gradient(linear, left top, left bottom, color-stop(0.05, #89c403), color-stop(1, #77a809)); background: -moz-linear-gradient(center top, #89c403 5%, #77a809 100%); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#89c403', endColorstr='#77a809'); background-color: #89c403; -moz-border-radius: 6px; -webkit-border-radius: 6px; border-radius: 6px; border: 1px solid #74b807; color: #ffffff; padding: 4px 10px 6px; text-decoration: none; text-shadow: 1px 1px 0px #528009; line-height: 15px;}.lien-report .btnGreenCheck:hover{background: -webkit-gradient(linear, left top, left bottom, color-stop(0.05, #77a809), color-stop(1, #89c403)); background: -moz-linear-gradient(center top, #77a809 5%, #89c403 100%); filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#77a809', endColorstr='#89c403'); background-color: #77a809;}.lien-report .btnGreenCheck:active{position: relative; top: 1px;}.vhronlyUpsellMessage{border: 1px solid #ccc; color: #4a7800; height: 35px; line-height: 35px; padding: 0 10px; display: block; overflow: hidden; min-width: 195px; width: 195px; text-align: center; float: none; margin: 7px auto; -webkit-border-radius: 3px; -moz-border-radius: 3px; border-radius: 3px; background: rgb(235, 231, 203);}#viewLienReportBtn{margin-right: 5px;}#vhrLienButtons{display: inline-block;}#value-container{display: flex; flex-direction: column; margin: auto auto 10px auto;}#value-container .icr-op-header{display: flex; align-items: center; width: 100%; height: 56px; color: #ffffff; background-color: #3777BC; padding: 4px 0 2px 17px; -webkit-box-sizing: border-box; -moz-box-sizing: border-box; box-sizing: border-box; border-radius: 4px 4px 0 0;}#value-container .icr-opr-header{display: flex;}#value-container .cfx-logo{margin: 0 10px 0 10px;}#value-container .chbv-header-text{color: #FFFFFF; font-size: 18px; font-weight: 500; width: 70%;}#value-container .chbv-container{display: flex; height: 100%;}#value-container .badge-container{display: flex; flex-direction: column; width: 40%; box-shadow: inset -1px 0 0 0 #BCC3CA; background-color: #EBF4FD; justify-content: center; align-items: center;}#value-container .badges img{display: flex; max-width: 155px; width: 155px; height: 150px;}#value-container .pricing-container{display: flex; flex-direction: column; justify-content: normal; margin: 0 !important; width: 60%;}#value-container .history-events-text{color: #3777BC; line-height: 1.2; text-align: center; margin: 10px 0 3px 0; font-size: 24px; font-weight: 500;}#value-container .arrows-container{display: flex; margin: 15px auto 0 auto;}.real-arrows-container{max-width: 462px; align-self: center;}#value-container .arrow-cell{width: 70px; display: flex; flex-direction: column; margin: 0 auto 0 auto;}#value-container .arrow{margin: 0 auto 5px auto;}#value-container .arrow-cell .icon-cell{display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%; max-width: 100%; margin: auto;}#value-container .icon-cell>img{width: 26px; height: 26px;}#value-container .icon-text{height: 42px; line-height: 1.1; text-align: center; padding: 5px; width: 100%; font-size: 14px; font-weight: 500;}#value-container .icon-text-long{width: 110px;}#value-container .icon-text-short{width: 85%;}#value-container .update-text{font-size: 14px; font-weight: 500; color: #FFFFFF; padding: 0 13px;}.buttons-and-toggles{display: flex; justify-content: space-around; padding: 12px 0;}#value-container .bottom-line{display: flex; border-bottom: 1px solid #555; border-right: 1px solid #555; width: 75%; height: 20px; margin: 0 auto 0 auto;}#value-container .pricing-info{display: flex; justify-content: center; line-height: 1.4; color: #212121;}#value-container .left-column{margin-right: 15px; text-align: right;}#value-container .make-it-blue{color: #3375BE;}#value-container .make-it-green{color: #378F4A;}.last-seen-text{font-size: 11px !important; font-weight: 500; color: #424242; text-align: center;}.last-seen-container{display: flex; height: 30px; min-height: 30px; align-items: flex-end;}#value-container .last-seen-text{color: #9E9E9E; text-align: center; margin: 0 auto 4px auto;}#value-container #bottom-line-4{border-bottom: 1px solid #555; width: 76%; margin: 0 auto 0 auto;}#value-container #bottom-line-3{border-bottom: 1px solid #555; width: 51%; margin: 0 auto 0 auto;}#value-container #bottom-line-2{border-bottom: 1px solid #555; width: 25.5%; margin: 0 auto 0 auto;}#value-container .zero-arrow-last-seen-text{margin: 10px auto 4px auto;}#value-container #bottom-line-1{display: none;}#value-container .hide-me{display: none;}#value-container .zero-arrow-height{height: 210px;}#value-container .zero-arrow-content{justify-content: center; align-items: center;}#value-container .four-arrow-width{width: 100%;}#value-container .three-arrow-width{width: 76%;}#value-container .two-arrow-width{width: 50.5%;}#chbv-section{border-top: 0px none #BDBDBD; border-right: 1px solid #BDBDBD; border-bottom: 1px solid #BDBDBD; border-left: 1px solid #BDBDBD; display: flex; justify-content: center; min-height: 275px; border-radius: 0 0 4px 4px; margin-bottom: 18px;}.chbv-container{display: flex; justify-content: center;}#show-hide{color: #fff; font-size: 14px; text-decoration: underline; cursor: pointer; display: none; font-weight: 500; padding-left: 110px;}.center{display: flex; justify-content: center; align-items: center;}.update-btn{height: 32px; max-width: 180px; border-radius: 4px; background-color: #7DC243; cursor: pointer; align-self: center; min-width: 115px;}.update-btn:hover{background-color: #3bac3b;}.tos-toggle-balancer{width: 109px;}.tos-toggle{height: 50px; display: flex; flex-direction: column; justify-content: space-between; align-items: flex-end;}.radio-container{display: flex; width: 109px; align-items: flex-start;}.radio-outer{border: 2px solid #BDBDBD; width: 18px; height: 18px; border-radius: 50%; cursor: pointer;}.radio-inner{width: 12px; height: 12px; border-radius: 50%; background-color: #3777bc; visibility: hidden;}.radio-text{color: #666666; font-size: 12px; margin: auto 10px; font-weight: 500;}.flex-row{display: flex;}.flex-column{display: flex; flex-direction: column;}.no-badge-pricing{font-size: 24px; text-align: center; font-weight: 600;}.dealer-pricing-container{height: 50px; font-size: 24px; font-weight: 500;}#pricing-no-badge{width: 100%;}#pricing-with-badge{display: flex; justify-content: center; font-size: 24px; font-weight: 500;}.dealer-pricing-no-badge{height: 30px; margin-top: 20px}#value-container .no-badge-update-btn{cursor: pointer; align-self: center; height: 32px; width: 180px; border-radius: 4px; background-color: #7DC243;}#value-container .no-badge-update-btn:hover{background-color: #3bac3b;}@media screen and (max-width: 1280px){#value-container .history-events-text{font-size: 18px;}#chbv-section div:first-child .history-events-text{font-size: 24px;}#chbv-section div:first-child.pricing-container{width: auto;}#chbv-section{margin-bottom: 6px;}}@media screen and (max-width: 1004px){#show-hide{padding-left: 10%;}}@media screen and (max-width: 900px){#show-hide{padding-left: 7%;}}@media screen and (max-width: 800px){#show-hide{padding-left: 4%;}}@media print{#value-container .noPrint, #value-container.noPrint{display: none;}#value-container #oneprice{display: none;}#value-container .tos-toggle{display: none;}#value-container .icr-opr-header{background-color: #3777bc !important;}}.til-car-money-section{display: flex; position: relative; /* for til-car-logo-for-car-money */ border: 1px solid #bdbdbd; border-radius: 4px; height: 208px; flex-flow: row wrap; justify-content: start; margin-bottom: 24px;}.til-car-logo-for-car-money{height: 24px; margin-top: 24px; margin-left: 24px;}.til-car-money-flex-container{width: 100%; display: flex; flex-direction: row; /* explicit */ justify-content: space-between; align-items: flex-end; margin-bottom: 48px;}.til-car-money-sell-container{display: flex; flex-direction: column; align-items: center; height: 112px;}.til-car-money-sell-text-container{text-align: center;}.til-car-money-sell-first-line{font-family: Roboto, sans-serif; font-size: 24px; color: #424242; line-height: 32px;}.til-car-money-sell-second-line{font-family: Roboto, sans-serif; font-weight: bold; font-size: 24px; line-height: 32px;}.til-car-money-sell-container .tradeInLeadsBtn{margin-top: 8px;}.til-money-image{margin-bottom: 6px;}.til-car-money-flex-container .tradeInLeadsBtn{display: flex; height: 40px; width: 153px; flex-shrink: 0;}.til-car-money-flex-container a.tradeInLeadsBtnTxt{font-size: 14px; font-weight: 500; letter-spacing: 0; line-height: 24px;}@media screen and (max-width: 1280px){.til-car-money-section{margin-bottom: 16px;}}@media screen and (max-width: 720px){.til-car-money-section{justify-content: center; height: 307px; margin: 16px 0px;}.til-car-logo-for-car-money{margin-left: 0; margin-bottom: 24px;}.til-car-money-flex-container{flex-wrap: wrap;}.til-car-money-sell-container{order: -1; flex-basis: 100%; flex-grow: 4; margin-bottom: 24px;}.til-car-image{order: 0;}.til-money-image{order: 1; margin: 0;}}@media print{.til-car-money-section{display: none;}}/* cyrillic-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu72xKOzY.woff2) format('woff2'); unicode-range: U+0460-052F, U+1C80-1C88, U+20B4, U+2DE0-2DFF, U+A640-A69F, U+FE2E-FE2F;}/* cyrillic */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu5mxKOzY.woff2) format('woff2'); unicode-range: U+0301, U+0400-045F, U+0490-0491, U+04B0-04B1, U+2116;}/* greek-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu7mxKOzY.woff2) format('woff2'); unicode-range: U+1F00-1FFF;}/* greek */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu4WxKOzY.woff2) format('woff2'); unicode-range: U+0370-03FF;}/* vietnamese */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu7WxKOzY.woff2) format('woff2'); unicode-range: U+0102-0103, U+0110-0111, U+0128-0129, U+0168-0169, U+01A0-01A1, U+01AF-01B0, U+1EA0-1EF9, U+20AB;}/* latin-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu7GxKOzY.woff2) format('woff2'); unicode-range: U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;}/* latin */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu4mxK.woff2) format('woff2'); unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;}/* cyrillic-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu72xKOzY.woff2) format('woff2'); unicode-range: U+0460-052F, U+1C80-1C88, U+20B4, U+2DE0-2DFF, U+A640-A69F, U+FE2E-FE2F;}/* cyrillic */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu5mxKOzY.woff2) format('woff2'); unicode-range: U+0301, U+0400-045F, U+0490-0491, U+04B0-04B1, U+2116;}/* greek-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu7mxKOzY.woff2) format('woff2'); unicode-range: U+1F00-1FFF;}/* greek */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu4WxKOzY.woff2) format('woff2'); unicode-range: U+0370-03FF;}/* vietnamese */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu7WxKOzY.woff2) format('woff2'); unicode-range: U+0102-0103, U+0110-0111, U+0128-0129, U+0168-0169, U+01A0-01A1, U+01AF-01B0, U+1EA0-1EF9, U+20AB;}/* latin-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu7GxKOzY.woff2) format('woff2'); unicode-range: U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;}/* latin */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 400; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOmCnqEu92Fr1Mu4mxK.woff2) format('woff2'); unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;}/* cyrillic-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fCRc4EsA.woff2) format('woff2'); unicode-range: U+0460-052F, U+1C80-1C88, U+20B4, U+2DE0-2DFF, U+A640-A69F, U+FE2E-FE2F;}/* cyrillic */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fABc4EsA.woff2) format('woff2'); unicode-range: U+0301, U+0400-045F, U+0490-0491, U+04B0-04B1, U+2116;}/* greek-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fCBc4EsA.woff2) format('woff2'); unicode-range: U+1F00-1FFF;}/* greek */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fBxc4EsA.woff2) format('woff2'); unicode-range: U+0370-03FF;}/* vietnamese */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fCxc4EsA.woff2) format('woff2'); unicode-range: U+0102-0103, U+0110-0111, U+0128-0129, U+0168-0169, U+01A0-01A1, U+01AF-01B0, U+1EA0-1EF9, U+20AB;}/* latin-ext */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fChc4EsA.woff2) format('woff2'); unicode-range: U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;}/* latin */@font-face{font-family: 'Roboto'; font-style: normal; font-weight: 500; src: url(https://fonts.gstatic.com/s/roboto/v30/KFOlCnqEu92Fr1MmEU9fBBc4.woff2) format('woff2'); unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;} </style>");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/vhr-common.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/vhr_1_0.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/warranty-check-styles.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/vhrHeaderBlock.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/cip_2_1.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/vhr_1_CIP.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/upsellButton.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/valuebadge.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/css/tradeInLeadsSection.css", "");
                responseHTML = responseHTML.Replace("https://api.allreports.tools/wp-content/themes/apicarfaxpro/carfax/car-fax-files/img/carfox-header-accident-superiority.png", "");
                responseHTML = responseHTML.Replace("class=\"backToTop\"", "class=\"backToTop\" style=\"display:none\"");

                #endregion

                await File.WriteAllTextAsync(path, responseHTML);

                return true;
            }

            return false;
        }

        public async Task Refund(string phoneno)
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == "+994" + phoneno);

            appUser.Balance += 4;

            await _userManager.UpdateAsync(appUser);
        }

        public async Task<int> GetUserBalance()
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);

            return appUser.Balance;
        }

        public async Task<string> GetUserPhoneNumber()
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);

            return appUser.PhoneNumber[4..];
        }

        public async Task SubstractFromBalance()
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);

            appUser.Balance -= 4;

            await _userManager.UpdateAsync(appUser);
        }
    }
}