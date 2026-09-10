using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedAdvice.Data
{
    public class MedAdviceDb : IdentityDbContext<ApplicationUser>
    {
        public MedAdviceDb(DbContextOptions<MedAdviceDb> options)
            : base(options)
        {
        }
        public DbSet<AdviceComment> adviceComments { get; set; }
        public DbSet<Advice> Advices { get; set; }
        public DbSet<AdviceImage> AdviceImages { get; set; }
        public DbSet<AdviceCategory> adviceCategories { get; set; }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorImage> DoctorImages { get; set; }
        public DbSet<DoctorSpaciality> DoctorSpacialities { get; set; }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogImage> BlogImages { get; set; }
        public DbSet<BlogCategory> blogCategories { get; set; }
        public DbSet<AdviceComment> AdviceComments{ get; set; }


        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ApplicationUser> users { get; set; }
        public DbSet<Purchasecart> Purchasecarts { get; set; }
        public DbSet<PurchaseCartItem> purchaseCartItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
