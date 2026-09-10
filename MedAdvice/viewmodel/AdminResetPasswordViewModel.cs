using System.ComponentModel.DataAnnotations;

namespace MedAdvice.viewmodel
{
    public class AdminResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "لطفا رمز عبور جدید را وارد کنید")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "رمزهای عبور یکسان نیستند")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
