using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedAdvice.viewmodel
{
    public class HomepageContentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "لطفا عنوان اصلی را وارد کنید")]
        public string HeroTitle { get; set; }

        public string HeroTagline { get; set; }
        public string HeroText { get; set; }

        /// Leaving this empty keeps the stored image.
        public IFormFile HeroImage { get; set; }
        public bool HasHeroImage { get; set; }
        public bool RemoveHeroImage { get; set; }

        public bool AnnouncementVisible { get; set; }
        public string AnnouncementText { get; set; }
        public string AnnouncementCtaText { get; set; }

        [Url(ErrorMessage = "نشانی وارد شده معتبر نیست")]
        public string AnnouncementCtaUrl { get; set; }
    }
}
