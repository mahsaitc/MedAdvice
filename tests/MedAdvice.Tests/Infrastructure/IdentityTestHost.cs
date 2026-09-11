using System;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MedAdvice.Tests.Infrastructure
{
    /// A real UserManager and RoleManager over the test database.
    ///
    /// Deliberately the genuine Identity stack rather than a mock: the behaviour under test
    /// is Identity's own (password validation, role membership, lockout), and a mock would
    /// only assert that the test's own stub was called.
    public sealed class IdentityTestHost : IDisposable
    {
        readonly ServiceProvider provider;
        readonly IServiceScope scope;

        public IdentityTestHost(TestDatabase database)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
            services.AddDbContext<MedAdviceDb>(options => options.UseSqlite(database.Connection));
            // The default token providers protect their payloads, so data protection has
            // to be available before UserManager can be resolved.
            services.AddDataProtection();

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    // Mirrors IdentityHostingStartup. IdentityConfigurationTests asserts the
                    // application's real values separately, so this copy cannot drift
                    // unnoticed.
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 3;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MedAdviceDb>()
                // The admin password reset goes through Identity's own token flow, which
                // needs the default providers registered.
                .AddDefaultTokenProviders();

            provider = services.BuildServiceProvider();
            scope = provider.CreateScope();

            Db = scope.ServiceProvider.GetRequiredService<MedAdviceDb>();
            Users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        }

        public MedAdviceDb Db { get; }
        public UserManager<ApplicationUser> Users { get; }
        public RoleManager<IdentityRole> Roles { get; }

        public async Task<ApplicationUser> CreateUserAsync(string userName, string password = "Str0ng!Pass", string role = null)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = userName,
                Email = userName,
                firstname = "Test",
                lastname = "User",
                EmailConfirmed = true
            };

            IdentityResult result = await Users.CreateAsync(user, password);
            if (result.Succeeded == false)
            {
                throw new InvalidOperationException("could not create test user: " + string.Join("; ", result.Errors));
            }

            if (role != null)
            {
                if (await Roles.RoleExistsAsync(role) == false)
                {
                    await Roles.CreateAsync(new IdentityRole(role));
                }
                await Users.AddToRoleAsync(user, role);
            }

            return user;
        }

        public void Dispose()
        {
            scope.Dispose();
            provider.Dispose();
        }
    }
}
