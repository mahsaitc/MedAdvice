using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    /// One block of the homepage. SectionKey matches a partial in Views/Home, so the page
    /// renders whatever rows are visible, in SortOrder.
    public class HomepageSection
    {
        public int Id { get; set; }
        public string SectionKey { get; set; }
        public string DisplayName { get; set; }
        public bool IsVisible { get; set; }
        public int SortOrder { get; set; }
    }
}
