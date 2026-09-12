using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedAdvice.Areas.Identity
{
    /// Creates the application roles, and the seeded administrator when AdminSeed is
    /// configured.
    ///
    /// A hosted service rather than statements in Program.Main, because the test host is
    /// built by WebApplicationFactory, which reflects over CreateHostBuilder and builds the
    /// host itself -- it never executes the body of Main. It does call StartAsync on the
    /// host, so anything registered here runs for the real host and the test host alike.
    /// Previously this ran from Startup.Configure, which blocked on async work with .Wait()
    /// and took UserManager and RoleManager as parameters, resolving two scoped services
    /// from the root container for the lifetime of the process.
    public sealed class IdentitySeedHostedService : IHostedService
    {
        readonly IServiceProvider services;
        readonly IConfiguration configuration;
        readonly ILogger<IdentitySeedHostedService> logger;

        public IdentitySeedHostedService(IServiceProvider services, IConfiguration configuration,
            ILogger<IdentitySeedHostedService> logger)
        {
            this.services = services;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // An explicit scope: UserManager, RoleManager and the DbContext behind them are
            // scoped, and resolving them from the root provider would keep one context alive
            // for the whole process.
            using (IServiceScope scope = services.CreateScope())
            {
                try
                {
                    await SeedAsync(
                        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
                }
                catch (Exception exception)
                {
                    // Reaching the database is the usual reason this fails. Logging and
                    // continuing keeps the rest of the site serving; throwing here would stop
                    // the host from starting at all.
                    logger.LogError(exception, "Identity seeding failed. Roles and the seeded administrator may be missing.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            List<string> roles = new List<string> { "admins", "customers" };
            foreach (string role in roles)
            {
                if (await roleManager.RoleExistsAsync(role) == false)
                {
                    IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (roleResult.Succeeded == false)
                    {
                        logger.LogError("Could not create the role {Role}: {Errors}",
                            role, DescribeErrors(roleResult));
                        return;
                    }
                }
            }

            string adminUserName = configuration["AdminSeed:UserName"];
            string adminPassword = configuration["AdminSeed:Password"];
            if (string.IsNullOrEmpty(adminUserName) || string.IsNullOrEmpty(adminPassword))
            {
                // No admin seed configured: roles are still created, no account is provisioned.
                logger.LogInformation("AdminSeed is not configured, so no administrator account was provisioned.");
                return;
            }

            ApplicationUser admin = await userManager.FindByNameAsync(adminUserName);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    firstname = "admin",
                    lastname = "admin",
                    Email = configuration["AdminSeed:Email"],
                    EmailConfirmed = true
                };

                IdentityResult adminResult = await userManager.CreateAsync(admin, adminPassword);
                if (adminResult.Succeeded == false)
                {
                    logger.LogError("Could not create the seeded administrator: {Errors}",
                        DescribeErrors(adminResult));
                    return;
                }

                logger.LogInformation("Seeded the administrator account {UserName}.", adminUserName);
            }

            if (await userManager.IsInRoleAsync(admin, "admins") == false)
            {
                IdentityResult roleAssignment = await userManager.AddToRoleAsync(admin, "admins");
                if (roleAssignment.Succeeded == false)
                {
                    logger.LogError("Could not add the seeded administrator to the admins role: {Errors}",
                        DescribeErrors(roleAssignment));
                }
            }
        }

        static string DescribeErrors(IdentityResult result)
        {
            return string.Join(" ", Array.ConvertAll(
                new List<IdentityError>(result.Errors).ToArray(), x => x.Description));
        }
    }
}
