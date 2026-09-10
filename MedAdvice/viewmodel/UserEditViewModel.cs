using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedAdvice.viewmodel
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        /// Login identifier. Shown for reference but never edited: sign-in resolves the
        /// account by user name, so changing it would silently lock the user out.
        public string UserName { get; set; }

        [Required(ErrorMessage = "لطفا ایمیل را وارد کنید")]
        [EmailAddress(ErrorMessage = "ایمیل وارد شده معتبر نیست")]
        public string Email { get; set; }

        [Required(ErrorMessage = "لطفا نام را وارد کنید")]
        public string firstname { get; set; }

        [Required(ErrorMessage = "لطفا نام خانوادگی را وارد کنید")]
        public string lastname { get; set; }

        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public string[] SelectedRoles { get; set; } = new string[0];
        public List<string> AllRoles { get; set; } = new List<string>();

        /// Set by the controller so the view can disable controls the server will refuse.
        public bool IsSelf { get; set; }
        public bool IsLastAdmin { get; set; }
    }
}
