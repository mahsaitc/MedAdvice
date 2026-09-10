using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class BlogImageViewModel
    {
       
        public string BlogImageTitle { get; set; }
        public IFormFile Blogimg { get; set; }
       
        
    }
}
