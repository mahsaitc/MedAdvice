using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminsPolicy")]
    public class UserController : Controller
    {
        const int PageSize = 20;
        const string AdminRole = "admins";

        /// Deactivation reuses Identity's lockout column. The sentinel sits far enough in the
        /// future that it cannot be confused with the three-strikes lockout, which is
        /// configured for eighty seconds and writes the same column.
        static readonly DateTimeOffset DeactivatedUntil = DateTimeOffset.MaxValue;

        MedAdviceDb db;
        UserManager<ApplicationUser> userManager;
        RoleManager<IdentityRole> roleManager;

        public UserController(MedAdviceDb _db, UserManager<ApplicationUser> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            db = _db;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        [NonAction]
        static bool IsDeactivated(ApplicationUser user)
        {
            return user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow.AddYears(100);
        }

        [NonAction]
        static bool IsTemporarilyLockedOut(ApplicationUser user)
        {
            return user.LockoutEnd != null
                && user.LockoutEnd > DateTimeOffset.UtcNow
                && user.LockoutEnd <= DateTimeOffset.UtcNow.AddYears(100);
        }

        /// An admin must not be able to lock themselves out of the panel.
        [NonAction]
        private async Task<bool> IsSelfAsync(string userId)
        {
            ApplicationUser current = await userManager.GetUserAsync(User);
            return current != null && current.Id == userId;
        }

        /// Removing the last admin would leave the panel unreachable, and the startup seeder
        /// only provisions an account when AdminSeed is configured.
        [NonAction]
        private async Task<bool> IsLastAdminAsync(string userId)
        {
            IList<ApplicationUser> admins = await userManager.GetUsersInRoleAsync(AdminRole);
            return admins.Count <= 1 && admins.Any(x => x.Id == userId);
        }

        [NonAction]
        private static string DescribeErrors(IdentityResult result)
        {
            return string.Join(" ", result.Errors.Select(x => x.Description));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string q, string role, string status, int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            IQueryable<ApplicationUser> query = userManager.Users;

            if (string.IsNullOrWhiteSpace(q) == false)
            {
                query = query.Where(x => x.UserName.Contains(q)
                    || x.Email.Contains(q)
                    || x.firstname.Contains(q)
                    || x.lastname.Contains(q));
            }

            if (string.IsNullOrWhiteSpace(role) == false)
            {
                List<string> roleUserIds = (await userManager.GetUsersInRoleAsync(role))
                    .Select(x => x.Id).ToList();
                query = query.Where(x => roleUserIds.Contains(x.Id));
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset farFuture = now.AddYears(100);
            if (status == "deactivated")
            {
                query = query.Where(x => x.LockoutEnd != null && x.LockoutEnd > farFuture);
            }
            else if (status == "lockedout")
            {
                query = query.Where(x => x.LockoutEnd != null && x.LockoutEnd > now && x.LockoutEnd <= farFuture);
            }
            else if (status == "active")
            {
                query = query.Where(x => x.LockoutEnd == null || x.LockoutEnd <= now);
            }

            int total = await query.CountAsync();
            List<ApplicationUser> users = await query
                .OrderBy(x => x.UserName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // One join, rather than a GetRolesAsync round trip per row.
            List<string> pageIds = users.Select(x => x.Id).ToList();
            var roleMap = await (from userRole in db.UserRoles
                                 join identityRole in db.Roles on userRole.RoleId equals identityRole.Id
                                 where pageIds.Contains(userRole.UserId)
                                 select new { userRole.UserId, RoleName = identityRole.Name })
                                .ToListAsync();

            UserListViewModel model = new UserListViewModel
            {
                Query = q,
                Role = role,
                Status = status,
                Page = page,
                PageSize = PageSize,
                TotalCount = total,
                TotalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)PageSize),
                AvailableRoles = await roleManager.Roles.Select(x => x.Name).OrderBy(x => x).ToListAsync(),
                Users = users.Select(x => new UserListItemViewModel
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    FullName = (x.firstname + " " + x.lastname).Trim(),
                    PhoneNumber = x.PhoneNumber,
                    Roles = roleMap.Where(r => r.UserId == x.Id).Select(r => r.RoleName).ToList(),
                    IsDeactivated = IsDeactivated(x),
                    IsTemporarilyLockedOut = IsTemporarilyLockedOut(x)
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            UserDetailsViewModel model = new UserDetailsViewModel
            {
                User = user,
                Roles = (await userManager.GetRolesAsync(user)).ToList(),
                IsDeactivated = IsDeactivated(user),
                IsTemporarilyLockedOut = IsTemporarilyLockedOut(user),
                CartCount = await db.Purchasecarts.CountAsync(x => x.UserId == id),
                CommentCount = await db.adviceComments.CountAsync(x => x.Userid == id),
                RecentCarts = await db.Purchasecarts
                    .Where(x => x.UserId == id)
                    .Include(x => x.PurchaseCartItems)
                    .OrderByDescending(x => x.Id)
                    .Take(10)
                    .ToListAsync(),
                RecentComments = await db.adviceComments
                    .Where(x => x.Userid == id)
                    .Include(x => x.Advice)
                    .OrderByDescending(x => x.id)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            UserEditViewModel model = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                firstname = user.firstname,
                lastname = user.lastname,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                SelectedRoles = (await userManager.GetRolesAsync(user)).ToArray(),
                AllRoles = await roleManager.Roles.Select(x => x.Name).OrderBy(x => x).ToListAsync(),
                IsSelf = await IsSelfAsync(user.Id),
                IsLastAdmin = await IsLastAdminAsync(user.Id)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirm(UserEditViewModel model)
        {
            ApplicationUser user = model.Id == null ? null : await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            model.UserName = user.UserName;
            model.AllRoles = await roleManager.Roles.Select(x => x.Name).OrderBy(x => x).ToListAsync();
            model.IsSelf = await IsSelfAsync(user.Id);
            model.IsLastAdmin = await IsLastAdminAsync(user.Id);

            if (ModelState.IsValid == false)
            {
                return View("Edit", model);
            }

            string[] selected = model.SelectedRoles ?? new string[0];
            IList<string> current = await userManager.GetRolesAsync(user);

            // Refused in the handler, not merely hidden in the view. An admin removing their
            // own admin role, or the last admin losing it, makes the panel unreachable.
            bool losingAdmin = current.Contains(AdminRole) && selected.Contains(AdminRole) == false;
            if (losingAdmin && (model.IsSelf || model.IsLastAdmin))
            {
                ModelState.AddModelError(string.Empty, model.IsSelf
                    ? "نمی‌توانید نقش مدیر را از حساب خودتان بردارید."
                    : "این تنها حساب مدیر است و نقش مدیر آن قابل حذف نیست.");
                return View("Edit", model);
            }

            user.Email = model.Email;
            user.firstname = model.firstname;
            user.lastname = model.lastname;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;

            IdentityResult updateResult = await userManager.UpdateAsync(user);
            if (updateResult.Succeeded == false)
            {
                ModelState.AddModelError(string.Empty, DescribeErrors(updateResult));
                return View("Edit", model);
            }

            List<string> toAdd = selected.Except(current).ToList();
            List<string> toRemove = current.Except(selected).ToList();

            if (toRemove.Count > 0)
            {
                IdentityResult removeResult = await userManager.RemoveFromRolesAsync(user, toRemove);
                if (removeResult.Succeeded == false)
                {
                    ModelState.AddModelError(string.Empty, DescribeErrors(removeResult));
                    return View("Edit", model);
                }
            }

            if (toAdd.Count > 0)
            {
                IdentityResult addResult = await userManager.AddToRolesAsync(user, toAdd);
                if (addResult.Succeeded == false)
                {
                    ModelState.AddModelError(string.Empty, DescribeErrors(addResult));
                    return View("Edit", model);
                }
            }

            TempData["msg"] = "اطلاعات کاربر با موفقیت ذخیره شد.";
            return RedirectToAction("Details", new { id = user.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            if (await IsSelfAsync(id))
            {
                TempData["msg"] = "نمی‌توانید حساب خودتان را غیرفعال کنید.";
                return RedirectToAction("Details", new { id = id });
            }

            if (await IsLastAdminAsync(id))
            {
                TempData["msg"] = "این تنها حساب مدیر است و قابل غیرفعال کردن نیست.";
                return RedirectToAction("Details", new { id = id });
            }

            await userManager.SetLockoutEnabledAsync(user, true);
            IdentityResult result = await userManager.SetLockoutEndDateAsync(user, DeactivatedUntil);
            TempData["msg"] = result.Succeeded ? "حساب کاربر غیرفعال شد." : DescribeErrors(result);

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            IdentityResult result = await userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
            {
                await userManager.ResetAccessFailedCountAsync(user);
            }

            TempData["msg"] = result.Succeeded ? "حساب کاربر فعال شد." : DescribeErrors(result);

            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            return View(new AdminResetPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirm(AdminResetPasswordViewModel model)
        {
            ApplicationUser user = model.UserId == null ? null : await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            model.UserName = user.UserName;

            if (ModelState.IsValid == false)
            {
                return View("ResetPassword", model);
            }

            // Going through Identity's own token flow means the configured password policy is
            // enforced here exactly as it is on the public reset page.
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (result.Succeeded == false)
            {
                ModelState.AddModelError(string.Empty, DescribeErrors(result));
                return View("ResetPassword", model);
            }

            TempData["msg"] = "رمز عبور کاربر با موفقیت تغییر کرد.";
            return RedirectToAction("Details", new { id = user.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            UserDetailsViewModel model = new UserDetailsViewModel
            {
                User = user,
                Roles = (await userManager.GetRolesAsync(user)).ToList(),
                IsDeactivated = IsDeactivated(user),
                IsTemporarilyLockedOut = IsTemporarilyLockedOut(user),
                CartCount = await db.Purchasecarts.CountAsync(x => x.UserId == id),
                CommentCount = await db.adviceComments.CountAsync(x => x.Userid == id)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(string id)
        {
            ApplicationUser user = id == null ? null : await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["msg"] = "کاربر مورد نظر پیدا نشد.";
                return RedirectToAction("Index");
            }

            if (await IsSelfAsync(id))
            {
                TempData["msg"] = "نمی‌توانید حساب خودتان را حذف کنید.";
                return RedirectToAction("Details", new { id = id });
            }

            if (await IsLastAdminAsync(id))
            {
                TempData["msg"] = "این تنها حساب مدیر است و قابل حذف نیست.";
                return RedirectToAction("Details", new { id = id });
            }

            // Both user-referencing foreign keys are optional with ClientSetNull, which only
            // acts on tracked entities, so the database itself refuses the delete. Comments
            // are detached and survive as guest comments; baskets are removed outright.
            // Identity shares this context, so the whole thing is one transaction.
            using (IDbContextTransaction transaction = await db.Database.BeginTransactionAsync())
            {
                List<AdviceComment> comments = await db.adviceComments
                    .Where(x => x.Userid == id).ToListAsync();
                foreach (AdviceComment comment in comments)
                {
                    comment.Userid = null;
                }

                List<Purchasecart> carts = await db.Purchasecarts
                    .Where(x => x.UserId == id).ToListAsync();
                db.Purchasecarts.RemoveRange(carts);

                await db.SaveChangesAsync();

                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded == false)
                {
                    await transaction.RollbackAsync();
                    TempData["msg"] = DescribeErrors(result);
                    return RedirectToAction("Details", new { id = id });
                }

                await transaction.CommitAsync();
            }

            TempData["msg"] = "کاربر حذف شد.";
            return RedirectToAction("Index");
        }
    }
}
