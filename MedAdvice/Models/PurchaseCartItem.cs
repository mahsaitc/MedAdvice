using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class PurchaseCartItem
    {
        public int Id { get; set; }

        public int PurchaseCartId { get; set; }
        [ForeignKey("PurchaseCartId")]
        public Purchasecart PurchaseCart { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int count { get; set; }
    }
}
