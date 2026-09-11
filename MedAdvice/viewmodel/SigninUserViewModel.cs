using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class SigninUserViewModel
    {
        [Required(ErrorMessage = "please enter username!")]
        public string username { get; set; }
        [Required(ErrorMessage = "please enter your password!")]
        public string password { get; set; }
        public Boolean rememberme { get; set; }
    }
}
