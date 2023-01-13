using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vinyoxla.Service.Mappings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vinyoxla.Data;
using Vinyoxla.Service.Exceptions;
using Vinyoxla.Service.ViewModels.PurchaseVMs;
using Vinyoxla.MVC.Extensions;

namespace Vinyoxla.MVC
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            //eto iz https://stackoverflow.com/questions/62711146/fluentvalidation-error-doesnt-show-in-view-after-moved-to-net-standard-2-0
            //services.AddFluentValidation(fvc => fvc.RegisterValidatorsFromAssemblyContaining<PurchaseVM>());
            //poguglit kak pralno zat fluent valid v startape shto b on vozvrashal mne message errora

            services.IdentityBuilder();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Default"));
            });

            services.AddAutoMapper(options =>
            {
                options.AddProfile(new MappingProfile());
            });

            services.ServicesBuilder();

            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.ExceptionHandling();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute("Default", "{controller=home}/{action=index}/{id?}");
            });
        }
    }
}
