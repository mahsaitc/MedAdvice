using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class Advice
    {
        public int Id { get; set; }
        public string AdviceTitle { get; set; }
        public string AdviceDate { get; set; }
        public string AdviceText { get; set; }
        public string AdviceBriefText { get; set; }
        public int AdviceCategoryId { get; set; }
        [ForeignKey("AdviceCategoryId")]
        public AdviceCategory AdviceCategory { get; set; }
        public byte[] AdviceHeaderImage { get; set; }
        public List<AdviceImage> AdviceImages { get; set; }
    }
}
