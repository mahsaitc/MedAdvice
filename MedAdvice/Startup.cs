using MedAdvice.Areas.Identity.Data;
using MedAdvice.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.Configure<CookiePolicyOptions>(x =>
            {
                x.CheckConsentNeeded = y => false;
                x.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                x.Secure = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            });

            services.AddSession(x =>
            {
                x.Cookie.HttpOnly = true;
                x.Cookie.IsEssential = true;
                x.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                x.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            });
            services.AddHttpClient();
            // Runs on host start, for the real host and any test host built from
            // CreateHostBuilder alike. See IdentitySeedHostedService.
            services.AddHostedService<MedAdvice.Areas.Identity.IdentitySeedHostedService>();
            services.AddSingleton(typeof(CaptchaService));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
        
                endpoints.MapControllerRoute(
                   name: "areas",
                   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=home}/{id?}");
            });
        }
    }
}
