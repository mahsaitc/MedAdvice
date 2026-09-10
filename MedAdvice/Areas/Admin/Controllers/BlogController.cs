using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.Services;
using MedAdvice.mapper;
using Microsoft.AspNetCore.Hosting;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class BlogController : Controller
    {
        MedAdviceDb db;
        IWebHostEnvironment env;
        public BlogController(MedAdviceDb _db, IWebHostEnvironment _env)
        {
            db = _db;
            env = _env;
        }
        [HttpGet]
        public IActionResult insertblogCategory()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> insertblogCategory(BlogCategoryViewModel Model)
        {
            BlogCategory blogcategory = new BlogCategory
            {
                BlogCategoryname = Model.BlogCategoryname,
                BlogCategoryParentId = Model.BlogCategoryParentId

            };

            if (Model.BlogCategoryPicture == null)
            {
                blogcategory.BlogCategoryPicture = await ImageUpload.ReadDefaultAsync(env, "advicetextheader.jpg");
            }
            else
            {
                ImageReadResult picture = await ImageUpload.ReadAsync(Model.BlogCategoryPicture);
                if (picture.Ok == false)
                {
                    TempData["msg"] = picture.Error;
                    return View();
                }
                blogcategory.BlogCategoryPicture = picture.Content;
            }
            db.Add(blogcategory);
            await db.SaveChangesAsync();
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> InsertBlogDetail()
        {
            ViewData["BlogCategories"] = await db.blogCategories.Where(x => x.BlogCategoryParentId == null).ToListAsync();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetBlogCategories(int BlogId)
        {
            List<BlogCategory> blogCategories = await db.blogCategories.Where(x => x.BlogCategoryParentId == BlogId).ToListAsync();
            return Json(blogCategories);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertBlogConfirm(BlogViewModel model)
        {
            ImageReadResult headerResult = await ImageUpload.ReadAsync(model.BlogHeaderImage);
            if (headerResult.Ok == false)
            {
                TempData["msg"] = headerResult.Error;
                return RedirectToAction("InsertBlogDetail", "Blog");
            }
            Blog blog = model.ToEntity();
            blog.BlogHeaderImage = headerResult.Content;
            blog.BlogText = HtmlContentSanitizer.Sanitize(blog.BlogText);
            db.Add(blog);
            await db.SaveChangesAsync();
            return RedirectToAction("InsertBlogImage", "blog", new { BlogId = blog.Id });
        }
        [HttpGet]
        public async Task<IActionResult> InsertBlogImage(int blogid)
        {
            Blog blog = await db.Blogs.FirstOrDefaultAsync(x => x.Id == blogid);

            HttpContext.Session.SetInt32("blogid" , blogid);

            ViewData["blog"] = blog;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertBlogImageConfirm(BlogImageViewModel model)
        {
            int blogid = HttpContext.Session.GetInt32("blogid").Value;
            ImageReadResult imageResult = await ImageUpload.ReadAsync(model.Blogimg);
            if (imageResult.Ok == false)
            {
                TempData["msg"] = imageResult.Error;
                return RedirectToAction("InsertBlogImage", "Blog", new { blogid = blogid });
            }
            BlogImage blogImage = model.ToEntity();
            blogImage.Blogimg = imageResult.Content;
            blogImage.BlogId = blogid;
            db.Add(blogImage);
            await db.SaveChangesAsync();
            return RedirectToAction("InsertBlogImage", "Blog", new { blogid = blogid });
        }
    }

}
