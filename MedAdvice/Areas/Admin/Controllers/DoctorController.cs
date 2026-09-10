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
    public class DoctorController : Controller
    {
        MedAdviceDb db;
        public DoctorController(MedAdviceDb _db)
        {
            db = _db;
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

            if (Model.DrSpacialityPicture != null)
            {
                if (Model.DrSpacialityPicture.Length <= 5 * Math.Pow(1024, 2))
                {
                    string extension = System.IO.Path.GetExtension(Model.DrSpacialityPicture.FileName.ToLower());
                    if (extension == ".jpeg" || extension == ".png" || extension == ".jpg")
                    {
                        byte[] b = new byte[Model.DrSpacialityPicture.Length];
                       Model.DrSpacialityPicture.OpenReadStream().Read(b, 0, b.Length);
                        doctorSpaciality.DrSpacialityPicture = b;
                    }
                }
            }
            else
            {
                byte[] ax = System.IO.File.ReadAllBytes("doctor2.png");
                doctorSpaciality.DrSpacialityPicture = ax;
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
        public IActionResult InsertDoctorConfirm([FromServices] IMapper mapper, DoctorViewModel model)
        {
            Doctor doctor = mapper.Map<Doctor>(model);
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
        public IActionResult InsertDoctortImageConfirm(DrImageViewModel model, [FromServices] IMapper mapper)
        {
            DoctorImage doctorImage = mapper.Map<DoctorImage>(model);
            int drid = HttpContext.Session.GetInt32("drid").Value;
            doctorImage.DoctorId = drid;
            db.Add(doctorImage);
            db.SaveChanges();

            return RedirectToAction("InsertDrImage", "doctor", new { drid = drid });
        }
    }
}
