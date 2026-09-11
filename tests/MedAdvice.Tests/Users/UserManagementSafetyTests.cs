using System;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Areas.Admin.Controllers;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Models;
using MedAdvice.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedAdvice.Tests.Users
{
    /// The two safety rules exist because one careless click can leave the admin panel
    /// permanently unreachable: AdminSeed is empty in configuration, so the startup seeder
    /// provisions nothing and would not recreate an administrator.
    ///
    /// Each rule is asserted against the POST handler, not against the view, because hiding
    /// a button is not a guarantee.
    public class UserManagementSafetyTests : IDisposable
    {
        readonly TestDatabase database = new TestDatabase();
        readonly IdentityTestHost identity;

        public UserManagementSafetyTests()
        {
            identity = new IdentityTestHost(database);
        }

        UserController ControllerFor(ApplicationUser signedInAs)
        {
            UserController controller = new UserController(identity.Db, identity.Users, identity.Roles);
            TestContext.Prepare(controller, signedInAs);
            return controller;
        }

        static bool IsDeactivated(ApplicationUser user)
        {
            return user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow.AddYears(100);
        }

        [Fact]
        public async Task An_admin_cannot_deactivate_their_own_account()
        {
            ApplicationUser self = await identity.CreateUserAsync("admin@test.local", role: "admins");
            await identity.CreateUserAsync("other-admin@test.local", role: "admins");

            await ControllerFor(self).Deactivate(self.Id);

            ApplicationUser reloaded = await identity.Users.FindByIdAsync(self.Id);
            Assert.False(IsDeactivated(reloaded));
        }

        [Fact]
        public async Task An_admin_cannot_delete_their_own_account()
        {
            ApplicationUser self = await identity.CreateUserAsync("admin@test.local", role: "admins");
            await identity.CreateUserAsync("other-admin@test.local", role: "admins");

            await ControllerFor(self).DeleteConfirm(self.Id);

            Assert.NotNull(await identity.Users.FindByIdAsync(self.Id));
        }

        [Fact]
        public async Task The_last_admin_cannot_be_deactivated()
        {
            ApplicationUser onlyAdmin = await identity.CreateUserAsync("admin@test.local", role: "admins");
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            await identity.Users.RemoveFromRoleAsync(actor, "admins");

            await ControllerFor(actor).Deactivate(onlyAdmin.Id);

            ApplicationUser reloaded = await identity.Users.FindByIdAsync(onlyAdmin.Id);
            Assert.False(IsDeactivated(reloaded));
        }

        [Fact]
        public async Task The_last_admin_cannot_be_deleted()
        {
            ApplicationUser onlyAdmin = await identity.CreateUserAsync("admin@test.local", role: "admins");
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            await identity.Users.RemoveFromRoleAsync(actor, "admins");

            await ControllerFor(actor).DeleteConfirm(onlyAdmin.Id);

            Assert.NotNull(await identity.Users.FindByIdAsync(onlyAdmin.Id));
        }

        [Fact]
        public async Task An_admin_who_is_not_the_last_one_can_be_deactivated()
        {
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local", role: "admins");

            await ControllerFor(actor).Deactivate(target.Id);

            ApplicationUser reloaded = await identity.Users.FindByIdAsync(target.Id);
            Assert.True(IsDeactivated(reloaded));
        }

        [Fact]
        public async Task Deactivating_then_activating_restores_access()
        {
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local");

            await ControllerFor(actor).Deactivate(target.Id);
            await ControllerFor(actor).Activate(target.Id);

            ApplicationUser reloaded = await identity.Users.FindByIdAsync(target.Id);
            Assert.False(IsDeactivated(reloaded));
            Assert.False(await identity.Users.IsLockedOutAsync(reloaded));
        }

        [Fact]
        public async Task Deactivation_is_distinguishable_from_the_three_strikes_lockout()
        {
            // Both write LockoutEnd. A short lockout must not read as a deactivated account.
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser struck = await identity.CreateUserAsync("struck@test.local");

            await identity.Users.SetLockoutEnabledAsync(struck, true);
            await identity.Users.SetLockoutEndDateAsync(struck, DateTimeOffset.UtcNow.AddSeconds(80));

            ApplicationUser reloaded = await identity.Users.FindByIdAsync(struck.Id);
            Assert.True(await identity.Users.IsLockedOutAsync(reloaded));
            Assert.False(IsDeactivated(reloaded));
        }

        [Fact]
        public async Task Deleting_a_user_keeps_their_comments_as_guest_comments()
        {
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local");

            AdviceCategory category = new AdviceCategory { AdviceCategoryname = "c" };
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();

            Advice advice = new Advice { AdviceTitle = "a", AdviceCategoryId = category.Id };
            identity.Db.Add(advice);
            await identity.Db.SaveChangesAsync();

            identity.Db.Add(new AdviceComment
            {
                Userid = target.Id,
                AdviceId = advice.Id,
                comment = "kept",
                firstname = "Guest",
                lasttname = "Person",
                EmailAdress = "guest@test.local"
            });
            await identity.Db.SaveChangesAsync();

            await ControllerFor(actor).DeleteConfirm(target.Id);

            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            AdviceComment comment = await verify.adviceComments.SingleAsync();
            Assert.Null(comment.Userid);
            Assert.Equal("kept", comment.comment);
            Assert.Equal("guest@test.local", comment.EmailAdress);
        }

        [Fact]
        public async Task Deleting_a_user_removes_their_baskets()
        {
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local");

            identity.Db.Add(new Purchasecart
            {
                UserId = target.Id,
                isOpen = true,
                createdDate = DateTime.UtcNow
            });
            await identity.Db.SaveChangesAsync();

            await ControllerFor(actor).DeleteConfirm(target.Id);

            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            Assert.False(await verify.Purchasecarts.AnyAsync());
            Assert.Null(await identity.Users.FindByIdAsync(target.Id));
        }

        [Fact]
        public async Task A_user_with_history_can_actually_be_deleted()
        {
            // The database refuses a plain delete when comments or baskets reference the user,
            // because both foreign keys are ClientSetNull and the rows are not tracked. This
            // is the case the transaction exists for.
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local");

            AdviceCategory category = new AdviceCategory { AdviceCategoryname = "c" };
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();
            Advice advice = new Advice { AdviceTitle = "a", AdviceCategoryId = category.Id };
            identity.Db.Add(advice);
            await identity.Db.SaveChangesAsync();

            identity.Db.Add(new AdviceComment { Userid = target.Id, AdviceId = advice.Id, comment = "x" });
            identity.Db.Add(new Purchasecart { UserId = target.Id, isOpen = true, createdDate = DateTime.UtcNow });
            await identity.Db.SaveChangesAsync();

            await ControllerFor(actor).DeleteConfirm(target.Id);

            Assert.Null(await identity.Users.FindByIdAsync(target.Id));
        }

        [Fact]
        public async Task An_admin_cannot_strip_the_admin_role_from_themselves()
        {
            ApplicationUser self = await identity.CreateUserAsync("admin@test.local", role: "admins");
            await identity.CreateUserAsync("other-admin@test.local", role: "admins");

            UserController controller = ControllerFor(self);
            await controller.EditConfirm(new MedAdvice.viewmodel.UserEditViewModel
            {
                Id = self.Id,
                Email = self.Email,
                firstname = "a",
                lastname = "b",
                SelectedRoles = new string[0]
            });

            Assert.True(await identity.Users.IsInRoleAsync(
                await identity.Users.FindByIdAsync(self.Id), "admins"));
        }

        [Fact]
        public async Task The_last_admin_cannot_be_demoted()
        {
            ApplicationUser onlyAdmin = await identity.CreateUserAsync("admin@test.local", role: "admins");
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            await identity.Users.RemoveFromRoleAsync(actor, "admins");

            UserController controller = ControllerFor(actor);
            await controller.EditConfirm(new MedAdvice.viewmodel.UserEditViewModel
            {
                Id = onlyAdmin.Id,
                Email = onlyAdmin.Email,
                firstname = "a",
                lastname = "b",
                SelectedRoles = new string[0]
            });

            Assert.True(await identity.Users.IsInRoleAsync(
                await identity.Users.FindByIdAsync(onlyAdmin.Id), "admins"));
        }

        [Fact]
        public async Task A_weak_password_is_refused_by_the_admin_reset()
        {
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local");

            UserController controller = ControllerFor(actor);
            IActionResult result = await controller.ResetPasswordConfirm(
                new MedAdvice.viewmodel.AdminResetPasswordViewModel
                {
                    UserId = target.Id,
                    NewPassword = "abc",
                    ConfirmPassword = "abc"
                });

            Assert.IsType<ViewResult>(result);
            Assert.False(await identity.Users.CheckPasswordAsync(
                await identity.Users.FindByIdAsync(target.Id), "abc"));
        }

        [Fact]
        public async Task A_compliant_password_is_applied_by_the_admin_reset()
        {
            ApplicationUser actor = await identity.CreateUserAsync("actor@test.local", role: "admins");
            ApplicationUser target = await identity.CreateUserAsync("target@test.local");

            UserController controller = ControllerFor(actor);
            await controller.ResetPasswordConfirm(new MedAdvice.viewmodel.AdminResetPasswordViewModel
            {
                UserId = target.Id,
                NewPassword = "N3w!Password",
                ConfirmPassword = "N3w!Password"
            });

            Assert.True(await identity.Users.CheckPasswordAsync(
                await identity.Users.FindByIdAsync(target.Id), "N3w!Password"));
        }

        public void Dispose()
        {
            identity.Dispose();
            database.Dispose();
        }
    }
}
