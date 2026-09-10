using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class DoctorSpacialityViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "لطفا عنوان تخصص دکتر را وارد کنید")]
        public string SpacialityTitle { get; set; }
        
        public int? DrSpacialityParentId { get; set; }
        
        public IFormFile DrSpacialityPicture { get; set; }
    }
}
