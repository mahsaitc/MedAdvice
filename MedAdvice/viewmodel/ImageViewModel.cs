namespace MedAdvice.viewmodel
{
    /// What the shared _Image partial needs: where the stored file is, what to show when
    /// there is none, and the alt text.
    public class ImageViewModel
    {
        public ImageViewModel(string path, string fallback, string alt = "Image", string cssClass = null, int? width = null)
        {
            Path = path;
            Fallback = fallback;
            Alt = alt;
            CssClass = cssClass;
            Width = width;
        }

        public string Path { get; }
        public string Fallback { get; }
        public string Alt { get; }
        public string CssClass { get; }
        public int? Width { get; }

        /// A missing image is a normal state now that paths are nullable, where a missing
        /// byte array used to throw inside Convert.ToBase64String.
        public string Source
        {
            get { return string.IsNullOrWhiteSpace(Path) ? Fallback : Path; }
        }
    }

    /// The stock images shown when nothing has been uploaded.
    public static class ImageFallbacks
    {
        public const string Advice = "/img/defaults/advice.jpg";
        public const string Blog = "/img/defaults/advice.jpg";
        public const string Doctor = "/img/defaults/doctor.png";
        public const string Product = "/img/defaults/advice.jpg";
        public const string Banner = "/layout/img/banner/banner-1.jpg";
    }
}
