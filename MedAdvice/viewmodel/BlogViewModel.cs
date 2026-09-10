using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class BlogViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "لطفا عنوان متن بلاگ را وارد کنید")]
        public string BlogTitle { get; set; }
        public string BlogDate { get; set; }
        public string BlogText { get; set; }
        public string BlogBriefText { get; set; }
        public int BlogCategoryId { get; set; }
        public IFormFile BlogHeaderImage { get; set; }
       
    }
}
