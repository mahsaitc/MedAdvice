using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MedAdvice.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MedAdvice.Tests.Security
{
    /// Whole-assembly guards rather than per-action assertions.
    ///
    /// These exist to fail when someone adds a new action later, which is the failure mode
    /// per-action tests never catch. They lock in the authorization and antiforgery work
    /// from phase one point five.
    public class ConventionTests
    {
        static readonly Assembly Application = typeof(AdminPanelController).Assembly;

        static IEnumerable<Type> Controllers()
        {
            return Application.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && t.IsAbstract == false);
        }

        static IEnumerable<MethodInfo> Actions(Type controller)
        {
            return controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.IsSpecialName == false)
                .Where(m => m.GetCustomAttribute<NonActionAttribute>() == null)
                .Where(m => typeof(IActionResult).IsAssignableFrom(m.ReturnType)
                         || typeof(Task<IActionResult>).IsAssignableFrom(m.ReturnType));
        }

        static bool HasVerb<T>(MethodInfo action) where T : Attribute
        {
            return action.GetCustomAttribute<T>() != null;
        }

        [Fact]
        public void Every_admin_controller_requires_the_admins_policy()
        {
            List<string> offenders = Controllers()
                .Where(t => t.Namespace != null && t.Namespace.Contains("Areas.Admin"))
                .Where(t =>
                {
                    AuthorizeAttribute authorize = t.GetCustomAttribute<AuthorizeAttribute>();
                    return authorize == null || authorize.Policy != "AdminsPolicy";
                })
                .Select(t => t.Name)
                .ToList();

            Assert.True(offenders.Count == 0,
                "admin controllers without [Authorize(Policy = \"AdminsPolicy\")]: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Every_admin_controller_declares_its_area()
        {
            List<string> offenders = Controllers()
                .Where(t => t.Namespace != null && t.Namespace.Contains("Areas.Admin"))
                .Where(t => t.GetCustomAttribute<AreaAttribute>() == null)
                .Select(t => t.Name)
                .ToList();

            Assert.True(offenders.Count == 0, "admin controllers without [Area]: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Every_post_action_validates_an_antiforgery_token()
        {
            List<string> offenders = new List<string>();
            foreach (Type controller in Controllers())
            {
                foreach (MethodInfo action in Actions(controller))
                {
                    if (HasVerb<HttpPostAttribute>(action)
                        && HasVerb<ValidateAntiForgeryTokenAttribute>(action) == false)
                    {
                        offenders.Add(controller.Name + "." + action.Name);
                    }
                }
            }

            Assert.True(offenders.Count == 0,
                "[HttpPost] actions without [ValidateAntiForgeryToken]: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Every_action_that_binds_a_view_model_is_post_with_antiforgery()
        {
            // The invariant phase one point five actually established. An action taking a view
            // model is a form handler, and a form handler reachable by GET is how credentials
            // ended up in the query string and every mutation was open to CSRF.
            //
            // Deliberately not "every action declares a verb": roughly forty read-only view
            // actions are still verbless, which is loose but harmless, since answering a POST
            // with a rendered page changes nothing.
            List<string> offenders = new List<string>();
            foreach (Type controller in Controllers())
            {
                foreach (MethodInfo action in Actions(controller))
                {
                    bool bindsViewModel = action.GetParameters().Any(p =>
                        p.ParameterType.Namespace != null
                        && p.ParameterType.Namespace.Equals("MedAdvice.viewmodel", StringComparison.Ordinal));

                    if (bindsViewModel == false)
                    {
                        continue;
                    }

                    if (HasVerb<HttpPostAttribute>(action) == false
                        || HasVerb<ValidateAntiForgeryTokenAttribute>(action) == false)
                    {
                        offenders.Add(controller.Name + "." + action.Name);
                    }
                }
            }

            Assert.True(offenders.Count == 0,
                "form handlers not protected by [HttpPost] + [ValidateAntiForgeryToken]: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Every_form_handler_checks_model_state()
        {
            // Validation attributes were decorative before phase three batch five, because
            // nothing ever consulted ModelState.
            string root = MarkupTests.RepositoryRoot();
            List<string> offenders = new List<string>();

            foreach (Type controller in Controllers())
            {
                string file = Directory
                    .EnumerateFiles(root, controller.Name + ".cs", SearchOption.AllDirectories)
                    .FirstOrDefault(f => f.Contains("bin") == false && f.Contains("obj") == false);

                if (file == null)
                {
                    continue;
                }

                bool bindsAnyViewModel = Actions(controller).Any(a => a.GetParameters().Any(p =>
                    p.ParameterType.Namespace != null
                    && p.ParameterType.Namespace.Equals("MedAdvice.viewmodel", StringComparison.Ordinal)));

                if (bindsAnyViewModel && File.ReadAllText(file).Contains("ModelState.IsValid") == false)
                {
                    offenders.Add(controller.Name);
                }
            }

            Assert.True(offenders.Count == 0,
                "controllers binding view models without any ModelState check: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Antiforgery_is_never_applied_to_a_get_action()
        {
            List<string> offenders = new List<string>();
            foreach (Type controller in Controllers())
            {
                foreach (MethodInfo action in Actions(controller))
                {
                    if (HasVerb<HttpGetAttribute>(action) && HasVerb<ValidateAntiForgeryTokenAttribute>(action))
                    {
                        offenders.Add(controller.Name + "." + action.Name);
                    }
                }
            }

            Assert.True(offenders.Count == 0,
                "[HttpGet] actions carrying [ValidateAntiForgeryToken]: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Customer_controller_denies_by_default()
        {
            Type customer = Application.GetTypes().Single(t => t.Name == "CustomerController");
            Assert.NotNull(customer.GetCustomAttribute<AuthorizeAttribute>());
        }

        [Theory]
        [InlineData("AddtoPurchaseCart")]
        [InlineData("ChangeCountPurchaseItem")]
        [InlineData("RemoveFromPurchaseCart")]
        [InlineData("PurchaseCartManagement")]
        public void Cart_actions_are_not_anonymous(string actionName)
        {
            Type customer = Application.GetTypes().Single(t => t.Name == "CustomerController");
            MethodInfo action = customer.GetMethods().Single(m => m.Name == actionName);

            Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
        }

        [Theory]
        [InlineData("Index")]
        [InlineData("ProductDetails")]
        [InlineData("Showproducts")]
        [InlineData("SearchProduct")]
        [InlineData("ProductCategoryone")]
        [InlineData("productCategoriesLevelTwo")]
        public void Browsing_stays_public(string actionName)
        {
            Type customer = Application.GetTypes().Single(t => t.Name == "CustomerController");
            MethodInfo action = customer.GetMethods().Single(m => m.Name == actionName);

            Assert.NotNull(action.GetCustomAttribute<AllowAnonymousAttribute>());
        }

        [Fact]
        public void Cart_mutations_are_post_only()
        {
            Type customer = Application.GetTypes().Single(t => t.Name == "CustomerController");
            foreach (string name in new[] { "AddtoPurchaseCart", "ChangeCountPurchaseItem", "RemoveFromPurchaseCart" })
            {
                MethodInfo action = customer.GetMethods().Single(m => m.Name == name);
                Assert.NotNull(action.GetCustomAttribute<HttpPostAttribute>());
                Assert.NotNull(action.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
            }
        }
    }
}
