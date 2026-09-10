using System.Collections.Generic;
using MedAdvice.Models;

namespace MedAdvice.viewmodel
{
    public class FeaturedArticleViewModel
    {
        public string Title { get; set; }
        public string BriefText { get; set; }
        public byte[] Image { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int RouteId { get; set; }
    }

    public class HomepageViewModel
    {
        /// Never null: the controller substitutes an empty instance if the row is missing,
        /// so every partial can bind without a null check.
        public HomepageContent Content { get; set; } = new HomepageContent();
        public List<HomepageSection> Sections { get; set; } = new List<HomepageSection>();
        public List<FeaturedArticleViewModel> Featured { get; set; } = new List<FeaturedArticleViewModel>();
    }
}
