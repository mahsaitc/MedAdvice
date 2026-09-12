using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    /// The reset-by-email flow: the user arrives from a link that already carries the
    /// Identity token, so only the new password is collected.
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "  Enter new password")]
        public string password { get; set; }

        [Compare("password", ErrorMessage = "Passwords are not the same")]
        public string repassword { get; set; }
    }
}
