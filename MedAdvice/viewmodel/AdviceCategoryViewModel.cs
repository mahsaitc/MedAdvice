using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class AdviceCategoryViewModel
    {
        public int Id { get; set; }
        public string AdviceCategoryname { get; set; }
      
        public int? AdviceCategoryParentId { get; set; }
        
        public IFormFile AdviceCategoryPicture { get; set; }
    }
}
