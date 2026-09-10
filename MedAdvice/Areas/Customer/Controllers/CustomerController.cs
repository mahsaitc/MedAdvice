using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using MedAdvice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CustomerController : Controller
    {
        MedAdviceDb db;
        UserManager<ApplicationUser> userManager;
        public CustomerController(MedAdviceDb _db, UserManager<ApplicationUser> _userManager)
        {
            db = _db;
            userManager = _userManager;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = db.Products.Include(x => x.productImages).Include(y=>y.Brand).Include(z=>z.productcategory).ToList();
            return View(products);
        }
        [AllowAnonymous]
        public IActionResult ProductDetails(int id)
        {
            Product product = db.Products.Include(x => x.productImages).Include(z=>z.productcategory).Include(m=>m.Brand).FirstOrDefault(y => y.Id == id);
            
            var productcategorieslevelone = db.ProductCategories.Where(x => x.ParentId == null).ToList();
            var blogs = db.Blogs.Include(x => x.BlogImages).Include(y => y.BlogCategory).ToList();
            var lastfourblogs = blogs.TakeLast(4).ToList();
            ViewData["lastfourblogs"] = lastfourblogs;
            ViewData["productcategorieslevelone"] = productcategorieslevelone;
            return View(product);
        }
        [AllowAnonymous]
        public IActionResult ProductCategoryone()
        {
            var productcategoriesone = db.ProductCategories.Where(y => y.ParentId == null).ToList();
            return View(productcategoriesone);
        }
      [AllowAnonymous]
      [HttpGet]
      public IActionResult productCategoriesLevelTwo(int id)
        {
            var productcategoryleveltwo = db.ProductCategories.Where(x => x.ParentId == id);
            return View(productcategoryleveltwo);

            
        }
        [AllowAnonymous]
        public IActionResult Showproducts(int id)
        {
            var Products = db.Products.Where(x => x.ProductCategoryId == id).Include(y => y.productcategory).ToList();
            return View(Products);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddtoPurchaseCart(int productid)
        {
            string userid = (await userManager.FindByNameAsync(User.Identity.Name)).Id;
            Purchasecart purchasecart = db.Purchasecarts.FirstOrDefault(x => x.UserId == userid && x.isOpen == true);
            if (purchasecart == null)
            {
                purchasecart = new Purchasecart
                {
                    isOpen = true,
                    UserId = userid,
                    createdDate = DateTime.Now
                };
                db.Add(purchasecart);
                db.SaveChanges();

            }
            if (db.purchaseCartItems.Any(x => x.PurchaseCartId == purchasecart.Id && x.ProductId == productid) == false)
            {
                PurchaseCartItem purchaseCartItem = new PurchaseCartItem
                {
                    ProductId = productid,
                    count = 1,
                    PurchaseCartId = purchasecart.Id
                };
                db.Add(purchaseCartItem);
                db.SaveChanges();
            };
            return Json(true);
        }
        public async Task<IActionResult> PurchaseCartManagement()
        {


            string userid = (await userManager.FindByNameAsync(User.Identity.Name)).Id;
            Purchasecart purchaseCart = db.Purchasecarts.Include(x => x.PurchaseCartItems).ThenInclude(x => x.Product)
                 .ThenInclude(x => x.productImages)
                 .FirstOrDefault(x => x.UserId == userid && x.isOpen == true);
            ViewData["totalprice"] = $"{PurchaseCartTotalPrice(purchaseCart.Id):0,0}";

            HttpContext.Session.SetInt32("purchaseCartId", purchaseCart.Id);

            return View(purchaseCart);
        }
        [NonAction]
        public double PurchaseCartTotalPrice(int purchasecartid)
        {
            var items = db.purchaseCartItems
                .Where(x => x.PurchaseCartId == purchasecartid)
                 .Include(x => x.Product).ToList();

            double totalsum = items.Sum(x => x.count * x.Product.price);
            return totalsum;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeCountPurchaseItem(int count, int purchaseitemid)
        {
            if (count <= 0)
            {
                return Json(new
                {
                    status = false,
                });
            }
            PurchaseCartItem item = await FindOwnedPurchaseCartItem(purchaseitemid);
            if (item == null)
            {
                return Json(new
                {
                    status = false,
                });
            }
            else
            {
                item.count = count;
                db.SaveChanges();
                return Json(
                    new
                    {
                        status = true,
                        totalprice = $"{PurchaseCartTotalPrice(item.PurchaseCartId):0,0}"
                    });
            }
        }

        /// Returns the item only when it belongs to the signed-in user's open cart; otherwise null.
        [NonAction]
        private async Task<PurchaseCartItem> FindOwnedPurchaseCartItem(int purchaseitemid)
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            if (user == null)
                return null;
            return db.purchaseCartItems
                .Include(x => x.PurchaseCart)
                .FirstOrDefault(x => x.Id == purchaseitemid
                    && x.PurchaseCart.UserId == user.Id
                    && x.PurchaseCart.isOpen == true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromPurchaseCart(int purchaseitemid)
        {
            try
            {
                PurchaseCartItem item = await FindOwnedPurchaseCartItem(purchaseitemid);
                if (item == null)
                {
                    return Json(new
                    {
                        status = false,
                    });
                }
                int purchaseCartId = item.PurchaseCartId;
                db.purchaseCartItems.Remove(item);
                db.SaveChanges();
                return Json(
                    new
                    {
                        status = true,
                        totalprice = $"{PurchaseCartTotalPrice(purchaseCartId):0,0}"
                    });
            }
            catch
            {
                return Json(
                     new
                     {
                         status = false,
                     });
            }
        }
        [AllowAnonymous]
        public IActionResult SearchProduct(string sname)
        {
            var sproducts = db.Products.Include(y=>y.productImages).Include(z=>z.productcategory). Where(x => x.englishname.Contains(sname) || x.descreption.Contains(sname)).ToList();
            return View(sproducts);
        }
    }
}

   

