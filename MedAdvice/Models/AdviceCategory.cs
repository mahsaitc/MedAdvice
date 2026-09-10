using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class AdviceCategory
    {
        public int Id { get; set; }
        public string AdviceCategoryname { get; set; }
        public List<Advice> Advices { get; set; }
        public int? AdviceCategoryParentId { get; set; }
        [ForeignKey("AdviceCategoryParentId")]
        
        public AdviceCategory AdviceCategoryParent { get; set; }
        public List<AdviceCategory> AdviceCategoryChildren { get; set; }
        public byte[] AdviceCategoryPicture { get; set; }

    }
}
