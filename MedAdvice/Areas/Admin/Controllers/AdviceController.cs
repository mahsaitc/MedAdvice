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
        public async Task<IActionResult> insertAdviceCategory(AdviceCategoryViewModel Model)
        {
            if (ModelState.IsValid == false)
            {
                return View(Model);
            }
            AdviceCategory adviceCategory = new AdviceCategory { 
                AdviceCategoryname = Model.AdviceCategoryname,
                AdviceCategoryParentId = Model.AdviceCategoryParentId

            };

            if (Model.AdviceCategoryPicture == null)
            {
                adviceCategory.AdviceCategoryPicture = await ImageUpload.ReadDefaultAsync(env, "advicetextheader.jpg");
            }
            else
            {
                ImageReadResult picture = await ImageUpload.ReadAsync(Model.AdviceCategoryPicture);
                if (picture.Ok == false)
                {
                    TempData["msg"] = picture.Error;
                    return View();
                }
                adviceCategory.AdviceCategoryPicture = picture.Content;
            }
            db.Add(adviceCategory);
            await db.SaveChangesAsync();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> InsertAdvice()
        {
            ViewData["AdviceCategory"] = await db.adviceCategories.Where(x => x.AdviceCategoryParentId == null).ToListAsync();
           
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertAdviceConfirm(AdviceViewmodel model)
        {
            if (ModelState.IsValid == false)
            {
                ViewData["AdviceCategory"] = await db.adviceCategories.Where(x => x.AdviceCategoryParentId == null).ToListAsync();
                return View("InsertAdvice", model);
            }
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
                advice.AdviceHeaderImage = await ImageUpload.ReadDefaultAsync(env, "advicetextheader.jpg");
            }
            else
            {
                ImageReadResult header = await ImageUpload.ReadAsync(model.AdviceHeaderImage);
                if (header.Ok == false)
                {
                    TempData["msg"] = header.Error;
                    return RedirectToAction("InsertAdvice", "Advice");
                }
                advice.AdviceHeaderImage = header.Content;
            }
            db.Add(advice);
            await db.SaveChangesAsync();
            return RedirectToAction("InsertAdviceImage","Advice",new {AdviceId=advice.Id }   );
        }
        [HttpGet]
        public async Task<IActionResult> InsertAdviceImage(int AdviceId)
        {
            Advice advice = await db.FindAsync<Advice>(AdviceId);
            if (advice == null)
            {
                TempData["msg"] = "رکورد مورد نظر پیدا نشد.";
                return RedirectToAction("InsertAdvice", "Advice");
            }
            ViewData["Advice"] = advice;

            HttpContext.Session.SetInt32("AdviceId", AdviceId);
            return View(); 
        }
      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> InsertAdviceImageConfirm(AdviceImageViewModel model)
        {
            int? sessionAdviceId = HttpContext.Session.GetInt32("AdviceId");
            if (sessionAdviceId == null)
            {
                TempData["msg"] = "نشست شما منقضی شده است. لطفا دوباره تلاش کنید.";
                return RedirectToAction("InsertAdvice", "Advice");
            }
            int AdviceId = sessionAdviceId.Value;
            if (ModelState.IsValid == false)
            {
                ViewData["Advice"] = await db.FindAsync<Advice>(AdviceId);
                return View("InsertAdviceImage", model);
            }
            ImageReadResult imageResult = await ImageUpload.ReadAsync(model.AdviceImage);
            if (imageResult.Ok == false)
            {
                TempData["msg"] = imageResult.Error;
                return RedirectToAction("InsertAdviceImage", "Advice", new { AdviceId = AdviceId });
            }
            AdviceImage adviceImage = model.ToEntity();
            adviceImage.Adviceimg = imageResult.Content;
            adviceImage.AdviceId = AdviceId;



            db.Add(adviceImage);
            await db.SaveChangesAsync();

            return RedirectToAction("InsertAdviceImage", "Advice", new { AdviceId = AdviceId });
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAdviceCategories(int AdvCategoryId)
        {
            List<AdviceCategory> AdviceCategories = await db.adviceCategories.Where(x => x.AdviceCategoryParentId == AdvCategoryId).ToListAsync();
            return Json(AdviceCategories);
        }
    }
}
