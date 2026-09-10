using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class DrImageViewModel
    {
       
        public string DoctorImageTitle { get; set; }
        public IFormFile Doctorimg { get; set; }
    }
}
