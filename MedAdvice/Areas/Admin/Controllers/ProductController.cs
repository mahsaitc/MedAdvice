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
        public IActionResult InsertProduct()
        {
            ViewData["brands"] = db.Brands.ToList();
            ViewData["productcategories"] = db.ProductCategories.Where(x => x.ParentId == null).ToList();

            return View();
        }
        [HttpGet]
        public IActionResult ShowProductByCategory()
        {

            ViewData["productcategories"] = db.ProductCategories.Where(x => x.ParentId == null).ToList();
            return View();
        }
        [HttpGet]
        public IActionResult GetProductByCategory(int categoryid)
        {
            ProductCategory productCategory = db.ProductCategories.Include(x => x.Products).FirstOrDefault(x => x.Id == categoryid);
            return View(productCategory.Products.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertProductconfirm(ProductViewModel model)
        {
            Product product = model.ToEntity();
            db.Add(product);
            db.SaveChanges();
            return RedirectToAction("insertproductimage","product",new {productid=product.Id });
        }
        [HttpGet]
        public IActionResult InsertProductImage(int ProductId)
        {
            //Session
            Product product = db.Products.Include(x => x.Brand).FirstOrDefault(x => x.Id == ProductId);
            ViewData["product"] = product;
            HttpContext.Session.SetInt32("productid", ProductId);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertProductImageConfirm(ProductImageViewModel model)
        {
            int productId = HttpContext.Session.GetInt32("productid").Value;
            if (ImageUpload.TryRead(model.img, out byte[] image, out string imageError) == false)
            {
                TempData["msg"] = imageError;
                return RedirectToAction("InsertProductImage", "Product", new { productId = productId });
            }
            ProductImage productImage = model.ToEntity();
            productImage.img = image;
            productImage.ProductId = productId;
            db.Add(productImage);
            db.SaveChanges();

            return RedirectToAction("InsertProductImage", "Product", new { productId = productId });
        }
        [HttpGet]
        public IActionResult GetCategories(int categoryId)
        {
            List<ProductCategory> productCategories = db.ProductCategories.Where(x => x.ParentId == categoryId)
                .ToList();
            return Json(productCategories);
        }
        
    }
}
