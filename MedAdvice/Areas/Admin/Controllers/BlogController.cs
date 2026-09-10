using AutoMapper;
using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.Services;
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
        public BlogController(MedAdviceDb _db)
        {
            db = _db;
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

            if (Model.BlogCategoryPicture != null)
            {
                if (Model.BlogCategoryPicture.Length <= 5 * Math.Pow(1024, 2))
                {
                    string extension = System.IO.Path.GetExtension(Model.BlogCategoryPicture.FileName.ToLower());
                    if (extension == ".jpeg" || extension == ".png" || extension == ".jpg")
                    {
                        byte[] b = new byte[Model.BlogCategoryPicture.Length];
                        Model.BlogCategoryPicture.OpenReadStream().Read(b, 0, b.Length);
                        blogcategory.BlogCategoryPicture = b;
                    }
                }
            }
            else
            {
                byte[] ax = System.IO.File.ReadAllBytes("advicetextheader.jpg");
                blogcategory.BlogCategoryPicture = ax;
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
        public IActionResult InsertBlogConfirm(BlogViewModel model, [FromServices] IMapper mapper)
        {
            Blog blog = mapper.Map<Blog>(model);
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
        public IActionResult InsertBlogImageConfirm(BlogImageViewModel model, [FromServices] IMapper mapper)
        {
            BlogImage blogImage = mapper.Map<BlogImage>(model);
            int blogid = HttpContext.Session.GetInt32("blogid").Value;
            blogImage.BlogId = blogid;
            db.Add(blogImage);
            db.SaveChanges();
            return RedirectToAction("InsertBlogImage", "Blog", new { blogid = blogid });
        }
    }

}
