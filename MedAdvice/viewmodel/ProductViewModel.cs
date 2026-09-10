using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "نام کالا وارد نمایید")]
        public string Date { get; set; }
        public string englishname { get; set; }
        [Required(ErrorMessage = "قیمت کالا وارد نمایید")]
        public string color { get; set; }
        public string weight { get; set; }
        public string size { get; set; }
        public string productmodel { get; set; }
        public int price { get; set; }
        public int count { get; set; }
        public int discount { get; set; }
        public string descreption { get; set; }
        public int BrandId { get; set; }
        public int ProductCategoryId { get; set; }

    }
}
