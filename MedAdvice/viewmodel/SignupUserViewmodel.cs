using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MedAdvice.viewmodel
{
    public class SignupUserViewmodel
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        [Required(ErrorMessage ="Enter Your UserName")]
       
        [Remote("CheckUsername", "Account", ErrorMessage = "Unfortunately,This Username has been selected before!...")]

        public string username { get; set; }
        public string email { get; set; }
        [Required(ErrorMessage = "Enter Your Password")]
        public string password { get; set; }
        [Compare("password" , ErrorMessage ="Passwords must be equal!") ]
        public string reppasword { get; set; }
        public string phonenumber { get; set; }


    }
}
