using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class ResetPasswordViewModel
    {
        
        [Required(ErrorMessage = "  Enter new password")]
        public string password { get; set; }

        [Compare("password", ErrorMessage = "Passwords are not the same")]
        public string repassword { get; set; }
        public string smstoken { get; set; }
    }
}
