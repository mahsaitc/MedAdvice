using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MedAdvice.Controllers
{
    public class HomeController : Controller
    {
        MedAdviceDb Db;
        UserManager<ApplicationUser> userManager;
        IConfiguration configuration;
        public HomeController(MedAdviceDb _db,UserManager<ApplicationUser> _userManager,IConfiguration _configuration)
        {
            Db = _db;
            userManager = _userManager;
            configuration = _configuration;
        }
        ///advice area
        ///
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertAdviceCommentConfirm(AdviceCommentViewMoldel model)
        {
            AdviceComment adviceComment = new AdviceComment { 
                comment = model.comment,
                EmailAdress = model.EmailAdress,
                lasttname = model.lastname,
                Website = model.Website,
                Userid = model.Userid,
                AdviceId = model.AdviceId
            };
            
            Db.Add(adviceComment);
            Db.SaveChanges();
            return RedirectToAction("ViewAdviceDetails",new{ id = model.AdviceId });
        }
        public IActionResult ShowAdviceCategoriesLevelOne()
        {
            var AdviceCategories = Db.adviceCategories.Where(x => x.AdviceCategoryParentId == null).ToList();
            return View(AdviceCategories);
        }
        public IActionResult ShowAdviceCategoriesLevelTwo(int id)
        {
            var AdviceCategoriesLevelTwo = Db.adviceCategories.Where(x => x.AdviceCategoryParentId == id).ToList();
           
            return View(AdviceCategoriesLevelTwo);
        }
       
        public IActionResult ShowAdvices(int id)
        {
            var advices = Db.Advices.Where(x => x.AdviceCategoryId == id).Include(y => y.AdviceCategory).ToList();
            return View(advices);
        }
        public IActionResult MedicalAdvices()
        {
            var Advices = Db.Advices.Include(x=>x.AdviceCategory) .ToList();
            return View(Advices);
        }
        public IActionResult ViewAdviceDetails(int id)
        {
            Advice advice = Db.Advices.Include(y=>y.AdviceImages).Include(z=>z.AdviceCategory).FirstOrDefault(x=>x.Id==id);
            var adviceCategoriesfirst = Db.adviceCategories.Where(x=>x.AdviceCategoryParentId==null).ToList();
            var blogs = Db.Blogs.Include(x => x.BlogImages).Include(y => y.BlogCategory).ToList();
            var lastfourblogs = blogs.TakeLast(4).ToList();
            ViewData["lastfourblogs"] = lastfourblogs;
            ViewData["advice"] = advice;
            
            return View(adviceCategoriesfirst);

        }
        [HttpGet]
        public IActionResult GetAdviceCategories(int id)
        {
            var advicecategories = Db.adviceCategories.Where(x => x.AdviceCategoryParentId == id);
            return Json(advicecategories);
        }
        public IActionResult SearchAdvice(string sname)
        {
            var sadvice = Db.Advices.Include(x => x.AdviceImages).Include(y => y.AdviceCategory).Where(z => z.AdviceTitle.Contains(sname)).ToList();
            return View(sadvice);
        }
   //doctor area
      
        public IActionResult Doctors()
        {
            var doctors = Db.Doctors.Include(x=>x.DrSpaciality). ToList();

            return View(doctors);
        }
        public IActionResult DoctorDetails(int id)
        {
            Doctor doctor = Db.Doctors.Include(x => x.DrImages).Include(z=>z.DrSpaciality). FirstOrDefault(y => y.Id == id);
            ViewData["doctor"] = doctor;
           
           
            var blogs = Db.Blogs.Include(x => x.BlogImages).Include(y => y.BlogCategory).ToList();
            var lastfourblogs = blogs.TakeLast(4).ToList();
            ViewData["lastfourblogs"] = lastfourblogs;
           
            var DrSpacialityLevelOne = Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToList();
            return View(DrSpacialityLevelOne);
        }
        [HttpGet]
        public IActionResult GetDrSpaciality(int id)
        {
            var drspacialities = Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == id);
            return Json(drspacialities);
        }
        public IActionResult ViewDrGroups()
        {
            var drgroups = Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToList();
            return View(drgroups);
        }
        public IActionResult ViewDrSpacialities(int id)
        {
            var drspacialities = Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == id).ToList();
            return View(drspacialities);
        }
        public IActionResult ViewDoctorsbyCategory(int id)
        {
            var doctors = Db.Doctors.Include(x => x.DrSpaciality).Where(y => y.DrSpacialityId == id).ToList();
            return View(doctors);
        }
        public IActionResult SearchDoctor(string sname)
        {
            var sdoctor = Db.Doctors.Include(x => x.DrImages).Include(y => y.DrSpaciality).Where(z => z.FamillyName.Contains(sname) || z.FirstName.Contains(sname)).ToList();
            return View(sdoctor);
        }
        //widget area

        ///blog area
        public IActionResult Blog()
        {
            var blogs = Db.Blogs.Include(x=>x.BlogCategory). ToList();
          
            return View(blogs);
        }
        
        [HttpGet]
        public IActionResult BlogCategoriesLevelTwo(int id)
        {
            var blogcategories = Db.blogCategories.Where(x => x.BlogCategoryParentId == id);
            return Json(blogcategories);
        }
        public IActionResult BlogDetails(int id)
        {
            Blog blog = Db.Blogs.Include(x => x.BlogImages).Include(z => z.BlogCategory).FirstOrDefault(y => y.Id == id);
            ViewData["blog"] = blog;
            var blogcategoriesfirst = Db.blogCategories.Where(x => x.BlogCategoryParentId == null).ToList();
            var blogs = Db.Blogs.Include(x => x.BlogImages).Include(y => y.BlogCategory).ToList();
            var lastfourblogs = blogs.TakeLast(4).ToList();
            ViewData["lastfourblogs"] = lastfourblogs;
            return View(blogcategoriesfirst);
        }
        public IActionResult BlogCategories()
        {
            var blogcategorylevelone = Db.blogCategories.Where(z => z.BlogCategoryParentId == null).ToList();
            return View(blogcategorylevelone);
        }
        public IActionResult BlogCategoriesTwo(int id)
        {
            var blogcategorylevelTwo = Db.blogCategories.Where(z => z.BlogCategoryParentId == id).ToList();
            return View(blogcategorylevelTwo);
        }
        public IActionResult BlogCategoriesThree(int id)
        {
            var blogs = Db.Blogs.Include(x => x.BlogCategory).Include(y => y.BlogImages).Where(z => z.BlogCategoryId == id).ToList();
            return View(blogs);
        }
        public IActionResult SearchBlog(string sname)
        {
            var sblog = Db.Blogs.Include(x => x.BlogImages).Include(y => y.BlogCategory).Where(z => z.BlogTitle.Contains(sname) || z.BlogText.Contains(sname) || z.BlogBriefText.Contains(sname)).ToList();
            return View(sblog);
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult Home()
        {
            return View();
        }
        class USD
        {
            public string rate { get; set; }
        }
        class bpi
        {
            public USD USD { get; set; }
        }
        class Data
        {
            public bpi bpi { get; set; }
        }
        public IActionResult Index()
        {
            //TempData["msg"] = "پیام تست";
            return View();

        }

        class Current
        {
            public float temp_c { get; set; }
        }
        class Datas
        {
            public Current Current { get; set; }
        }
        public async Task<IActionResult> ShowTehranWeather()
        {
            HttpClient httpClient = new HttpClient();
            var Status = await httpClient.GetAsync(
                $"{configuration["WeatherApi:BaseUrl"]}?key={configuration["WeatherApi:Key"]}&q=tehran&aqi=yes");
            if (Status.IsSuccessStatusCode)
            {
                string content = await Status.Content.ReadAsStringAsync();
                var Datas = Newtonsoft.Json.JsonConvert.DeserializeObject<Datas>(content);
                ViewData["Tehran Temprature"] = Datas.Current.temp_c;

            }
            return View();

        }


        public async Task<IActionResult> ShowBitcoinPrice()
        {
            HttpClient httpClient = new HttpClient();
            var Status = await httpClient.GetAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
            if (Status.IsSuccessStatusCode)
            {
                string content = await Status.Content.ReadAsStringAsync();
                var Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Data>(content);
                ViewData["BitCoin"] = Data.bpi.USD.rate;

            }
            return View();
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        public IActionResult TermsCondition()
        {
            return View();
        }
        public IActionResult Testimonials()
        {
            return View();
        }
        public IActionResult Gallery()
        {
            return View();
        }
        public IActionResult FAQ()
        {
            return View();
        }
        public IActionResult Team()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }
    }
}

