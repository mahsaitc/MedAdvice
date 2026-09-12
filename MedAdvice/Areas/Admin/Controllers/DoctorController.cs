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
    public class DoctorController : Controller
    {
        MedAdviceDb db;
        ImageStorage images;
        public DoctorController(MedAdviceDb _db, ImageStorage _images)
        {
            db = _db;
            images = _images;
        }
        [HttpGet]
        public IActionResult InsertDrSpaciality()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertDrSpaciality(DoctorSpacialityViewModel Model)
        {
            if (ModelState.IsValid == false)
            {
                return View(Model);
            }
            DoctorSpaciality doctorSpaciality = new DoctorSpaciality
            {
                SpacialityTitle = Model.SpacialityTitle,
                DrSpacialityParentId = Model.DrSpacialityParentId
               
            };

            // No file uploaded leaves the path null; the view falls back to the
            // theme image rather than copying a default into the database.
            if (Model.DrSpacialityPicture != null)
            {
                ImageSaveResult picture = await images.SaveAsync(Model.DrSpacialityPicture, ImageFolders.Doctors);
                if (picture.Ok == false)
                {
                    TempData["msg"] = picture.Error;
                    return View();
                }
                doctorSpaciality.DrSpacialityPicturePath = picture.Path;
            }
            db.Add(doctorSpaciality);
            await db.SaveChangesAsync();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> InsertDoctorProfile()
        {
            ViewData["doctorspacaiality"] = await db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToListAsync();


            return View();

        }
        [HttpGet]
        public async Task<IActionResult> GetCategories(int categoryid)
        {
          List<DoctorSpaciality> doctorSpacialities= await db.DoctorSpacialities.Where(x => x.DrSpacialityParentId==categoryid ).ToListAsync();
            return Json(doctorSpacialities);
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertDoctorConfirm(DoctorViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                ViewData["doctorspacaiality"] = await db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToListAsync();
                return View("InsertDoctorProfile", model);
            }
            ImageSaveResult profileResult = await images.SaveAsync(model.DrProfileImage, ImageFolders.Doctors);
            if (profileResult.Ok == false)
            {
                TempData["msg"] = profileResult.Error;
                return RedirectToAction("InsertDoctorProfile", "Doctor");
            }
            Doctor doctor = model.ToEntity();
            doctor.DrProfileImagePath = profileResult.Path;
            doctor.DrDetails = HtmlContentSanitizer.Sanitize(doctor.DrDetails);
            db.Add(doctor);
            await db.SaveChangesAsync();
            return RedirectToAction("InsertDrImage", "doctor", new { drid = doctor.Id });


        }
        [HttpGet]
        public async Task<IActionResult> InsertDrImage(int drid)
        {
            Doctor doctor = await db.Doctors.FirstOrDefaultAsync(x => x.Id == drid);
            if (doctor == null)
            {
                TempData["msg"] = "رکورد مورد نظر پیدا نشد.";
                return RedirectToAction("InsertDoctorProfile", "Doctor");
            }
            ViewData["doctor"] = doctor;
            HttpContext.Session.SetInt32("drid" , doctor.Id);
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertDoctortImageConfirm(DrImageViewModel model)
        {
            int? sessionDrId = HttpContext.Session.GetInt32("drid");
            if (sessionDrId == null)
            {
                TempData["msg"] = "نشست شما منقضی شده است. لطفا دوباره تلاش کنید.";
                return RedirectToAction("InsertDoctorProfile", "Doctor");
            }
            int drid = sessionDrId.Value;
            if (ModelState.IsValid == false)
            {
                ViewData["doctor"] = await db.Doctors.FirstOrDefaultAsync(x => x.Id == drid);
                return View("InsertDrImage", model);
            }
            ImageSaveResult imageResult = await images.SaveAsync(model.Doctorimg, ImageFolders.Doctors);
            if (imageResult.Ok == false)
            {
                TempData["msg"] = imageResult.Error;
                return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
            }
            DoctorImage doctorImage = model.ToEntity();
            doctorImage.DoctorimgPath = imageResult.Path;
            doctorImage.DoctorId = drid;
            db.Add(doctorImage);
            await db.SaveChangesAsync();

            return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
        }
    }
}
