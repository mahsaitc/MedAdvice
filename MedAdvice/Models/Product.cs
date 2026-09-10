using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string englishname { get; set; }
        public string color { get; set; }
        public string weight { get; set; }
        public string size { get; set; }
        public string productmodel { get; set; }
        public int price { get; set; }
        public int count { get; set; }
        public List<ProductImage> productImages { get; set; }
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }
        public int discount { get; set; }

        public string descreption { get; set; }

        public int ProductCategoryId { get; set; }
        [ForeignKey("ProductCategoryId")]
        public ProductCategory productcategory { get; set; }
       


    }
}
