using System;
using MedAdvice.Areas.Identity.Data;
using System.Threading.Tasks;
using MedAdvice.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(MedAdvice.Areas.Identity.IdentityHostingStartup))]
namespace MedAdvice.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<MedAdviceDb>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("MedAdviceDbConnection")));

                services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<MedAdviceDb>();
                    
                services.Configure<IdentityOptions>(x =>
                {
                    x.Lockout.MaxFailedAccessAttempts = 3;
                    x.Lockout.AllowedForNewUsers = true;
                    x.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(80);

                    x.Password.RequiredLength = 8;
                    x.Password.RequireDigit = true;
                    x.Password.RequireLowercase = true;
                    x.Password.RequireUppercase = true;
                    x.Password.RequireNonAlphanumeric = true;

                });
                
                services.ConfigureApplicationCookie(x =>
                {
                    x.Cookie.HttpOnly = true;
                    x.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    x.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                    x.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = y =>
                        {
                            y.Response.Redirect("/Account/signinsignup");
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = y =>
                        {
                            y.Response.Redirect("/Account/SignInSignUp");
                            return Task.CompletedTask;
                        }

                    };
                });
                services.AddAuthorization(x =>
                {
                    x.AddPolicy("AdminsPolicy", p => p.RequireRole("admins"));
                });

            });
        }
    }
}