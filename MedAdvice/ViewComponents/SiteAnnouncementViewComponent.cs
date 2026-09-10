using System.Threading.Tasks;
using MedAdvice.Data;
using MedAdvice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAdvice.ViewComponents
{
    /// The announcement is site-wide, so it cannot live in the homepage view. A view
    /// component keeps the query in one place and lets the layout render it on every page.
    public class SiteAnnouncementViewComponent : ViewComponent
    {
        MedAdviceDb db;

        public SiteAnnouncementViewComponent(MedAdviceDb _db)
        {
            db = _db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            HomepageContent content = await db.HomepageContents.FirstOrDefaultAsync();
            if (content == null || content.AnnouncementVisible == false
                || string.IsNullOrWhiteSpace(content.AnnouncementText))
            {
                return Content(string.Empty);
            }

            return View(content);
        }
    }
}
