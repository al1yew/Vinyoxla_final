using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vinyoxla.Core;
using Vinyoxla.Core.Models;
using Vinyoxla.Service.Interfaces;
using Vinyoxla.Service.ViewModels;
using Vinyoxla.Service.ViewModels.AccountVMs;
using Vinyoxla.Service.ViewModels.AppUserToVincodeVMs;
using Vinyoxla.Service.ViewModels.BankVMs;
using Vinyoxla.Service.ViewModels.UserVMs;

namespace Vinyoxla.Service.Implementations
{
    public class AccountService : IAccountService
    {
        private IConfiguration Configuration { get; }
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CheckLogin(LoginVM loginVM)
        {
            Regex regexNumber = new Regex("^[0-9]+$");

            if (regexNumber.IsMatch(loginVM.PhoneNumber.ToString()))
            {
                if (!(loginVM.PhoneNumber.StartsWith("50") ||
                    loginVM.PhoneNumber.StartsWith("10") ||
                    loginVM.PhoneNumber.StartsWith("51") ||
                    loginVM.PhoneNumber.StartsWith("70") ||
                    loginVM.PhoneNumber.StartsWith("77") ||
                    loginVM.PhoneNumber.StartsWith("99") ||
                    loginVM.PhoneNumber.StartsWith("55")))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public async Task<int> SendCode(string number)
        {
            string phone = "+994" + number;

            Random random = new Random();
            int generatedcode = random.Next(1000, 9999);

            string url = $"http://api.msm.az/sendsms?user={Environment.GetEnvironmentVariable("Username")}&password={Environment.GetEnvironmentVariable("Apikey")}&gsm={phone}&from={Environment.GetEnvironmentVariable("From")}&text=Code: {generatedcode}\nXoş gəldiniz!";
            //string url = $"http://api.msm.az/sendsms?user={Configuration.GetSection("MSM:Username").Value}&password={Configuration.GetSection("MSM:Apikey").Value}&gsm={phone}&from={Configuration.GetSection("MSM:From").Value}&text=Code: {generatedcode}\nXoş gəldiniz!";

            HttpResponseMessage response = null;

            string responsedecoded = "";

            using (HttpClient client = new HttpClient())
            {
                response = await client.GetAsync(url);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (response.IsSuccessStatusCode)
            {
                responsedecoded = await response.Content.ReadAsStringAsync();

                if (responsedecoded.Contains("errno=100"))
                {
                    return generatedcode;
                }
            }

            return 0;
        }

        public async Task<List<string>> Login(LoginVM loginVM, int code)
        {
            AppUser appUser = await _userManager.Users.Include(u => u.AppUserToVincodes).ThenInclude(x => x.VinCode).FirstOrDefaultAsync(u => u.UserName == "+994" + loginVM.PhoneNumber);

            List<string> errors = new List<string>();

            if (loginVM.Code == code)
            {
                if (appUser == null)
                {
                    AppUser newUser = new AppUser()
                    {
                        UserName = "+994" + loginVM.PhoneNumber,
                        PhoneNumber = "+994" + loginVM.PhoneNumber,
                        PhoneNumberConfirmed = false,
                        Balance = 0,
                        IsAdmin = false,
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    };

                    IdentityResult result = await _userManager.CreateAsync(newUser);

                    if (!result.Succeeded)
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            errors.Add(error.Description);
                        }

                        return errors;
                    }

                    result = await _userManager.AddToRoleAsync(newUser, "Member");

                    await _signInManager.SignInAsync(newUser, true);
                }
                else
                {
                    appUser.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(appUser);

                    await _signInManager.SignInAsync(appUser, true);
                }
            }
            else
            {
                errors.Add("Kodu səhv daxil etmisiniz.");
            }

            return errors;
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<AccountVM> Profile()
        {
            AppUserGetVM appUser = _mapper.Map<AppUserGetVM>(
                await _userManager.Users
                .Include(x => x.AppUserToVincodes)
                .ThenInclude(x => x.VinCode)
                .FirstOrDefaultAsync(x =>
                x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name));

            List<AppUserToVincodeVM> appUserToVincodes = _mapper.Map<List<AppUserToVincodeVM>>(await _unitOfWork.AppUserToVincodeRepository.GetAllByExAsync(x =>
            x.AppUserId == appUser.Id, "AppUser", "VinCode"));

            IQueryable<AppUserToVincodeVM> query = appUserToVincodes.AsQueryable();

            query = query.OrderBy(x => x.CreatedAt);

            AccountVM accountVM = new AccountVM()
            {
                AppUserToVincodes = PaginationList<AppUserToVincodeVM>.Create(query, 1, 10),
                Balance = appUser.Balance
            };

            return accountVM;
        }

        public async Task<PaginationList<AppUserToVincodeVM>> Sort(int page, string vin, int sortbydate, int showcount)
        {
            AppUser appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);

            List<AppUserToVincodeVM> appUserToVincodes = _mapper.Map<List<AppUserToVincodeVM>>(await _unitOfWork.AppUserToVincodeRepository.GetAllByExAsync(x =>
            x.AppUserId == appUser.Id, "AppUser", "VinCode"));

            IQueryable<AppUserToVincodeVM> query = appUserToVincodes.AsQueryable();

            if (vin != null)
            {
                query = query.Where(x => x.VinCode.Vin.Contains(vin.Trim().ToUpperInvariant())).OrderBy(x => x.CreatedAt);
            }

            if (sortbydate == 2)
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            return PaginationList<AppUserToVincodeVM>.Create(query, page == 0 ? 1 : page, showcount == 0 ? 1 : showcount);
        }

        public async Task<string> Bank(string amount)
        {
            string url = $"https://3dsrv.kapitalbank.az:5443/Exec";
            //https://tstpg.kapitalbank.az:5443/Exec
            //https://3dsrv.kapitalbank.az:5443/Exec
            //E1090050
            //E1000010

            string xml =
                "<TKKPG>" +
                    "<Request>" +
                        "<Operation>CreateOrder</Operation>" +
                        "<Language>AZ</Language>" +
                        "<Order>" +
                            "<OrderType>Purchase</OrderType>" +
                            "<Merchant>E1090050</Merchant>" +
                            $"<Amount>{int.Parse(amount) * 100}</Amount>" +
                            "<Currency>944</Currency>" +
                            $"<Description>{_httpContextAccessor.HttpContext.User.Identity.Name} Phone / {amount} azn</Description>" +
                            "<ApproveURL>https://vinyoxla.az/Account/UpdateBalance</ApproveURL>" +
                            "<CancelURL>https://vinyoxla.az/Account/Error?errno=10</CancelURL>" +
                            "<DeclineURL>https://vinyoxla.az/Purchase/Error?errno=10</DeclineURL>" +
                        "</Order>" +
                    "</Request>" +
                "</TKKPG>";

            #region crt zad

            byte[] PublicCertificate = Encoding.Unicode.GetBytes(Environment.GetEnvironmentVariable("CRT"));
            byte[] PrivateKey = Convert.FromBase64String(Environment.GetEnvironmentVariable("KEY"));

            //byte[] PublicCertificate = Encoding.Unicode.GetBytes(Configuration.GetSection("SSL:CRT").Value);
            //byte[] PrivateKey = Convert.FromBase64String(Configuration.GetSection("SSL:KEY").Value);


            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(PrivateKey, out _);

            X509Certificate2 publicCertificate = new X509Certificate2(PublicCertificate);
            publicCertificate = publicCertificate.CopyWithPrivateKey(rsa);
            publicCertificate = new X509Certificate2(publicCertificate.Export(X509ContentType.Pkcs12));

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            handler.ClientCertificates.Add(new X509Certificate2(PublicCertificate));
            handler.ClientCertificates.Add(new X509Certificate2(publicCertificate));

            StringContent content = new StringContent(xml);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");

            #endregion

            HttpResponseMessage response = null;

            TKKPG tkkpg = new TKKPG();

            using (HttpClient client = new HttpClient(handler))
            {
                response = await client.PostAsync(url, content);
            }

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();

                XmlSerializer ser = new XmlSerializer(typeof(TKKPG));

                using (StringReader sr = new StringReader(result))
                {
                    tkkpg = (TKKPG)ser.Deserialize(sr);
                }

                if (tkkpg.Response.Status == "00")
                {
                    string newUrl = tkkpg.Response.Order.URL
                        + "?ORDERID=" + tkkpg.Response.Order.OrderID
                        + "&SESSIONID=" + tkkpg.Response.Order.SessionID;

                    TopUpVM topUpVM = new TopUpVM()
                    {
                        OrderId = tkkpg.Response.Order.OrderID,
                        SessionId = tkkpg.Response.Order.SessionID,
                        Amount = amount,
                        Phone = _httpContextAccessor.HttpContext.User.Identity.Name
                    };

                    string topUp = _httpContextAccessor.HttpContext.Request.Cookies["topUp"];

                    if (!string.IsNullOrWhiteSpace(topUp))
                    {
                        topUp = null;
                    }

                    topUp = JsonConvert.SerializeObject(topUpVM);

                    _httpContextAccessor.HttpContext.Response.Cookies.Append("topUp", topUp);

                    return newUrl;
                }
            }

            return null;
        }

        public async Task<bool> CheckOrder(string amount, string orderid, string sessionId, string phone)
        {
            string url = $"https://3dsrv.kapitalbank.az:5443/Exec";
            //https://tstpg.kapitalbank.az:5443/Exec
            //https://3dsrv.kapitalbank.az:5443/Exec
            //E1090050
            //E1000010

            string xml =
                "<TKKPG>" +
                    "<Request>" +
                        "<Operation>GetOrderStatus</Operation>" +
                        "<Language>AZ</Language>" +
                        "<Order>" +
                            "<Merchant>E1090050</Merchant>" +
                            $"<OrderID>{orderid}</OrderID>" +
                        "</Order>" +
                        $"<SessionID>{sessionId}</SessionID>" +
                    "</Request>" +
                "</TKKPG>";

            #region crt zad

            byte[] PublicCertificate = Encoding.Unicode.GetBytes(Environment.GetEnvironmentVariable("CRT"));
            byte[] PrivateKey = Convert.FromBase64String(Environment.GetEnvironmentVariable("KEY"));

            //byte[] PublicCertificate = Encoding.Unicode.GetBytes(Configuration.GetSection("SSL:CRT").Value);
            //byte[] PrivateKey = Convert.FromBase64String(Configuration.GetSection("SSL:KEY").Value);

            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(PrivateKey, out _);

            X509Certificate2 publicCertificate = new X509Certificate2(PublicCertificate);
            publicCertificate = publicCertificate.CopyWithPrivateKey(rsa);
            publicCertificate = new X509Certificate2(publicCertificate.Export(X509ContentType.Pkcs12));

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            handler.ClientCertificates.Add(new X509Certificate2(PublicCertificate));
            handler.ClientCertificates.Add(new X509Certificate2(publicCertificate));

            StringContent content = new StringContent(xml);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");

            #endregion

            HttpResponseMessage response = null;

            TKKPG tkkpg = new TKKPG();

            using (HttpClient client = new HttpClient(handler))
            {
                response = await client.PostAsync(url, content);
            }

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();

                XmlSerializer ser = new XmlSerializer(typeof(TKKPG));

                using (StringReader sr = new StringReader(result))
                {
                    tkkpg = (TKKPG)ser.Deserialize(sr);
                }

                if (tkkpg.Response.Status == "00" && tkkpg.Response.Order.OrderStatus == "APPROVED")
                {
                    return true;
                }
                else
                {
                    AppUser appUser = await _userManager.FindByNameAsync(phone);

                    #region Event handle

                    Event newUserEvent = new Event()
                    {
                        AppUser = appUser,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        IsFromApi = false,
                        IsApiError = false,
                        DidRefundToBalance = false,
                        ErrorWhileRenew = false,
                        ErrorWhileReplace = false,
                        FileExists = false,
                        IsRenewedDueToAbsence = false,
                        IsRenewedDueToExpire = false,
                        Vin = "TOPUP",
                        EventMessages = new List<EventMessage>()
                        {
                            new EventMessage()
                            {
                                Message = "user sistemi qirmag istedi, pulu odemeyib balansi artirmag istedi, cehdine son goydug" +
                                "event yaratdig, getsin garnin gashisin",
                                CreatedAt = DateTime.UtcNow.AddHours(4)
                            }
                        }
                    };

                    #endregion

                    await _unitOfWork.EventRepository.AddAsync(newUserEvent);
                    await _unitOfWork.CommitAsync();
                }

                return false;
            }

            return false;
        }

        public async Task UpdateBalance(string amount, string orderId, string sessionId, string phone)
        {
            AppUser appUser = await _userManager.FindByNameAsync(phone);

            appUser.Balance += int.Parse(amount);

            Transaction transaction = new Transaction()
            {
                AppUser = appUser,
                Amount = int.Parse(amount),
                CreatedAt = DateTime.UtcNow.AddHours(4),
                IsFromBalance = false,
                IsTopUp = true,
                OrderId = orderId,
                PaymentIsSuccessful = true,
                SessionId = sessionId
            };

            await _unitOfWork.TransactionRepository.AddAsync(transaction);
            await _unitOfWork.CommitAsync();
        }
    }
}
