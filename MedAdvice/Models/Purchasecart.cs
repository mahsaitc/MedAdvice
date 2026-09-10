using MedAdvice.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class Purchasecart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public DateTime createdDate { get; set; }

        public List<PurchaseCartItem> PurchaseCartItems { get; set; }

        public bool isOpen { get; set; }
    }
}
