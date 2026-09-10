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
        IWebHostEnvironment env;
        public DoctorController(MedAdviceDb _db, IWebHostEnvironment _env)
        {
            db = _db;
            env = _env;
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
            DoctorSpaciality doctorSpaciality = new DoctorSpaciality
            {
                SpacialityTitle = Model.SpacialityTitle,
                DrSpacialityParentId = Model.DrSpacialityParentId
               
            };

            if (Model.DrSpacialityPicture == null)
            {
                doctorSpaciality.DrSpacialityPicture = await ImageUpload.ReadDefaultAsync(env, "doctor2.png");
            }
            else
            {
                ImageReadResult picture = await ImageUpload.ReadAsync(Model.DrSpacialityPicture);
                if (picture.Ok == false)
                {
                    TempData["msg"] = picture.Error;
                    return View();
                }
                doctorSpaciality.DrSpacialityPicture = picture.Content;
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
            ImageReadResult profileResult = await ImageUpload.ReadAsync(model.DrProfileImage);
            if (profileResult.Ok == false)
            {
                TempData["msg"] = profileResult.Error;
                return RedirectToAction("InsertDoctorProfile", "Doctor");
            }
            Doctor doctor = model.ToEntity();
            doctor.DrProfileImage = profileResult.Content;
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
            ImageReadResult imageResult = await ImageUpload.ReadAsync(model.Doctorimg);
            if (imageResult.Ok == false)
            {
                TempData["msg"] = imageResult.Error;
                return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
            }
            DoctorImage doctorImage = model.ToEntity();
            doctorImage.Doctorimg = imageResult.Content;
            doctorImage.DoctorId = drid;
            db.Add(doctorImage);
            await db.SaveChangesAsync();

            return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
        }
    }
}
