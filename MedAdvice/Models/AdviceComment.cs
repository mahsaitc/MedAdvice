using MedAdvice.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class AdviceComment
    {
        public int id { get; set; }
        public string Userid  { get; set; }
        [ForeignKey("Userid")]
        public ApplicationUser User { get; set; }

        public int AdviceId { get; set; }
        [ForeignKey("AdviceId")]
        public Advice Advice { get; set; }

        public string comment { get; set; }

        public string firstname { get; set; }
        public string Website { get; set; }
        public string lasttname { get; set; }
        public string EmailAdress { get; set; }
    }
}
