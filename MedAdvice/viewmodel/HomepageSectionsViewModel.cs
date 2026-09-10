using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedAdvice.viewmodel
{
    public class HomepageSectionRowViewModel
    {
        public int Id { get; set; }
        public string SectionKey { get; set; }
        public string DisplayName { get; set; }
        public bool IsVisible { get; set; }

        [Range(1, 99, ErrorMessage = "ترتیب باید عددی بین ۱ تا ۹۹ باشد")]
        public int SortOrder { get; set; }
    }

    public class HomepageSectionsViewModel
    {
        public List<HomepageSectionRowViewModel> Sections { get; set; } = new List<HomepageSectionRowViewModel>();
    }
}
