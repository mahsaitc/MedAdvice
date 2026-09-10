using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class AdviceImageViewModel
    {
        public string AdviceImageTitle { get; set; }
        public IFormFile AdviceImage { get; set; }
    }
}
