using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string BlogTitle { get; set; }
        public string BlogDate { get; set; }
        public string BlogText { get; set; }
        public string BlogBriefText { get; set; }
        public BlogCategory BlogCategory { get; set; }
        public int BlogCategoryId { get; set; }
        [ForeignKey("BlogCategoryId")]

        public byte[] BlogHeaderImage { get; set; }
        public List<BlogImage> BlogImages { get; set; }
    }
}
