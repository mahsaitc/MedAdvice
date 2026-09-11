using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MedAdvice.Areas.Customer.Controllers;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Controllers;
using MedAdvice.Models;
using MedAdvice.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedAdvice.Tests.NullSafety
{
    /// Batch four turned a family of unhandled nulls into handled responses. Every case here
    /// produced a 500 before that change.
    public class NullSafetyTests : IDisposable
    {
        readonly TestDatabase database = new TestDatabase();
        readonly IdentityTestHost identity;

        public NullSafetyTests()
        {
            identity = new IdentityTestHost(database);
        }

        HomeController HomeController()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            IHttpClientFactory clients = new ServiceCollection()
                .AddHttpClient().BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>();

            HomeController controller = new HomeController(identity.Db, identity.Users, configuration, clients);
            TestContext.Prepare(controller);
            return controller;
        }

        CustomerController CustomerController(ApplicationUser signedInAs)
        {
            CustomerController controller = new CustomerController(identity.Db, identity.Users);
            TestContext.Prepare(controller, signedInAs, new FakeSession());
            return controller;
        }

        [Fact]
        public async Task A_missing_advice_is_not_found_rather_than_an_exception()
        {
            Assert.IsType<NotFoundResult>(await HomeController().ViewAdviceDetails(999999));
        }

        [Fact]
        public async Task A_missing_blog_is_not_found()
        {
            Assert.IsType<NotFoundResult>(await HomeController().BlogDetails(999999));
        }

        [Fact]
        public async Task A_missing_doctor_is_not_found()
        {
            Assert.IsType<NotFoundResult>(await HomeController().DoctorDetails(999999));
        }

        [Fact]
        public async Task A_missing_product_is_not_found()
        {
            ApplicationUser user = await identity.CreateUserAsync("shopper@test.local");
            Assert.IsType<NotFoundResult>(await CustomerController(user).ProductDetails(999999));
        }

        [Fact]
        public async Task An_existing_advice_renders()
        {
            AdviceCategory category = new AdviceCategory { AdviceCategoryname = "c" };
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();

            Advice advice = new Advice { AdviceTitle = "Real", AdviceCategoryId = category.Id };
            identity.Db.Add(advice);
            await identity.Db.SaveChangesAsync();

            Assert.IsType<ViewResult>(await HomeController().ViewAdviceDetails(advice.Id));
        }

        [Fact]
        public async Task A_user_with_no_basket_sees_an_empty_basket_instead_of_a_crash()
        {
            // The sharpest case in batch four: FirstOrDefaultAsync returns null whenever a
            // user has never added anything, which is the normal state for a new account, so
            // every signed-in user hit a 500 the first time they opened the basket.
            ApplicationUser user = await identity.CreateUserAsync("empty@test.local");

            IActionResult result = await CustomerController(user).PurchaseCartManagement();

            ViewResult view = Assert.IsType<ViewResult>(result);
            Purchasecart model = Assert.IsType<Purchasecart>(view.Model);
            Assert.NotNull(model.PurchaseCartItems);
            Assert.Empty(model.PurchaseCartItems);
            Assert.Equal("0", view.ViewData["totalprice"]);
        }

        [Fact]
        public async Task A_user_with_a_basket_sees_it()
        {
            ApplicationUser user = await identity.CreateUserAsync("shopper@test.local");
            identity.Db.Add(new Purchasecart { UserId = user.Id, isOpen = true, createdDate = DateTime.UtcNow });
            await identity.Db.SaveChangesAsync();

            IActionResult result = await CustomerController(user).PurchaseCartManagement();

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Purchasecart>(view.Model);
        }

        [Fact]
        public void The_error_action_renders_without_touching_the_database()
        {
            // It is the last line of defence, so it must not be able to fail.
            IActionResult result = HomeController().Error(500);

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Equal(500, view.ViewData["StatusCode"]);
            Assert.IsType<ErrorViewModel>(view.Model);
        }

        [Fact]
        public void The_error_action_handles_a_missing_status_code()
        {
            IActionResult result = HomeController().Error(null);

            ViewResult view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewData["StatusCode"]);
        }

        public void Dispose()
        {
            identity.Dispose();
            database.Dispose();
        }
    }
}
