using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class ProductImageViewModel
    {
        public string title { get; set; }
        public IFormFile img { get; set; }
    }
}
