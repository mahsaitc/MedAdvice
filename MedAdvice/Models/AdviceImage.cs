using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class AdviceImage
    {
        public int Id { get; set; }
        public string AdviceImageTitle { get; set; }
        public byte[] Adviceimg { get; set; }

        public int AdviceId { get; set; }
        [ForeignKey("AdviceId")]
        public Advice Advice { get; set; }

    }
}
