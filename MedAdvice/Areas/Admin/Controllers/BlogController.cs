using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.Services;
using MedAdvice.mapper;
using Microsoft.AspNetCore.Hosting;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult insertblogCategory(BlogCategoryViewModel Model)
        {
            BlogCategory blogcategory = new BlogCategory
            {
                BlogCategoryname = Model.BlogCategoryname,
                BlogCategoryParentId = Model.BlogCategoryParentId

            };

            if (Model.BlogCategoryPicture == null)
            {
                blogcategory.BlogCategoryPicture = ImageUpload.ReadDefault(env, "advicetextheader.jpg");
            }
            else if (ImageUpload.TryRead(Model.BlogCategoryPicture, out byte[] picture, out string pictureError))
            {
                blogcategory.BlogCategoryPicture = picture;
            }
            else
            {
                TempData["msg"] = pictureError;
                return View();
            }
            db.Add(blogcategory);
            db.SaveChanges();
            return View();
        }
        
        [HttpGet]
        public IActionResult InsertBlogDetail()
        {
            ViewData["BlogCategories"] = db.blogCategories.Where(x => x.BlogCategoryParentId == null).ToList();
            return View();
        }
        [HttpGet]
        public IActionResult GetBlogCategories(int BlogId)
        {
            List<BlogCategory> blogCategories = db.blogCategories.Where(x => x.BlogCategoryParentId == BlogId).ToList();
            return Json(blogCategories);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertBlogConfirm(BlogViewModel model)
        {
            if (ImageUpload.TryRead(model.BlogHeaderImage, out byte[] header, out string headerError) == false)
            {
                TempData["msg"] = headerError;
                return RedirectToAction("InsertBlogDetail", "Blog");
            }
            Blog blog = model.ToEntity();
            blog.BlogHeaderImage = header;
            blog.BlogText = HtmlContentSanitizer.Sanitize(blog.BlogText);
            db.Add(blog);
            db.SaveChanges();
            return RedirectToAction("InsertBlogImage", "blog", new { BlogId = blog.Id });
        }
        [HttpGet]
        public IActionResult InsertBlogImage(int blogid)
        {
            Blog blog = db.Blogs.FirstOrDefault(x => x.Id == blogid);

            HttpContext.Session.SetInt32("blogid" , blogid);

            ViewData["blog"] = blog;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertBlogImageConfirm(BlogImageViewModel model)
        {
            int blogid = HttpContext.Session.GetInt32("blogid").Value;
            if (ImageUpload.TryRead(model.Blogimg, out byte[] image, out string imageError) == false)
            {
                TempData["msg"] = imageError;
                return RedirectToAction("InsertBlogImage", "Blog", new { blogid = blogid });
            }
            BlogImage blogImage = model.ToEntity();
            blogImage.Blogimg = image;
            blogImage.BlogId = blogid;
            db.Add(blogImage);
            db.SaveChanges();
            return RedirectToAction("InsertBlogImage", "Blog", new { blogid = blogid });
        }
    }

}
