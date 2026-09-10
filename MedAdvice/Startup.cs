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
            services.AddSingleton(typeof(CaptchaService));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
           UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)//1-moarefi usermanager va rolemanager

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
            ///2 - seda karad tabe identityinitializer
            Identityinitializer(userManager, roleManager).Wait();
        }
        ///3-nevashtan tabe identityinitializer
        private async Task Identityinitializer(UserManager<ApplicationUser> usermanager,
            RoleManager<IdentityRole> rolemanager)
        {
            ///4-sakhte list roleha
            List<string> roles = new List<string> { "admins", "customers" };
            ////5-farakhani list va agar nist sakhtan har item
            foreach (var item in roles)
            {
                if ((await rolemanager.RoleExistsAsync(item)) == false)
                {
                    IdentityRole identityRole = new IdentityRole(item);
                    /////6-sakhte roleha dar rolemanager
                    await rolemanager.CreateAsync(identityRole);
                }
            }
            ////7-saerch username  admin dar usermanager va sakhtan an agar nabud
            string adminUserName = Configuration["AdminSeed:UserName"];
            string adminPassword = Configuration["AdminSeed:Password"];
            if (string.IsNullOrEmpty(adminUserName) || string.IsNullOrEmpty(adminPassword))
            {
                // No admin seed configured: roles are still created, no account is provisioned.
                return;
            }
            ApplicationUser admin = await usermanager.FindByNameAsync(adminUserName);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    firstname = "admin",
                    lastname = "admin",
                    Email = Configuration["AdminSeed:Email"],
                    EmailConfirmed = true

                };
                ///8-ezafe kardan admin va password be usermanager
                IdentityResult adminResult = await usermanager.CreateAsync(admin, adminPassword);
                if (adminResult.Succeeded == false)
                {
                    return;
                }
            }
           
            ////9- barrasi budan admin dar roleha va agar nabud ezafe kardane an be rolehaye admins 
            if (await usermanager.IsInRoleAsync(admin, "admins") == false)
            {
                await usermanager.AddToRoleAsync(admin, "admins");
            }
        }
    }
}
