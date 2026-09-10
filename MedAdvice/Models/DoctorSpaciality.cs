using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class DoctorSpaciality
    {
        public int Id { get; set; }
        public string SpacialityTitle { get; set; }
        public List<Doctor> Doctors { get; set; }
        public int? DrSpacialityParentId { get; set; }
        [ForeignKey("DrSpacialityParentId")]

        public DoctorSpaciality DrSpacialiytParent { get; set; }
        public List<DoctorSpaciality> DrSpacialityChildren { get; set; }
        public byte[] DrSpacialityPicture { get; set; }
    }
}