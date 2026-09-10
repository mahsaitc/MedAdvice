using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public ProductCategory Parent { get; set; }
        public List<ProductCategory> Children { get; set; }
        public List<Product> Products { get; set; }
    }
}
