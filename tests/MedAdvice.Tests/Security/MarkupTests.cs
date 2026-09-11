using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace MedAdvice.Tests.Security
{
    /// The "forms must POST" fix from phase one point five lives in Razor markup, so the only
    /// way to guard it is to read the view files. Reflection cannot see a missing method
    /// attribute on a form tag.
    public class MarkupTests
    {
        /// Forms that legitimately submit over GET because they only read: site searches and
        /// two navigation forms. Anything else carrying asp-action must declare method="post".
        static readonly HashSet<string> ReadOnlyActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SearchAdvice",
            "SearchBlog",
            "SearchDoctor",
            "SearchProduct",
            "ViewAdviceDetails",
            "productCategoriesLevelTwo"
        };

        /// The admin user filter posts over GET on purpose so its filters live in the query
        /// string and stay linkable. Scoped to the exact file so a generic Index elsewhere is
        /// still caught.
        static readonly HashSet<string> ReadOnlyFormsByPath = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Areas/Admin/Views/User/Index.cshtml|Index"
        };

        internal static string RepositoryRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && directory.GetFiles("MedAdvice.sln").Length == 0)
            {
                directory = directory.Parent;
            }

            Assert.True(directory != null, "could not locate the repository root from " + AppContext.BaseDirectory);
            return directory.FullName;
        }

        static string ProjectRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && directory.GetFiles("MedAdvice.sln").Length == 0)
            {
                directory = directory.Parent;
            }

            Assert.True(directory != null, "could not locate the repository root from " + AppContext.BaseDirectory);
            return Path.Combine(directory.FullName, "MedAdvice");
        }

        static IEnumerable<(string Path, string Action, string Method)> FormsWithActions()
        {
            string root = ProjectRoot();
            foreach (string file in Directory.EnumerateFiles(root, "*.cshtml", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
                if (relative.StartsWith("bin/") || relative.StartsWith("obj/") || relative.StartsWith("wwwroot/"))
                {
                    continue;
                }

                string content = File.ReadAllText(file);
                foreach (Match match in Regex.Matches(content, @"<form\b[^>]*>", RegexOptions.Singleline))
                {
                    string tag = match.Value;
                    Match action = Regex.Match(tag, @"asp-action=""([^""]+)""");
                    if (action.Success == false)
                    {
                        continue;
                    }

                    Match method = Regex.Match(tag, @"method=""([^""]+)""", RegexOptions.IgnoreCase);
                    yield return (relative, action.Groups[1].Value, method.Success ? method.Groups[1].Value.ToLowerInvariant() : "none");
                }
            }
        }

        [Fact]
        public void Every_state_changing_form_posts()
        {
            List<string> offenders = FormsWithActions()
                .Where(f => f.Method != "post")
                .Where(f => ReadOnlyActions.Contains(f.Action) == false)
                .Where(f => ReadOnlyFormsByPath.Contains(f.Path + "|" + f.Action) == false)
                .Select(f => $"{f.Path} -> {f.Action} (method={f.Method})")
                .ToList();

            Assert.True(offenders.Count == 0,
                "forms that should submit over POST but do not:\n  " + string.Join("\n  ", offenders));
        }

        [Fact]
        public void The_sign_in_and_sign_up_forms_post()
        {
            // The specific defect that started phase one point five: credentials in the query
            // string because these forms fell back to the HTML default of GET.
            List<(string Path, string Action, string Method)> credentialForms = FormsWithActions()
                .Where(f => f.Action.Equals("signinconfirm", StringComparison.OrdinalIgnoreCase)
                         || f.Action.Equals("signupconfirm", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(credentialForms);
            Assert.All(credentialForms, form => Assert.Equal("post", form.Method));
        }

        [Fact]
        public void Every_form_that_binds_a_model_shows_a_validation_summary()
        {
            // Adding ModelState checks without somewhere to render the errors would swap a
            // silent failure for a differently silent one.
            string root = ProjectRoot();
            List<string> offenders = new List<string>();

            foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "Areas/Admin/Views"), "*.cshtml", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(file);
                bool posts = Regex.IsMatch(content, @"<form\b[^>]*method=""post""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                bool bindsModel = Regex.IsMatch(content, @"asp-for=""");
                if (posts && bindsModel && content.Contains("asp-validation-summary") == false)
                {
                    offenders.Add(Path.GetRelativePath(root, file).Replace('\\', '/'));
                }
            }

            Assert.True(offenders.Count == 0,
                "admin forms binding a model with no validation summary:\n  " + string.Join("\n  ", offenders));
        }

        [Fact]
        public void No_view_renders_unsanitised_rich_text()
        {
            // The three rich text fields must pass through the sanitiser on the way out, as
            // well as on the way in.
            string root = ProjectRoot();
            List<string> offenders = new List<string>();

            foreach (string file in Directory.EnumerateFiles(root, "*.cshtml", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
                if (relative.StartsWith("bin/") || relative.StartsWith("obj/"))
                {
                    continue;
                }

                foreach (Match match in Regex.Matches(File.ReadAllText(file), @"@Html\.Raw\(([^)]*)\)"))
                {
                    string argument = match.Groups[1].Value;
                    if (argument.Contains("HtmlContentSanitizer.Sanitize") == false)
                    {
                        offenders.Add($"{relative}: @Html.Raw({argument})");
                    }
                }
            }

            Assert.True(offenders.Count == 0,
                "Html.Raw calls that skip the sanitiser:\n  " + string.Join("\n  ", offenders));
        }
    }
}
