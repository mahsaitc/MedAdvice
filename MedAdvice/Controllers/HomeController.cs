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
using System.Diagnostics;
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
        IHttpClientFactory httpClientFactory;
        public HomeController(MedAdviceDb _db,UserManager<ApplicationUser> _userManager,IConfiguration _configuration,IHttpClientFactory _httpClientFactory)
        {
            Db = _db;
            userManager = _userManager;
            configuration = _configuration;
            httpClientFactory = _httpClientFactory;
        }
        ///advice area
        ///
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertAdviceCommentConfirm(AdviceCommentViewMoldel model)
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
            await Db.SaveChangesAsync();
            return RedirectToAction("ViewAdviceDetails",new{ id = model.AdviceId });
        }
        public async Task<IActionResult> ShowAdviceCategoriesLevelOne()
        {
            var AdviceCategories = await Db.adviceCategories.Where(x => x.AdviceCategoryParentId == null).ToListAsync();
            return View(AdviceCategories);
        }
        public async Task<IActionResult> ShowAdviceCategoriesLevelTwo(int id)
        {
            var AdviceCategoriesLevelTwo = await Db.adviceCategories.Where(x => x.AdviceCategoryParentId == id).ToListAsync();
           
            return View(AdviceCategoriesLevelTwo);
        }
       
        public async Task<IActionResult> ShowAdvices(int id)
        {
            var advices = await Db.Advices.Where(x => x.AdviceCategoryId == id).Include(y => y.AdviceCategory).ToListAsync();
            return View(advices);
        }
        public async Task<IActionResult> MedicalAdvices()
        {
            var Advices = await Db.Advices.Include(x=>x.AdviceCategory) .ToListAsync();
            return View(Advices);
        }
        public async Task<IActionResult> ViewAdviceDetails(int id)
        {
            Advice advice = await Db.Advices.Include(y=>y.AdviceImages).Include(z=>z.AdviceCategory).FirstOrDefaultAsync(x=>x.Id==id);
            if (advice == null)
            {
                return NotFound();
            }
            var adviceCategoriesfirst = await Db.adviceCategories.Where(x=>x.AdviceCategoryParentId==null).ToListAsync();
            var lastfourblogs = await Db.Blogs
                .OrderByDescending(x => x.Id)
                .Take(4)
                .ToListAsync();
            ViewData["lastfourblogs"] = lastfourblogs;
            ViewData["advice"] = advice;
            
            return View(adviceCategoriesfirst);

        }
        [HttpGet]
        public async Task<IActionResult> GetAdviceCategories(int id)
        {
            var advicecategories = await Db.adviceCategories.Where(x => x.AdviceCategoryParentId == id).ToListAsync();
            return Json(advicecategories);
        }
        public async Task<IActionResult> SearchAdvice(string sname)
        {
            var sadvice = await Db.Advices.Include(x => x.AdviceImages).Include(y => y.AdviceCategory).Where(z => z.AdviceTitle.Contains(sname)).ToListAsync();
            return View(sadvice);
        }
   //doctor area
      
        public async Task<IActionResult> Doctors()
        {
            var doctors = await Db.Doctors.Include(x=>x.DrSpaciality).ToListAsync();

            return View(doctors);
        }
        public async Task<IActionResult> DoctorDetails(int id)
        {
            Doctor doctor = await Db.Doctors.Include(x => x.DrImages).Include(z => z.DrSpaciality).FirstOrDefaultAsync(y => y.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["doctor"] = doctor;
           
           
            var lastfourblogs = await Db.Blogs
                .OrderByDescending(x => x.Id)
                .Take(4)
                .ToListAsync();
            ViewData["lastfourblogs"] = lastfourblogs;
           
            var DrSpacialityLevelOne = await Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToListAsync();
            return View(DrSpacialityLevelOne);
        }
        [HttpGet]
        public async Task<IActionResult> GetDrSpaciality(int id)
        {
            var drspacialities = await Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == id).ToListAsync();
            return Json(drspacialities);
        }
        public async Task<IActionResult> ViewDrGroups()
        {
            var drgroups = await Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == null).ToListAsync();
            return View(drgroups);
        }
        public async Task<IActionResult> ViewDrSpacialities(int id)
        {
            var drspacialities = await Db.DoctorSpacialities.Where(x => x.DrSpacialityParentId == id).ToListAsync();
            return View(drspacialities);
        }
        public async Task<IActionResult> ViewDoctorsbyCategory(int id)
        {
            var doctors = await Db.Doctors.Include(x => x.DrSpaciality).Where(y => y.DrSpacialityId == id).ToListAsync();
            return View(doctors);
        }
        public async Task<IActionResult> SearchDoctor(string sname)
        {
            var sdoctor = await Db.Doctors.Include(x => x.DrImages).Include(y => y.DrSpaciality).Where(z => z.FamillyName.Contains(sname) || z.FirstName.Contains(sname)).ToListAsync();
            return View(sdoctor);
        }
        //widget area

        ///blog area
        public async Task<IActionResult> Blog()
        {
            var blogs = await Db.Blogs.Include(x=>x.BlogCategory).ToListAsync();
          
            return View(blogs);
        }
        
        [HttpGet]
        public async Task<IActionResult> BlogCategoriesLevelTwo(int id)
        {
            var blogcategories = await Db.blogCategories.Where(x => x.BlogCategoryParentId == id).ToListAsync();
            return Json(blogcategories);
        }
        public async Task<IActionResult> BlogDetails(int id)
        {
            Blog blog = await Db.Blogs.Include(x => x.BlogImages).Include(z => z.BlogCategory).FirstOrDefaultAsync(y => y.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            ViewData["blog"] = blog;
            var blogcategoriesfirst = await Db.blogCategories.Where(x => x.BlogCategoryParentId == null).ToListAsync();
            var lastfourblogs = await Db.Blogs
                .OrderByDescending(x => x.Id)
                .Take(4)
                .ToListAsync();
            ViewData["lastfourblogs"] = lastfourblogs;
            return View(blogcategoriesfirst);
        }
        public async Task<IActionResult> BlogCategories()
        {
            var blogcategorylevelone = await Db.blogCategories.Where(z => z.BlogCategoryParentId == null).ToListAsync();
            return View(blogcategorylevelone);
        }
        public async Task<IActionResult> BlogCategoriesTwo(int id)
        {
            var blogcategorylevelTwo = await Db.blogCategories.Where(z => z.BlogCategoryParentId == id).ToListAsync();
            return View(blogcategorylevelTwo);
        }
        public async Task<IActionResult> BlogCategoriesThree(int id)
        {
            var blogs = await Db.Blogs.Include(x => x.BlogCategory).Include(y => y.BlogImages).Where(z => z.BlogCategoryId == id).ToListAsync();
            return View(blogs);
        }
        public async Task<IActionResult> SearchBlog(string sname)
        {
            var sblog = await Db.Blogs.Include(x => x.BlogImages).Include(y => y.BlogCategory).Where(z => z.BlogTitle.Contains(sname) || z.BlogText.Contains(sname) || z.BlogBriefText.Contains(sname)).ToListAsync();
            return View(sblog);
        }
        public IActionResult Contact()
        {
            return View();
        }
        public async Task<IActionResult> Home()
        {
            HomepageContent content = await Db.HomepageContents.FirstOrDefaultAsync();
            List<HomepageSection> sections = await Db.HomepageSections
                .Where(x => x.IsVisible)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            List<FeaturedArticle> pinned = await Db.FeaturedArticles
                .Include(x => x.Blog)
                .Include(x => x.Advice)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            HomepageViewModel model = new HomepageViewModel
            {
                // An empty instance rather than null, so the partials bind without guards.
                Content = content ?? new HomepageContent(),
                Sections = sections
            };

            foreach (FeaturedArticle article in pinned)
            {
                if (article.Blog != null)
                {
                    model.Featured.Add(new FeaturedArticleViewModel
                    {
                        Title = article.Blog.BlogTitle,
                        BriefText = article.Blog.BlogBriefText,
                        Image = article.Blog.BlogHeaderImage,
                        Controller = "home",
                        Action = "BlogDetails",
                        RouteId = article.Blog.Id
                    });
                }
                else if (article.Advice != null)
                {
                    model.Featured.Add(new FeaturedArticleViewModel
                    {
                        Title = article.Advice.AdviceTitle,
                        BriefText = article.Advice.AdviceBriefText,
                        Image = article.Advice.AdviceHeaderImage,
                        Controller = "home",
                        Action = "ViewAdviceDetails",
                        RouteId = article.Advice.Id
                    });
                }
            }

            return View(model);
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
            HttpClient httpClient = httpClientFactory.CreateClient();
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
            HttpClient httpClient = httpClientFactory.CreateClient();
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

        /// Target of UseExceptionHandler and UseStatusCodePagesWithReExecute.
        /// Must not touch the database or throw: it is the last line of defence.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            ViewData["StatusCode"] = id;
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

