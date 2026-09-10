using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int MedicalCouncilNo { get; set; }
        public int DrBirthDate { get; set; }
        public string FirstName { get; set; }
        public string FamillyName { get; set; }
        public string DrServiceLocation { get; set; }
        public string CollaborationDate { get; set; }
        public string NationalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Mobilenumber { get; set; }
        public string InstagramId { get; set; }
        public string TwitterId { get; set; }
        public string FacebookId { get; set; }
        public string EmailAdress { get; set; }
        public string DrBriefIntroduction { get; set; }
        public string DrDetails { get; set; }
        public int DrSpacialityId { get; set; }
        [ForeignKey("DrSpacialityId")]
        public DoctorSpaciality DrSpaciality { get; set; }
        public byte[] DrProfileImage { get; set; }
        public List<DoctorImage> DrImages { get; set; }
        

    }
}
