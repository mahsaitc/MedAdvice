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
        public IActionResult InsertDrSpaciality(DoctorSpacialityViewModel Model)
        {
            DoctorSpaciality doctorSpaciality = new DoctorSpaciality
            {
                SpacialityTitle = Model.SpacialityTitle,
                DrSpacialityParentId = Model.DrSpacialityParentId
               
            };

            if (Model.DrSpacialityPicture == null)
            {
                doctorSpaciality.DrSpacialityPicture = ImageUpload.ReadDefault(env, "doctor2.png");
            }
            else if (ImageUpload.TryRead(Model.DrSpacialityPicture, out byte[] picture, out string pictureError))
            {
                doctorSpaciality.DrSpacialityPicture = picture;
            }
            else
            {
                TempData["msg"] = pictureError;
                return View();
            }
            db.Add(doctorSpaciality);
            db.SaveChanges();
            return View();
        }
        [HttpGet]
        public IActionResult InsertDoctorProfile()
        {
            ViewData["doctorspacaiality"] = db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToList();


            return View();

        }
        [HttpGet]
        public IActionResult GetCategories(int categoryid)
        {
          List<DoctorSpaciality> doctorSpacialities= db.DoctorSpacialities.Where(x => x.DrSpacialityParentId==categoryid ).ToList();
            return Json(doctorSpacialities);
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertDoctorConfirm(DoctorViewModel model)
        {
            if (ImageUpload.TryRead(model.DrProfileImage, out byte[] profile, out string profileError) == false)
            {
                TempData["msg"] = profileError;
                return RedirectToAction("InsertDoctorProfile", "Doctor");
            }
            Doctor doctor = model.ToEntity();
            doctor.DrProfileImage = profile;
            doctor.DrDetails = HtmlContentSanitizer.Sanitize(doctor.DrDetails);
            db.Add(doctor);
            db.SaveChanges();
            return RedirectToAction("InsertDrImage", "doctor", new { drid = doctor.Id });


        }
        [HttpGet]
        public IActionResult InsertDrImage(int drid)
        {
            Doctor doctor = db.Doctors.FirstOrDefault(x => x.Id == drid);
            ViewData["doctor"] = doctor;
            HttpContext.Session.SetInt32("drid" , doctor.Id);
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertDoctortImageConfirm(DrImageViewModel model)
        {
            int drid = HttpContext.Session.GetInt32("drid").Value;
            if (ImageUpload.TryRead(model.Doctorimg, out byte[] image, out string imageError) == false)
            {
                TempData["msg"] = imageError;
                return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
            }
            DoctorImage doctorImage = model.ToEntity();
            doctorImage.Doctorimg = image;
            doctorImage.DoctorId = drid;
            db.Add(doctorImage);
            db.SaveChanges();

            return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
        }
    }
}
