using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity;
using MedAdvice.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MedAdvice.IntegrationTests
{
    /// Seeding had no coverage at all before this. It also had to move: it used to run from
    /// Startup.Configure, blocking on async work and holding two scoped services for the life
    /// of the process. Moving it into Program.Main would have been the obvious fix and the
    /// wrong one -- WebApplicationFactory reflects over CreateHostBuilder and builds the host
    /// itself, so the body of Main never runs for a test host and seeding would have silently
    /// stopped happening here. A hosted service runs in both.
    public class IdentitySeedingTests
    {
        static WebApplicationFactory<Program> WithAdminSeed(MedAdviceFactory factory,
            string userName, string password, string email = "seeded-admin@test.local")
        {
            return factory.WithWebHostBuilder(builder =>
                builder.ConfigureAppConfiguration((context, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["AdminSeed:UserName"] = userName,
                        ["AdminSeed:Password"] = password,
                        ["AdminSeed:Email"] = email
                    })));
        }

        static T Resolve<T>(WebApplicationFactory<Program> factory) where T : notnull
        {
            return factory.Services.CreateScope().ServiceProvider.GetRequiredService<T>();
        }

        [Fact]
        public void The_seeder_is_registered_as_a_hosted_service()
        {
            // If this regresses to inline code in Main, the host still builds and every other
            // test here keeps passing on the real host while silently doing nothing in tests.
            using MedAdviceFactory factory = new MedAdviceFactory();

            IEnumerable<IHostedService> hosted = factory.Services.GetServices<IHostedService>();

            Assert.Contains(hosted, service => service is IdentitySeedHostedService);
        }

        [Fact]
        public async Task Roles_are_created_even_when_no_admin_is_configured()
        {
            // AdminSeed is empty in committed configuration, so this is the default path.
            using MedAdviceFactory factory = new MedAdviceFactory();
            factory.CreateSessionClient();

            RoleManager<IdentityRole> roles = Resolve<RoleManager<IdentityRole>>(factory);

            Assert.True(await roles.RoleExistsAsync("admins"));
            Assert.True(await roles.RoleExistsAsync("customers"));
        }

        [Fact]
        public async Task No_administrator_is_provisioned_when_AdminSeed_is_empty()
        {
            using MedAdviceFactory factory = new MedAdviceFactory();
            factory.CreateSessionClient();

            UserManager<ApplicationUser> users = Resolve<UserManager<ApplicationUser>>(factory);

            Assert.Empty(await users.GetUsersInRoleAsync("admins"));
        }

        [Fact]
        public async Task An_administrator_is_provisioned_when_AdminSeed_is_configured()
        {
            using MedAdviceFactory factory = new MedAdviceFactory();
            using WebApplicationFactory<Program> seeded =
                WithAdminSeed(factory, "seeded-admin@test.local", "Str0ng!Pass");

            seeded.CreateClient();

            UserManager<ApplicationUser> users = Resolve<UserManager<ApplicationUser>>(seeded);
            ApplicationUser admin = await users.FindByNameAsync("seeded-admin@test.local");

            Assert.NotNull(admin);
            Assert.True(await users.IsInRoleAsync(admin, "admins"));
            Assert.True(admin.EmailConfirmed);
        }

        [Fact]
        public async Task Seeding_twice_does_not_create_a_second_administrator()
        {
            // Every restart runs this. Two hosts over one database stands in for that.
            using MedAdviceFactory factory = new MedAdviceFactory();

            using (WebApplicationFactory<Program> first =
                WithAdminSeed(factory, "seeded-admin@test.local", "Str0ng!Pass"))
            {
                first.CreateClient();
            }

            using WebApplicationFactory<Program> second =
                WithAdminSeed(factory, "seeded-admin@test.local", "Str0ng!Pass");
            second.CreateClient();

            UserManager<ApplicationUser> users = Resolve<UserManager<ApplicationUser>>(second);

            Assert.Single(await users.GetUsersInRoleAsync("admins"));
        }

        [Fact]
        public async Task A_password_that_fails_the_policy_leaves_the_host_running()
        {
            // The seeder logs and continues rather than throwing: a misconfigured AdminSeed
            // should not stop the site from starting. Roles are still created.
            using MedAdviceFactory factory = new MedAdviceFactory();
            using WebApplicationFactory<Program> seeded =
                WithAdminSeed(factory, "weak-admin@test.local", "abc");

            System.Net.Http.HttpResponseMessage response =
                await seeded.CreateClient().GetAsync("/Home/Home");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            UserManager<ApplicationUser> users = Resolve<UserManager<ApplicationUser>>(seeded);
            RoleManager<IdentityRole> roles = Resolve<RoleManager<IdentityRole>>(seeded);

            Assert.True(await roles.RoleExistsAsync("admins"));
            Assert.Null(await users.FindByNameAsync("weak-admin@test.local"));
        }
    }
}
