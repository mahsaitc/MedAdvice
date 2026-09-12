using System;
using System.Linq;
using System.Net.Http;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using MedAdvice.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MedAdvice.IntegrationTests
{
    /// A real host, through the real middleware pipeline, with two swaps: the SQL Server
    /// database that IdentityHostingStartup wires up is replaced with an in-memory SQLite
    /// one (Group C is what exercises the real engine, see SqlServerFixture), and the
    /// environment is forced to Production so Startup.Configure takes the branch that
    /// registers UseExceptionHandler and UseStatusCodePagesWithReExecute -- the Development
    /// branch (UseDeveloperExceptionPage) is what a plain WebApplicationFactory would give
    /// by default, and it is not what Group B's error-page tests are checking.
    ///
    /// IdentityHostingStartup registers MedAdviceDb via [assembly: HostingStartup], which
    /// runs before Startup.ConfigureServices. Swapping the registration has to happen in
    /// ConfigureTestServices specifically -- it is the one hook guaranteed to run after both.
    public sealed class MedAdviceFactory : WebApplicationFactory<Program>
    {
        readonly SqliteConnection connection;

        public MedAdviceFactory()
        {
            // Held open for the factory's lifetime: an in-memory SQLite database only
            // exists while a connection to it is open, and every request needs the same
            // database the previous one left behind.
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            using (MedAdviceDb context = new MedAdviceDb(
                new DbContextOptionsBuilder<MedAdviceDb>().UseSqlite(connection).Options))
            {
                context.Database.EnsureCreated();
                Seed(context);
            }
        }

        /// The minimum other tables need so the actions under test do not fail for reasons
        /// unrelated to what they are testing -- a cart item pointing at a real product, a
        /// comment pointing at a real advice article. No secrets, no per-test data:
        /// individual tests create their own users.
        static void Seed(MedAdviceDb context)
        {
            Brand brand = new Brand { name = "Test brand" };
            ProductCategory category = new ProductCategory { CategoryName = "Test category" };
            AdviceCategory adviceCategory = new AdviceCategory { AdviceCategoryname = "Test category" };
            context.Add(brand);
            context.Add(category);
            context.Add(adviceCategory);
            context.SaveChanges();

            context.Add(new Product
            {
                englishname = "Test product",
                BrandId = brand.Id,
                ProductCategoryId = category.Id
            });
            context.Add(new Advice
            {
                AdviceTitle = "Test advice",
                AdviceCategoryId = adviceCategory.Id
            });
            context.SaveChanges();
        }

        /// A test client whose cookies survive: every cookie this app issues -- session,
        /// antiforgery, the Identity auth cookie -- is forced Secure by the global cookie
        /// policy in Startup.ConfigureServices. HttpClient's CookieContainer will not store
        /// or resend a Secure cookie against a plain "http" base address, so tests need an
        /// "https" one even though nothing here actually speaks TLS; TestServer does not
        /// care. Redirects are not auto-followed so tests can assert on the redirect itself.
        public HttpClient CreateSessionClient()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");

            builder.ConfigureTestServices(services =>
            {
                ServiceDescriptor descriptor = services.SingleOrDefault(
                    x => x.ServiceType == typeof(DbContextOptions<MedAdviceDb>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<MedAdviceDb>(options => options.UseSqlite(connection));

                services.AddSingleton<IStartupFilter, IntegrationTestHooksStartupFilter>();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                connection.Dispose();
            }
        }
    }

    /// Two endpoints that exist only in the test host, mapped after Startup.Configure has
    /// already added routing, authentication and the exception handler -- so both sit
    /// downstream of them in the pipeline, the same as any real endpoint would, and neither
    /// is reachable through MVC routing (UseAuthorization only enforces [Authorize] on a
    /// matched endpoint, and these are not one).
    ///
    /// /sign-in: establishes a real Identity auth cookie for a user the test already
    /// created, without going through the password/captcha flow (CaptchaService calls out
    /// to Google, which has no place in a test run).
    ///
    /// /throw: the routing tests need a genuinely unhandled exception to prove
    /// UseExceptionHandler produces the error page rather than a 404. Every unsafe lookup
    /// in the real controllers was already fixed (see NullSafetyTests), so nothing in the
    /// app can be provoked into throwing on demand any more -- which is a good sign for the
    /// app and the reason this exists.
    sealed class IntegrationTestHooksStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);

                app.Map("/__integration-tests/throw", branch => branch.Run(_ =>
                    throw new InvalidOperationException(
                        "Deliberate failure for the unhandled-exception routing test.")));

                app.Map("/__integration-tests/sign-in", branch => branch.Run(async context =>
                {
                    string userId = context.Request.Query["userId"];
                    UserManager<ApplicationUser> userManager =
                        context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    SignInManager<ApplicationUser> signInManager =
                        context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

                    ApplicationUser user = string.IsNullOrEmpty(userId)
                        ? null
                        : await userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                }));
            };
        }
    }
}
