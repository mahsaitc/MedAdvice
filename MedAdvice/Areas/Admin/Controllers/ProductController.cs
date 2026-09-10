using MedAdvice.Data;
using MedAdvice.Models;
using MedAdvice.Services;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.mapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace MedAdvice.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminsPolicy")]
    public class ProductController : Controller
    {
        public MedAdviceDb db { get; set; }
        public ProductController(MedAdviceDb _db)
        {
            db = _db;
        }
        [HttpGet]
        public async Task<IActionResult> InsertProduct()
        {
            ViewData["brands"] = await db.Brands.ToListAsync();
            ViewData["productcategories"] = await db.ProductCategories.Where(x => x.ParentId == null).ToListAsync();

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ShowProductByCategory()
        {

            ViewData["productcategories"] = await db.ProductCategories.Where(x => x.ParentId == null).ToListAsync();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetProductByCategory(int categoryid)
        {
            ProductCategory productCategory = await db.ProductCategories.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == categoryid);
            // Loaded over AJAX into the page, so an empty list is the right answer
            // for an unknown category; NotFound() would break the caller.
            if (productCategory == null)
            {
                return View(new List<Product>());
            }
            return View(productCategory.Products.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertProductconfirm(ProductViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                ViewData["brands"] = await db.Brands.ToListAsync();
                ViewData["productcategories"] = await db.ProductCategories.Where(x => x.ParentId == null).ToListAsync();
                return View("InsertProduct", model);
            }
            Product product = model.ToEntity();
            db.Add(product);
            await db.SaveChangesAsync();
            return RedirectToAction("insertproductimage","product",new {productid=product.Id });
        }
        [HttpGet]
        public async Task<IActionResult> InsertProductImage(int ProductId)
        {
            //Session
            Product product = await db.Products.Include(x => x.Brand).FirstOrDefaultAsync(x => x.Id == ProductId);
            if (product == null)
            {
                TempData["msg"] = "رکورد مورد نظر پیدا نشد.";
                return RedirectToAction("InsertProduct", "Product");
            }
            ViewData["product"] = product;
            HttpContext.Session.SetInt32("productid", ProductId);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertProductImageConfirm(ProductImageViewModel model)
        {
            int? sessionProductId = HttpContext.Session.GetInt32("productid");
            if (sessionProductId == null)
            {
                TempData["msg"] = "نشست شما منقضی شده است. لطفا دوباره تلاش کنید.";
                return RedirectToAction("InsertProduct", "Product");
            }
            int productId = sessionProductId.Value;
            if (ModelState.IsValid == false)
            {
                ViewData["product"] = await db.Products.Include(x => x.Brand).FirstOrDefaultAsync(x => x.Id == productId);
                return View("insertproductimage", model);
            }
            ImageReadResult imageResult = await ImageUpload.ReadAsync(model.img);
            if (imageResult.Ok == false)
            {
                TempData["msg"] = imageResult.Error;
                return RedirectToAction("InsertProductImage", "Product", new { productId = productId });
            }
            ProductImage productImage = model.ToEntity();
            productImage.img = imageResult.Content;
            productImage.ProductId = productId;
            db.Add(productImage);
            await db.SaveChangesAsync();

            return RedirectToAction("InsertProductImage", "Product", new { productId = productId });
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories(int categoryId)
        {
            List<ProductCategory> productCategories = await db.ProductCategories.Where(x => x.ParentId == categoryId)
                .ToListAsync();
            return Json(productCategories);
        }
        
    }
}
