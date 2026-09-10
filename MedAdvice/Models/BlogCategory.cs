using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class BlogCategory
    {
        public int Id { get; set; }
        public string BlogCategoryname { get; set; }
        public List<Blog> Blogs { get; set; }
        public int? BlogCategoryParentId { get; set; }
        [ForeignKey("BlogCategoryParentId")]

        public BlogCategory BlogCategoryParent { get; set; }
        public List<BlogCategory> BlogCategoryChildren { get; set; }
        public byte[] BlogCategoryPicture { get; set; }
    }
}
