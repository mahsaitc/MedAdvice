using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class BlogImage
    {
        public int Id { get; set; }
        public string BlogImageTitle { get; set; }
        public byte[] Blogimg { get; set; }

        public int BlogId { get; set; }
        [ForeignKey("BlogId")]
        public Blog Blog { get; set; }
    }
}
