using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.viewmodel
{
    public class AdviceCommentViewMoldel
    {
        public int id { get; set; }
        public string? Userid { get; set; }
       
        public int AdviceId { get; set; }
        
        public string comment { get; set; }

        public string firstname { get; set; }
        public string Website { get; set; }
        public string lastname { get; set; }
        public string EmailAdress { get; set; }
    }
}
