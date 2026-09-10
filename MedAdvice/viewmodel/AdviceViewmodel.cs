using MedAdvice.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class AdviceViewmodel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "لطفا عنوان متن را وارد کنید")]
        public string AdviceTitle { get; set; }
        [Required(ErrorMessage = " لطفا تاریخ ایجاد متن را وارد کنید")]
        public string AdviceDate { get; set; }
        [Required(ErrorMessage = "لطفا  متن را وارد کنید")]
        public string AdviceText { get; set; }
        [Required(ErrorMessage = "لطفا خلاصه متن را وارد کنید")]
        public string AdviceBriefText { get; set; }
       
        public int AdviceCategoryId { get; set; }
        public IFormFile AdviceHeaderImage { get; set; }
       
    }
}
