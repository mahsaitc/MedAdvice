using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    /// Editable homepage copy. A single row; the admin panel edits it in place.
    public class HomepageContent
    {
        public int Id { get; set; }

        public string HeroTagline { get; set; }
        public string HeroTitle { get; set; }
        public string HeroText { get; set; }

        /// Null falls back to the theme image shipped in wwwroot, so the page renders
        /// unchanged until an admin uploads one.
        public byte[] HeroImage { get; set; }

        public bool AnnouncementVisible { get; set; }
        public string AnnouncementText { get; set; }
        public string AnnouncementCtaText { get; set; }
        public string AnnouncementCtaUrl { get; set; }
    }
}
