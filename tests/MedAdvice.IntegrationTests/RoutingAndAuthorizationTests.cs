using System.Net;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedAdvice.IntegrationTests
{
    /// Group B: routing, authorization and the error pages, run through the real pipeline
    /// -- the pieces a directly-constructed controller (as the unit suite builds them)
    /// never passes through at all: UseRouting's area/controller/action resolution,
    /// UseAuthentication/UseAuthorization, the custom OnRedirectToLogin, and
    /// UseStatusCodePagesWithReExecute from PR #4.
    public class RoutingAndAuthorizationTests : IClassFixture<MedAdviceFactory>
    {
        readonly MedAdviceFactory factory;

        public RoutingAndAuthorizationTests(MedAdviceFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Unauthenticated_request_to_an_admin_route_redirects_to_signin()
        {
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/admin/User/Index");

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Account/signinsignup", response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task Non_admin_user_does_not_get_200_from_an_admin_route()
        {
            HttpClient client = factory.CreateSessionClient();
            ApplicationUser customer = await CreateUserAsync(admin: false);
            await SignIn(client, customer.Id);

            HttpResponseMessage response = await client.GetAsync("/admin/User/Index");

            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Admin_user_gets_200_from_an_admin_route()
        {
            HttpClient client = factory.CreateSessionClient();
            ApplicationUser admin = await CreateUserAsync(admin: true);
            await SignIn(client, admin.Id);

            HttpResponseMessage response = await client.GetAsync("/admin/User/Index");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Unauthenticated_request_to_a_customer_area_route_redirects_to_signin()
        {
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/customer/Customer/PurchaseCartManagement");

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Account/signinsignup", response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task Home_page_returns_200()
        {
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Missing_blog_returns_404_rendering_the_error_page()
        {
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/Home/BlogDetails/999999");
            string body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("Page not found", body);
        }

        [Fact]
        public async Task Unhandled_exception_renders_the_error_page_not_a_missing_route_404()
        {
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/__integration-tests/throw");
            string body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("An unexpected error occurred", body);
        }

        [Fact]
        public async Task Truly_unmapped_route_still_returns_a_plain_404()
        {
            // Distinguishes the exception-handler path above from ordinary routing: a path
            // nothing maps to must 404 normally, without going through the error page's
            // "unexpected error" branch.
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/this-route-does-not-exist-anywhere");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        async Task SignIn(HttpClient client, string userId)
        {
            HttpResponseMessage response = await client.GetAsync("/__integration-tests/sign-in?userId=" + userId);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        async Task<ApplicationUser> CreateUserAsync(bool admin)
        {
            using (IServiceScope scope = factory.Services.CreateScope())
            {
                UserManager<ApplicationUser> userManager =
                    scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                ApplicationUser user = new ApplicationUser
                {
                    UserName = "test_" + Guid.NewGuid().ToString("N"),
                    Email = Guid.NewGuid().ToString("N") + "@example.test",
                    EmailConfirmed = true
                };
                IdentityResult result = await userManager.CreateAsync(user, "Correct-Horse-Battery-9!");
                Assert.True(result.Succeeded, string.Join(" ", result.Errors));

                if (admin)
                {
                    await userManager.AddToRoleAsync(user, "admins");
                }

                return user;
            }
        }
    }
}
