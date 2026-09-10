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
        public async Task<IActionResult> Index()
        {
            var products = await db.Products.Include(x => x.productImages).Include(y=>y.Brand).Include(z=>z.productcategory).ToListAsync();
            return View(products);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ProductDetails(int id)
        {
            Product product = await db.Products.Include(x => x.productImages).Include(z=>z.productcategory).Include(m=>m.Brand).FirstOrDefaultAsync(y => y.Id == id);
            
            var productcategorieslevelone = await db.ProductCategories.Where(x => x.ParentId == null).ToListAsync();
            var lastfourblogs = await db.Blogs
                .OrderByDescending(x => x.Id)
                .Take(4)
                .ToListAsync();
            ViewData["lastfourblogs"] = lastfourblogs;
            ViewData["productcategorieslevelone"] = productcategorieslevelone;
            return View(product);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ProductCategoryone()
        {
            var productcategoriesone = await db.ProductCategories.Where(y => y.ParentId == null).ToListAsync();
            return View(productcategoriesone);
        }
      [AllowAnonymous]
      [HttpGet]
      public async Task<IActionResult> productCategoriesLevelTwo(int id)
        {
            var productcategoryleveltwo = await db.ProductCategories.Where(x => x.ParentId == id).ToListAsync();
            return View(productcategoryleveltwo);

            
        }
        [AllowAnonymous]
        public async Task<IActionResult> Showproducts(int id)
        {
            var Products = await db.Products.Where(x => x.ProductCategoryId == id).Include(y => y.productcategory).ToListAsync();
            return View(Products);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddtoPurchaseCart(int productid)
        {
            string userid = (await userManager.FindByNameAsync(User.Identity.Name)).Id;
            Purchasecart purchasecart = await db.Purchasecarts.FirstOrDefaultAsync(x => x.UserId == userid && x.isOpen == true);
            if (purchasecart == null)
            {
                purchasecart = new Purchasecart
                {
                    isOpen = true,
                    UserId = userid,
                    createdDate = DateTime.Now
                };
                db.Add(purchasecart);
                await db.SaveChangesAsync();

            }
            if (await db.purchaseCartItems.AnyAsync(x => x.PurchaseCartId == purchasecart.Id && x.ProductId == productid) == false)
            {
                PurchaseCartItem purchaseCartItem = new PurchaseCartItem
                {
                    ProductId = productid,
                    count = 1,
                    PurchaseCartId = purchasecart.Id
                };
                db.Add(purchaseCartItem);
                await db.SaveChangesAsync();
            };
            return Json(true);
        }
        public async Task<IActionResult> PurchaseCartManagement()
        {


            string userid = (await userManager.FindByNameAsync(User.Identity.Name)).Id;
            Purchasecart purchaseCart = await db.Purchasecarts.Include(x => x.PurchaseCartItems).ThenInclude(x => x.Product)
                 .ThenInclude(x => x.productImages)
                 .FirstOrDefaultAsync(x => x.UserId == userid && x.isOpen == true);
            ViewData["totalprice"] = $"{await PurchaseCartTotalPrice(purchaseCart.Id):0,0}";

            HttpContext.Session.SetInt32("purchaseCartId", purchaseCart.Id);

            return View(purchaseCart);
        }
        [NonAction]
        public async Task<double> PurchaseCartTotalPrice(int purchasecartid)
        {
            var items = await db.purchaseCartItems
                .Where(x => x.PurchaseCartId == purchasecartid)
                 .Include(x => x.Product).ToListAsync();

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
                await db.SaveChangesAsync();
                return Json(
                    new
                    {
                        status = true,
                        totalprice = $"{await PurchaseCartTotalPrice(item.PurchaseCartId):0,0}"
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
            return await db.purchaseCartItems
                .Include(x => x.PurchaseCart)
                .FirstOrDefaultAsync(x => x.Id == purchaseitemid
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
                await db.SaveChangesAsync();
                return Json(
                    new
                    {
                        status = true,
                        totalprice = $"{await PurchaseCartTotalPrice(purchaseCartId):0,0}"
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
        public async Task<IActionResult> SearchProduct(string sname)
        {
            var sproducts = await db.Products.Include(y=>y.productImages).Include(z=>z.productcategory). Where(x => x.englishname.Contains(sname) || x.descreption.Contains(sname)).ToListAsync();
            return View(sproducts);
        }
    }
}

   

