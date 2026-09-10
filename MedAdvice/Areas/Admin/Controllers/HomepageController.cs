using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.Services;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminsPolicy")]
    public class HomepageController : Controller
    {
        MedAdviceDb db;

        public HomepageController(MedAdviceDb _db)
        {
            db = _db;
        }

        /// The table holds a single row. The seed migration creates it; this guards against
        /// an empty table so the panel never presents a blank form it cannot save.
        [NonAction]
        private async Task<HomepageContent> GetOrCreateContentAsync()
        {
            HomepageContent content = await db.HomepageContents.FirstOrDefaultAsync();
            if (content == null)
            {
                content = new HomepageContent();
                db.HomepageContents.Add(content);
                await db.SaveChangesAsync();
            }
            return content;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            HomepageContent content = await GetOrCreateContentAsync();

            return View(new HomepageContentViewModel
            {
                Id = content.Id,
                HeroTagline = content.HeroTagline,
                HeroTitle = content.HeroTitle,
                HeroText = content.HeroText,
                HasHeroImage = content.HeroImage != null && content.HeroImage.Length > 0,
                AnnouncementVisible = content.AnnouncementVisible,
                AnnouncementText = content.AnnouncementText,
                AnnouncementCtaText = content.AnnouncementCtaText,
                AnnouncementCtaUrl = content.AnnouncementCtaUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexConfirm(HomepageContentViewModel model)
        {
            HomepageContent content = await GetOrCreateContentAsync();
            model.HasHeroImage = content.HeroImage != null && content.HeroImage.Length > 0;

            if (ModelState.IsValid == false)
            {
                return View("Index", model);
            }

            content.HeroTagline = model.HeroTagline;
            content.HeroTitle = model.HeroTitle;
            content.HeroText = model.HeroText;
            content.AnnouncementVisible = model.AnnouncementVisible;
            content.AnnouncementText = model.AnnouncementText;
            content.AnnouncementCtaText = model.AnnouncementCtaText;
            content.AnnouncementCtaUrl = model.AnnouncementCtaUrl;

            // Leaving the file empty keeps the current image rather than clearing it.
            if (model.HeroImage != null)
            {
                ImageReadResult image = await ImageUpload.ReadAsync(model.HeroImage);
                if (image.Ok == false)
                {
                    ModelState.AddModelError(string.Empty, image.Error);
                    return View("Index", model);
                }
                content.HeroImage = image.Content;
            }

            if (model.RemoveHeroImage)
            {
                content.HeroImage = null;
            }

            await db.SaveChangesAsync();
            TempData["msg"] = "محتوای صفحه اصلی ذخیره شد.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Sections()
        {
            List<HomepageSection> sections = await db.HomepageSections
                .OrderBy(x => x.SortOrder).ToListAsync();

            return View(new HomepageSectionsViewModel
            {
                Sections = sections.Select(x => new HomepageSectionRowViewModel
                {
                    Id = x.Id,
                    SectionKey = x.SectionKey,
                    DisplayName = x.DisplayName,
                    IsVisible = x.IsVisible,
                    SortOrder = x.SortOrder
                }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionsConfirm(HomepageSectionsViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View("Sections", model);
            }

            List<HomepageSection> sections = await db.HomepageSections.ToListAsync();
            foreach (HomepageSectionRowViewModel row in model.Sections ?? new List<HomepageSectionRowViewModel>())
            {
                HomepageSection section = sections.FirstOrDefault(x => x.Id == row.Id);
                if (section == null)
                {
                    continue;
                }
                section.IsVisible = row.IsVisible;
                section.SortOrder = row.SortOrder;
            }

            await db.SaveChangesAsync();
            TempData["msg"] = "ترتیب و نمایش بخش‌ها ذخیره شد.";
            return RedirectToAction("Sections");
        }

        [HttpGet]
        public async Task<IActionResult> Featured()
        {
            ViewData["blogs"] = await db.Blogs.OrderByDescending(x => x.Id).Take(100).ToListAsync();
            ViewData["advices"] = await db.Advices.OrderByDescending(x => x.Id).Take(100).ToListAsync();

            List<FeaturedArticle> pinned = await db.FeaturedArticles
                .Include(x => x.Blog)
                .Include(x => x.Advice)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return View(pinned);
        }

        [NonAction]
        private async Task<int> NextSortOrderAsync()
        {
            if (await db.FeaturedArticles.AnyAsync() == false)
            {
                return 1;
            }
            return await db.FeaturedArticles.MaxAsync(x => x.SortOrder) + 1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PinBlog(int blogId)
        {
            Blog blog = await db.Blogs.FirstOrDefaultAsync(x => x.Id == blogId);
            if (blog == null)
            {
                TempData["msg"] = "مطلب مورد نظر پیدا نشد.";
                return RedirectToAction("Featured");
            }

            if (await db.FeaturedArticles.AnyAsync(x => x.BlogId == blogId))
            {
                TempData["msg"] = "این مطلب قبلا اضافه شده است.";
                return RedirectToAction("Featured");
            }

            db.FeaturedArticles.Add(new FeaturedArticle
            {
                BlogId = blogId,
                SortOrder = await NextSortOrderAsync()
            });
            await db.SaveChangesAsync();

            TempData["msg"] = "مطلب به بخش منتخب اضافه شد.";
            return RedirectToAction("Featured");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PinAdvice(int adviceId)
        {
            Advice advice = await db.Advices.FirstOrDefaultAsync(x => x.Id == adviceId);
            if (advice == null)
            {
                TempData["msg"] = "توصیه مورد نظر پیدا نشد.";
                return RedirectToAction("Featured");
            }

            if (await db.FeaturedArticles.AnyAsync(x => x.AdviceId == adviceId))
            {
                TempData["msg"] = "این توصیه قبلا اضافه شده است.";
                return RedirectToAction("Featured");
            }

            db.FeaturedArticles.Add(new FeaturedArticle
            {
                AdviceId = adviceId,
                SortOrder = await NextSortOrderAsync()
            });
            await db.SaveChangesAsync();

            TempData["msg"] = "توصیه به بخش منتخب اضافه شد.";
            return RedirectToAction("Featured");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpin(int id)
        {
            FeaturedArticle article = await db.FeaturedArticles.FirstOrDefaultAsync(x => x.Id == id);
            if (article == null)
            {
                TempData["msg"] = "مورد انتخابی پیدا نشد.";
                return RedirectToAction("Featured");
            }

            db.FeaturedArticles.Remove(article);
            await db.SaveChangesAsync();

            TempData["msg"] = "مورد از بخش منتخب حذف شد.";
            return RedirectToAction("Featured");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            return await SwapAsync(id, moveUp: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            return await SwapAsync(id, moveUp: false);
        }

        /// Swaps SortOrder with the adjacent pin, so ordering stays stable even if the
        /// stored values are not contiguous.
        [NonAction]
        private async Task<IActionResult> SwapAsync(int id, bool moveUp)
        {
            List<FeaturedArticle> ordered = await db.FeaturedArticles
                .OrderBy(x => x.SortOrder).ToListAsync();

            int index = ordered.FindIndex(x => x.Id == id);
            if (index < 0)
            {
                TempData["msg"] = "مورد انتخابی پیدا نشد.";
                return RedirectToAction("Featured");
            }

            int neighbour = moveUp ? index - 1 : index + 1;
            if (neighbour < 0 || neighbour >= ordered.Count)
            {
                return RedirectToAction("Featured");
            }

            int current = ordered[index].SortOrder;
            ordered[index].SortOrder = ordered[neighbour].SortOrder;
            ordered[neighbour].SortOrder = current;

            await db.SaveChangesAsync();
            return RedirectToAction("Featured");
        }
    }
}
