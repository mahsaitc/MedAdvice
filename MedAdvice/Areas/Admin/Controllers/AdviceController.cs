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
    public class AdviceController : Controller
    {
        MedAdviceDb db;
        public AdviceController(MedAdviceDb _db)
        {
            db = _db;
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

            if (Model.AdviceCategoryPicture != null)
            {
                if (Model.AdviceCategoryPicture.Length <= 5 * Math.Pow(1024, 2))
                {
                    string extension = System.IO.Path.GetExtension(Model.AdviceCategoryPicture.FileName.ToLower());
                    if (extension == ".jpeg" || extension == ".png" || extension == ".jpg")
                    {
                        byte[] b = new byte[Model.AdviceCategoryPicture.Length];
                        Model.AdviceCategoryPicture.OpenReadStream().Read(b, 0, b.Length);
                        adviceCategory.AdviceCategoryPicture = b;
                    }
                }
            }
            else
            {
                byte[] ax = System.IO.File.ReadAllBytes("advicetextheader.jpg");
                adviceCategory.AdviceCategoryPicture = ax;
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
           
            if (model.AdviceHeaderImage != null)
            {
                if (model.AdviceHeaderImage.Length <= 5 * Math.Pow(1024, 2))
                {
                    string extension = System.IO.Path.GetExtension(model.AdviceHeaderImage.FileName.ToLower());
                    if (extension == ".jpeg" || extension == ".png" || extension == ".jpg")
                    {
                        byte[] b = new byte[model.AdviceHeaderImage.Length];
                        model.AdviceHeaderImage.OpenReadStream().Read(b, 0, b.Length);
                        advice.AdviceHeaderImage = b;
                    }
                }
            }
            else
            {
                byte[] ax = System.IO.File.ReadAllBytes("advicetextheader.jpg");
                advice.AdviceHeaderImage = ax;
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
      public IActionResult InsertAdviceImageConfirm(AdviceImageViewModel model, [FromServices] IMapper mapper)
        {
            AdviceImage adviceImage = mapper.Map<AdviceImage>(model);
            int AdviceId = HttpContext.Session.GetInt32("AdviceId").Value;
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
