using System;
using MedAdvice.Areas.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MedAdvice.Tests.Security
{
    /// Asserts the application's real configuration by running its own IHostingStartup and
    /// reading back the options it produced.
    ///
    /// Re-declaring the policy inside the test and asserting against that would prove
    /// nothing: it would pass even if IdentityHostingStartup were deleted.
    public class IdentityConfigurationTests : IDisposable
    {
        readonly IWebHost host;
        readonly IdentityOptions identity;

        public IdentityConfigurationTests()
        {
            IWebHostBuilder builder = new WebHostBuilder().UseStartup<NoopStartup>();
            new IdentityHostingStartup().Configure(builder);

            // Build only. The pipeline, and therefore the admin seeder, runs on Start, which
            // would need a database.
            host = builder.Build();
            identity = host.Services.GetRequiredService<IOptions<IdentityOptions>>().Value;
        }

        [Fact]
        public void Password_length_is_at_least_eight()
        {
            Assert.True(identity.Password.RequiredLength >= 8,
                "password policy was relaxed to " + identity.Password.RequiredLength);
        }

        [Theory]
        [InlineData("digit")]
        [InlineData("lowercase")]
        [InlineData("uppercase")]
        [InlineData("non-alphanumeric")]
        public void Password_complexity_is_required(string requirement)
        {
            bool required = requirement switch
            {
                "digit" => identity.Password.RequireDigit,
                "lowercase" => identity.Password.RequireLowercase,
                "uppercase" => identity.Password.RequireUppercase,
                "non-alphanumeric" => identity.Password.RequireNonAlphanumeric,
                _ => throw new ArgumentOutOfRangeException(nameof(requirement))
            };

            Assert.True(required, requirement + " is no longer required");
        }

        [Fact]
        public void Lockout_is_enabled_for_new_users()
        {
            Assert.True(identity.Lockout.AllowedForNewUsers);
            Assert.True(identity.Lockout.MaxFailedAccessAttempts > 0);
        }

        [Fact]
        public void The_authentication_cookie_is_https_only_and_same_site()
        {
            CookieAuthenticationOptions cookie = host.Services
                .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.ApplicationScheme);

            Assert.Equal(CookieSecurePolicy.Always, cookie.Cookie.SecurePolicy);
            Assert.Equal(SameSiteMode.Lax, cookie.Cookie.SameSite);
            Assert.True(cookie.Cookie.HttpOnly);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        sealed class NoopStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }
    }
}
