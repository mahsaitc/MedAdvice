using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedAdvice.IntegrationTests
{
    /// The half of the CSRF hardening (Batches C2/C3) that a unit test cannot reach: the
    /// real antiforgery cookie, the real hidden field the form tag helper renders, and the
    /// real filter that compares them. Exercised against a public form (the advice comment
    /// action -- InsertAdviceCommentConfirm, reachable from any page, since a harvested
    /// antiforgery token is not tied to the page it came from, only to the session that
    /// requested it), an admin mutation (UserController.Deactivate) and a cart mutation
    /// (AddtoPurchaseCart).
    public class AntiforgeryTests : IClassFixture<MedAdviceFactory>
    {
        static readonly Regex TokenPattern = new Regex(
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.Compiled);

        readonly MedAdviceFactory factory;

        public AntiforgeryTests(MedAdviceFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Post_with_no_token_is_rejected_on_a_public_form()
        {
            HttpClient client = factory.CreateSessionClient();
            await Harvest(client);
            int adviceId = await GetSeededAdviceIdAsync();

            HttpResponseMessage response = await client.PostAsync(
                "/Home/InsertAdviceCommentConfirm",
                Form(("AdviceId", adviceId.ToString())));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_with_a_forged_token_is_rejected_on_a_public_form()
        {
            HttpClient client = factory.CreateSessionClient();
            await Harvest(client);
            int adviceId = await GetSeededAdviceIdAsync();

            HttpResponseMessage response = await client.PostAsync(
                "/Home/InsertAdviceCommentConfirm",
                Form(("__RequestVerificationToken", "not-a-real-token"), ("AdviceId", adviceId.ToString())));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_with_a_valid_token_is_not_rejected_on_a_public_form()
        {
            HttpClient client = factory.CreateSessionClient();
            string token = await Harvest(client);
            int adviceId = await GetSeededAdviceIdAsync();

            HttpResponseMessage response = await client.PostAsync(
                "/Home/InsertAdviceCommentConfirm",
                Form(("__RequestVerificationToken", token), ("AdviceId", adviceId.ToString())));

            Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task A_token_from_one_session_is_rejected_in_another()
        {
            HttpClient sessionA = factory.CreateSessionClient();
            await Harvest(sessionA);

            HttpClient sessionB = factory.CreateSessionClient();
            string tokenB = await Harvest(sessionB);

            int adviceId = await GetSeededAdviceIdAsync();

            // Session A's cookie (from its own Harvest call above), session B's field
            // value: the pair no longer matches.
            HttpResponseMessage response = await sessionA.PostAsync(
                "/Home/InsertAdviceCommentConfirm",
                Form(("__RequestVerificationToken", tokenB), ("AdviceId", adviceId.ToString())));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_requests_are_unaffected()
        {
            HttpClient client = factory.CreateSessionClient();

            HttpResponseMessage response = await client.GetAsync("/Account/SigninSignup");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Post_with_no_token_is_rejected_on_an_admin_mutation()
        {
            HttpClient client = factory.CreateSessionClient();
            ApplicationUser admin = await CreateUserAsync(admin: true);
            ApplicationUser target = await CreateUserAsync(admin: false);
            await SignIn(client, admin.Id);

            HttpResponseMessage response = await client.PostAsync(
                "/admin/User/Deactivate",
                Form(("id", target.Id)));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_with_a_valid_token_is_not_rejected_on_an_admin_mutation()
        {
            HttpClient client = factory.CreateSessionClient();
            ApplicationUser admin = await CreateUserAsync(admin: true);
            ApplicationUser target = await CreateUserAsync(admin: false);
            await SignIn(client, admin.Id);
            string token = await Harvest(client);

            HttpResponseMessage response = await client.PostAsync(
                "/admin/User/Deactivate",
                Form(("__RequestVerificationToken", token), ("id", target.Id)));

            Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_with_a_valid_token_is_not_rejected_on_a_cart_mutation()
        {
            HttpClient client = factory.CreateSessionClient();
            ApplicationUser customer = await CreateUserAsync(admin: false);
            await SignIn(client, customer.Id);
            string token = await Harvest(client);
            int productId = await GetSeededProductIdAsync();

            HttpResponseMessage response = await client.PostAsync(
                "/customer/Customer/AddtoPurchaseCart",
                Form(("__RequestVerificationToken", token), ("productid", productId.ToString())));

            Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// A GET against any page that renders a tag-helper form is enough: the form tag
        /// helper calls IAntiforgery.GetAndStoreTokens itself, which both sets the cookie on
        /// the response and embeds the matching field in the HTML.
        static async Task<string> Harvest(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync("/Account/SigninSignup");
            string html = await response.Content.ReadAsStringAsync();
            Match match = TokenPattern.Match(html);
            Assert.True(match.Success, "No antiforgery field found on /Account/SigninSignup.");
            return match.Groups[1].Value;
        }

        static FormUrlEncodedContent Form(params (string Key, string Value)[] fields)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            foreach ((string Key, string Value) field in fields)
            {
                pairs.Add(new KeyValuePair<string, string>(field.Key, field.Value));
            }
            return new FormUrlEncodedContent(pairs);
        }

        static async Task SignIn(HttpClient client, string userId)
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

        async Task<int> GetSeededProductIdAsync()
        {
            using (IServiceScope scope = factory.Services.CreateScope())
            {
                MedAdviceDb db = scope.ServiceProvider.GetRequiredService<MedAdviceDb>();
                return (await db.Products.FirstAsync()).Id;
            }
        }

        async Task<int> GetSeededAdviceIdAsync()
        {
            using (IServiceScope scope = factory.Services.CreateScope())
            {
                MedAdviceDb db = scope.ServiceProvider.GetRequiredService<MedAdviceDb>();
                return (await db.Advices.FirstAsync()).Id;
            }
        }
    }
}
