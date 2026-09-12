using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    /// The reset-by-SMS flow. Identical to the email flow apart from the code sent to the
    /// user's phone, which is genuinely required here -- it could not be marked so while one
    /// view model served both flows, so the SMS action checked it by hand instead.
    public class ResetPasswordBySmsViewModel : ResetPasswordViewModel
    {
        [Required(ErrorMessage = "please enter the code sent to your phone.")]
        public string smstoken { get; set; }
    }
}
