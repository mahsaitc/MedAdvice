using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "لطفا شماره نظام پزشکی دکتر را وارد کنید")]
        public int MedicalCouncilNo { get; set; }
        [Required(ErrorMessage = "لطفا تاریخ تولد را وارد کنید")]
        public int DrBirthDate { get; set; }
        [Required(ErrorMessage = "لطفا نام را وارد کنید")]
        public string CollaborationDate { get; set; }
        public string FirstName { get; set; }
        [Required(ErrorMessage = "لطفا نام خانوادگی را وارد کنید")]
        public string FamillyName { get; set; }
        
        public string DrServiceLocation { get; set; }
        [Required(ErrorMessage = "لطفا شماره ملی را وارد کنید")]
        public string NationalCode { get; set; }
        [Required(ErrorMessage = "لطفاشماره تلفن را وارد کنید")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "لطفا شماره موبایل را وارد کنید")]
        public string Mobilenumber { get; set; }
        
        public string InstagramId { get; set; }
        public string TwitterId { get; set; }
        public string FacebookId { get; set; }
        [Required(ErrorMessage = "لطفا ادرس ایمیل را وارد کنید")]
        public string EmailAdress { get; set; }
        [Required(ErrorMessage = "لطفا خلاصه متن را وارد کنید")]
        public string DrBriefIntroduction { get; set; }
        public string DrDetails { get; set; }
        
        public int DrSpacialityId { get; set; }
        
        public IFormFile DrProfileImage { get; set; }
      
    }
}
