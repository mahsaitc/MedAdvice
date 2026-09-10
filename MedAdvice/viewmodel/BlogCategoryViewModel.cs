using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class BlogCategoryViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "لطفا عنوان دسته بندی بلاگ را وارد کنید")]
        public string BlogCategoryname { get; set; }
       
        public int? BlogCategoryParentId { get; set; }
      
        public IFormFile BlogCategoryPicture { get; set; }
    }
}
