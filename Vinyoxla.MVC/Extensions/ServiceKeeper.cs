using Vinyoxla.Core;
using Vinyoxla.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vinyoxla.Service.Implementations;
using Vinyoxla.Service.Interfaces;

namespace Vinyoxla.MVC.Extensions
{
    public static class ServiceKeeper
    {
        public static void ServicesBuilder(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IPurchaseService, PurchaseService>();
            services.AddScoped<IAdminHomeService, AdminHomeService>();
            services.AddScoped<IAdminRelationService, AdminRelationService>();
            services.AddScoped<IAdminAccountService, AdminAccountService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IReportService, ReportService>();
        }
    }
}