using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    /// A pinned blog post or medical advice shown in the homepage featured section.
    /// Two optional foreign keys rather than a type discriminator, so the database keeps
    /// referential integrity and a deleted article cannot leave a dangling pin.
    public class FeaturedArticle
    {
        public int Id { get; set; }

        public int? BlogId { get; set; }
        [ForeignKey("BlogId")]
        public Blog Blog { get; set; }

        public int? AdviceId { get; set; }
        [ForeignKey("AdviceId")]
        public Advice Advice { get; set; }

        public int SortOrder { get; set; }
    }
}
