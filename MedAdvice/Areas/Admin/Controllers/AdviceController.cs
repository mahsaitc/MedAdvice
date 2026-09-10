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
    public class AdviceController : Controller
    {
        MedAdviceDb db;
        IWebHostEnvironment env;
        public AdviceController(MedAdviceDb _db, IWebHostEnvironment _env)
        {
            db = _db;
            env = _env;
        }
        [HttpGet]
        public IActionResult insertAdviceCategory()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult insertAdviceCategory(AdviceCategoryViewModel Model)
        {
            AdviceCategory adviceCategory = new AdviceCategory { 
                AdviceCategoryname = Model.AdviceCategoryname,
                AdviceCategoryParentId = Model.AdviceCategoryParentId

            };

            if (Model.AdviceCategoryPicture == null)
            {
                adviceCategory.AdviceCategoryPicture = ImageUpload.ReadDefault(env, "advicetextheader.jpg");
            }
            else if (ImageUpload.TryRead(Model.AdviceCategoryPicture, out byte[] picture, out string pictureError))
            {
                adviceCategory.AdviceCategoryPicture = picture;
            }
            else
            {
                TempData["msg"] = pictureError;
                return View();
            }
            db.Add(adviceCategory);
            db.SaveChanges();
            return View();
        }
        [HttpGet]
        public IActionResult InsertAdvice()
        {
            ViewData["AdviceCategory"] = db.adviceCategories.Where(x => x.AdviceCategoryParentId == null).ToList();
           
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertAdviceConfirm(AdviceViewmodel model)
        {
            Advice advice = new Advice
            {
                AdviceBriefText = model.AdviceBriefText,
                AdviceText = HtmlContentSanitizer.Sanitize(model.AdviceText),
                AdviceDate = model.AdviceDate,
                AdviceTitle=model.AdviceTitle,
                AdviceCategoryId=model.AdviceCategoryId

            };
           
            if (model.AdviceHeaderImage == null)
            {
                advice.AdviceHeaderImage = ImageUpload.ReadDefault(env, "advicetextheader.jpg");
            }
            else if (ImageUpload.TryRead(model.AdviceHeaderImage, out byte[] header, out string headerError))
            {
                advice.AdviceHeaderImage = header;
            }
            else
            {
                TempData["msg"] = headerError;
                return RedirectToAction("InsertAdvice", "Advice");
            }
            db.Add(advice);
            db.SaveChanges();
            return RedirectToAction("InsertAdviceImage","Advice",new {AdviceId=advice.Id }   );
        }
        [HttpGet]
        public IActionResult InsertAdviceImage(int AdviceId)
        {
            Advice advice = db.Find<Advice>(AdviceId);
            ViewData["Advice"] = advice;

            HttpContext.Session.SetInt32("AdviceId", AdviceId);
            return View(); 
        }
      [HttpPost]
      [ValidateAntiForgeryToken]
      public IActionResult InsertAdviceImageConfirm(AdviceImageViewModel model)
        {
            int AdviceId = HttpContext.Session.GetInt32("AdviceId").Value;
            if (ImageUpload.TryRead(model.AdviceImage, out byte[] image, out string imageError) == false)
            {
                TempData["msg"] = imageError;
                return RedirectToAction("InsertAdviceImage", "Advice", new { AdviceId = AdviceId });
            }
            AdviceImage adviceImage = model.ToEntity();
            adviceImage.Adviceimg = image;
            adviceImage.AdviceId = AdviceId;



            db.Add(adviceImage);
            db.SaveChanges();

            return RedirectToAction("InsertAdviceImage", "Advice", new { AdviceId = AdviceId });
        }
        
        [HttpGet]
        public IActionResult GetAdviceCategories(int AdvCategoryId)
        {
            List<AdviceCategory> AdviceCategories = db.adviceCategories.Where(x => x.AdviceCategoryParentId == AdvCategoryId).ToList();
            return Json(AdviceCategories);
        }
    }
}
