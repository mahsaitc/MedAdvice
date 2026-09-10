using Ganss.Xss;

namespace MedAdvice.Services
{
    /// Strips scripts, event handlers and other active content from CKEditor HTML before it is
    /// persisted, so stored values stay safe for any view that renders them with @Html.Raw.
    public static class HtmlContentSanitizer
    {
        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;
            return new HtmlSanitizer().Sanitize(html);
        }
    }
}
