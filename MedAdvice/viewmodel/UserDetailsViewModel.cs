using System.Collections.Generic;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Models;

namespace MedAdvice.viewmodel
{
    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsDeactivated { get; set; }
        public bool IsTemporarilyLockedOut { get; set; }

        public int CartCount { get; set; }
        public int CommentCount { get; set; }
        public List<Purchasecart> RecentCarts { get; set; } = new List<Purchasecart>();
        public List<AdviceComment> RecentComments { get; set; } = new List<AdviceComment>();
    }
}
